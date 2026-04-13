# Architecture Walkthrough — GMD2 Pokemon

A top-down Pokemon-like built on **MonoGame** in C#. This document walks through the architecture from the ground up, pointing at relevant source code as it goes.

Key concepts covered by this project: a state *stack* for layered game flow, a GUI layer, the Service Locator pattern, a tweening system, a turn-based battle system, and RPG mechanics.

## Read In This Order

Do not try to understand every file in one pass. A much better path is:

1. `Pokemon/Game1.cs` — see the big picture: load content, create the state stack, update, draw.
2. `GMDCore/States/StateStack.cs` and `GMDCore/States/GameStateBase.cs` — understand how game flow is layered.
3. `Pokemon/States/GameStates/PlayState.cs` — see the overworld running as one state.
4. `Pokemon/Entities/Entity.cs`, `Pokemon/States/EntityStates/EntityWalkState.cs`, and `Pokemon/States/PlayerStates/PlayerWalkState.cs` — understand tile movement and encounters.
5. `Pokemon/States/GameStates/BattleState.cs`, `Pokemon/States/GameStates/BattleMenuState.cs`, and `Pokemon/States/GameStates/TakeTurnState.cs` — understand the battle loop and how multiple states cooperate.
6. `Pokemon/Mons/Mon.cs` — understand stats, damage, experience, and leveling.

On a first read, it is completely fine to ignore:

- most of `GMDCore/Graphics`
- the bitmap font internals
- audio implementation details
- the exact tween implementation

Those parts matter, but they are support systems. The main architecture lessons live in the files listed above.

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [The Game Loop](#2-the-game-loop)
3. [The State Stack](#3-the-state-stack)
4. [GUI System](#4-gui-system)
5. [Service Locator Pattern](#5-service-locator-pattern)
6. [Tweening System](#6-tweening-system)
7. [Tile-Based Movement](#7-tile-based-movement)
8. [The Overworld and Random Encounters](#8-the-overworld-and-random-encounters)
9. [The Battle System](#9-the-battle-system)
10. [RPG Mechanics](#10-rpg-mechanics)
11. [Data-Driven Definitions](#11-data-driven-definitions)

---

## 1. Project Structure

```
gmd2-pokemon/
├── GMDCore/               # Reusable engine framework (no game logic)
│   ├── Core.cs            # MonoGame Game subclass — input, letterboxing, Pixel texture
│   ├── Graphics/          # Sprite, AnimatedSprite, TextureAtlas, TileMap, BitmapFont, …
│   ├── GUI/               # Panel, ProgressBar
│   ├── States/            # GameStateBase, StateStack
│   └── Tweening/          # TweenManager, ITweenTask
├── Pokemon/               # Game-specific code
│   ├── Game1.cs           # Top-level game; owns the StateStack
│   ├── GameAssets.cs      # Shared fonts/textures registered in the locator
│   ├── GameSettings.cs    # All magic numbers in one place
│   ├── Locator.cs         # Service Locator (TweenManager, IAudio, GameAssets)
│   ├── Audio/             # IAudio, NullAudio, SoundManager
│   ├── Definitions/       # ContentLoader (JSON + species data → textures/animations)
│   ├── Entities/          # Direction, Entity, Player, AnimationKeys
│   ├── GUI/               # Textbox, Menu, Selection
│   ├── Input/             # GameController (keyboard → game actions)
│   ├── Mons/              # Mon, Party, PokemonSpecies, PokemonDefinitions
│   ├── Battle/            # BattleSprite, Opponent
│   ├── States/
│   │   ├── GameStates/    # StartState, PlayState, BattleState, BattleMenuState,
│   │   │                  # TakeTurnState, BattleMessageState, FadeState, DialogueState
│   │   ├── EntityStates/  # EntityStateBase, EntityWalkState, EntityIdleState
│   │   └── PlayerStates/  # PlayerIdleState, PlayerWalkState
│   └── World/             # Level
└── Tools/
    └── GenerateFontAtlas.py   # Dev utility — regenerates the bitmap font atlas PNGs
```

The split between `GMDCore` and `Pokemon` is intentional: `GMDCore` knows nothing about Pokemon. The engine provides window management, a virtual-resolution scaler, input tracking, sprite and animation primitives, a state stack, a tween system, and GUI building blocks. Everything Pokemon-specific lives in the `Pokemon` project.

That split is one of the main architecture lessons in the project:

- `GMDCore` is the reusable engine-like layer.
- `Pokemon` is the game layer built on top of it.
- If a class talks about Pokemon, grass, battles, or leveling, it belongs in `Pokemon`.
- If a class could be reused in a different game, it probably belongs in `GMDCore`.

---

## 2. The Game Loop

### Layer 1 — `Core` (`GMDCore/Core.cs`)

`Core` manages the window, the virtual resolution scaler, and the per-frame input snapshot. It also creates a shared 1×1 white `Pixel` texture used throughout the GUI system:

```csharp
// Core.cs
public static Texture2D Pixel { get; private set; }

protected override void Initialize()
{
    Pixel = new Texture2D(GraphicsDevice, 1, 1);
    Pixel.SetData(new[] { Color.White });
    // ...
}
```

Any class can draw a filled rectangle by calling `spriteBatch.Draw(Core.Pixel, rect, color)` without needing to create or pass its own texture.

### Layer 2 — `Game1` (`Pokemon/Game1.cs`)

`Core` refreshes input first, then hands control to `Game1`, which updates **tweens before states** so that property changes triggered by tween callbacks are visible to the state stack in the same frame.

```csharp
// Core.cs
protected sealed override void Update(GameTime gameTime)
{
    Input.Update();                  // gameplay reads fresh input every frame
    UpdateGame(gameTime);
    base.Update(gameTime);
}

// Game1.cs
protected override void UpdateGame(GameTime gameTime)
{
    Locator.Tweens.Update(gameTime);   // tweens fire first
    StateStack.Update(gameTime);
}
```

`Game1` then draws the whole game into a **`RenderTarget2D`** at the virtual resolution and scales that final image into `DestinationRectangle`, a centered rectangle that preserves the virtual aspect ratio inside the real window.

```csharp
// Game1.cs
GraphicsDevice.SetRenderTarget(_renderTarget);
GraphicsDevice.Clear(Color.Black);
StateStack.Draw(SpriteBatch);
GraphicsDevice.SetRenderTarget(null);

SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
SpriteBatch.Draw(_renderTarget, DestinationRectangle, Color.White);
SpriteBatch.End();
```

**Full call chain per frame:**

```
Core.Update
  └─ Game1.UpdateGame
       ├─ Locator.Tweens.Update        ← tween callbacks may push/pop states
       └─ StateStack.Update
            └─ (top state).Update      ← e.g. PlayState, BattleState, FadeState
```

This order matters:

- input must be updated before game code checks keys
- tweens run before states so finished animations can change state immediately
- only then does the top state read input and decide what happens next

---

## 3. The State Stack

Swapping a single active state works for simple linear flows, but breaks down once you need states to coexist — a fade overlay on top of a running game scene, or a menu on top of a battle. The **state stack** solves this by allowing states to be layered on top of each other.

```csharp
// GMDCore/States/StateStack.cs
public void Push(GameStateBase state) { ... }   // push and Enter the new state
public void Pop()                     { ... }   // Exit and remove the top state
public void Update(GameTime gameTime) { ... }   // only the top state updates
public void Draw(SpriteBatch sb)      { ... }   // all states draw, bottom to top
```

Only the top state receives `Update`, so it has exclusive control of input. All states draw, so underlying states remain visible behind overlays. Each state still draws in the same virtual coordinate system through `Core.BeginDraw`; `Game1` handles scaling afterward by drawing the final render target.

This enables patterns that a single-state machine cannot express:

- **Fade transitions** — `FadeState` is pushed on top of the current state, tweens an opacity overlay from 0→1, fires a callback, then pops itself. The game world beneath stays rendered throughout.
- **Dialogue overlays** — `BattleMessageState` sits on top of `BattleState` and only pops itself when the player confirms. `BattleState.Draw` still runs every frame behind it.
- **Chained battle phases** — `BattleMenuState` → `TakeTurnState` → `BattleMessageState` → back to `BattleMenuState` are all push/pop operations on the same stack. No state needs to know about the others.

**State stack during a battle:**

```
Bottom ──► PlayState          (draws the overworld in the background)
           BattleState        (draws the battle arena)
           BattleMenuState    (draws the Fight / Run menu) ◄── gets Update
```

When the player selects Fight:

```
           PlayState
           BattleState
           TakeTurnState      (executes the turn, pops itself when done)
           BattleMessageState (shows the current attack message) ◄── gets Update
```

---

## 4. GUI System

The GUI layer provides reusable widgets for menus, dialogue, and stat displays.

The guiding idea is to build a few tiny widgets and then combine them:

- `Panel` draws a box
- `Textbox` combines a `Panel` with wrapped text and paging
- `Selection` handles moving a cursor through a list of choices
- `Menu` combines a `Panel` with a `Selection`

### `BitmapFont` — `GMDCore/Graphics/BitmapFont.cs`

A pixel-perfect bitmap font backed by a pre-generated glyph atlas. The atlas covers printable ASCII (32–126) in 16-column rows. Per-character advance widths are stored in a static table, giving the font variable-width character spacing without runtime font rendering.

```csharp
Locator.Assets.MediumFont.Draw(spriteBatch, "Hello!", position, Color.White);
Vector2 size = Locator.Assets.MediumFont.MeasureString("Hello!");
```

Three sizes (`SmallFont`, `MediumFont`, `LargeFont`) are loaded once in `Game1` and registered in `Locator.Assets`.

The font images and the character-width data hardcoded in `BitmapFont.cs` were not written by hand — they were produced by `Tools/GenerateFontAtlas.py`. The script takes a `.ttf` font file, renders each character into a spritesheet, and prints the width of every character so the font knows how far to advance after drawing each one. If you ever want to use a different font, run the script and copy the printed widths back into `BitmapFont.cs`.

### `Panel` — `GMDCore/GUI/Panel.cs`

A filled white rectangle with a black outline, drawn using `Core.Pixel`. Used as the background for all UI boxes.

### `ProgressBar` — `GMDCore/GUI/ProgressBar.cs`

A filled bar scaled by `Value / Max`, with a black outline. Used for health bars and the EXP bar in battle.

```csharp
// BattleState.cs
PlayerHealthBar = new ProgressBar(x, y, 152, 6, HpColor, pokemon.CurrentHp, pokemon.Hp);
// TakeTurnState.cs — tween the bar down as HP drops
Locator.Tweens.Tween(GameSettings.HpTweenDuration)
    .Add(v => _battle.PlayerHealthBar.Value = v, currentHp, newHp);
```

### `Textbox` — `Pokemon/GUI/Textbox.cs`

Displays a string of text inside a `Panel`, word-wrapping to fit the box width and splitting into pages that the player advances with Confirm. Used for all dialogue and battle messages.

### `Selection` / `Menu` — `Pokemon/GUI/Selection.cs`, `Menu.cs`

`Selection` is a vertical list widget. Each item is a `MenuItem` record with a label and an `Action` callback:

```csharp
new("Fight", OnFightSelected),
new("Run",   OnRunSelected)
```

`Selection` handles `MenuUp` / `MenuDown` navigation and cursor rendering. `Menu` wraps a `Selection` with a `Panel` background for convenience. Adding a new menu option is one line — the widget handles all layout and input automatically.

---

## 5. Service Locator Pattern

A common approach to audio in games is a static singleton (`SoundManager.PlayMusic()`). That works, but hides dependencies — nothing in a class's signature tells you it uses audio. The **Service Locator** pattern keeps the convenience of global access while making registration explicit and allowing the concrete implementation to be swapped out.

```csharp
// Pokemon/Locator.cs
public static class Locator
{
    public static ITweenManager Tweens { get; private set; } = new TweenManager();
    public static IAudio        Audio  { get; private set; } = new NullAudio();
    public static GameAssets    Assets { get; private set; } = null!;

    public static void Provide(ITweenManager tweens) => Tweens = tweens;
    public static void Provide(IAudio audio)         => Audio  = audio;
    public static void Provide(GameAssets assets)    => Assets = assets;
}
```

Any class calls `Locator.Audio.PlayHit()`, `Locator.Tweens.After(1f, callback)`, or `Locator.Assets.SmallFont.Draw(...)` without knowing or caring who loaded those objects. That keeps the project on one shared-access pattern instead of a set of `Game1` statics.

You can think of `Locator` as the game's shared toolbox:

- audio service
- tween manager
- shared loaded assets

This is not the only way to structure a game, but it keeps small gameplay classes approachable because they do not need large constructors just to receive a font, an audio service, a tween manager, etc..

### The Null Object

Before `Game1.LoadContent` registers a real `SoundManager`, `Locator.Audio` holds a `NullAudio` — an implementation of `IAudio` where every method is a no-op:

```csharp
// Pokemon/Audio/NullAudio.cs
public sealed class NullAudio : IAudio
{
    public void PlayFieldMusic()  { }
    public void PlayBattleMusic() { }
    public void PlayHit()         { }
    // ... all others empty
}
```

This means any code that calls `Locator.Audio` before the service is registered silently does nothing instead of throwing a null reference exception. There are no null checks at call sites.

Registration happens in `Game1.LoadContent`:

```csharp
Locator.Provide(new GameAssets(
    smallFont, mediumFont, largeFont,
    tileAtlas, entityAtlas, cursorTex, shadowTex));

var audio = new SoundManager();
audio.LoadContent(Content);
Locator.Provide(audio);
```

---

## 6. Tweening System

Tweens are lightweight, time-based property animations managed by `TweenManager` (`GMDCore/Tweening/TweenManager.cs`). They run on the game thread — callbacks are safe to push/pop states or mutate game objects.

`TweenManager` provides three task types:

### `Tween` — animate one or more floats over a duration

```csharp
// Slide the player sprite from off-screen to its battle position
Locator.Tweens.Tween(GameSettings.BattleSlideInDuration)
    .Add(v => PlayerSprite.X   = v, startX, targetX)
    .Add(v => _playerShadowX   = v, startShadowX, targetShadowX)
    .Finish(() => ShowStartingDialogue());
```

Multiple properties tween in parallel within one group. `.Finish` fires once when all properties have reached their target.

### `After` — fire a callback after a delay

```csharp
Locator.Tweens.After(move.PauseBeforeAttack, () =>
{
    // ... begin the tackle animation
});
```

### `Every` — repeat a callback at a fixed interval

```csharp
// Cycle through Pokemon sprites on the title screen every 3 seconds
Locator.Tweens.Every(3f, CycleSprite);

// Blink the hit sprite a fixed number of times
Locator.Tweens.Every(GameSettings.AttackBlinkInterval, ToggleBlink)
    .Limit(GameSettings.AttackBlinkCount)
    .Finish(OnBlinkDone);
```

Tweens are registered in a pending list during the frame and flushed at the start of the next `Update`, so callbacks that create new tweens never corrupt the active list.

---

## 7. Tile-Based Movement

### Dual Coordinate System

Pokemon uses a **grid-first** movement model. Every entity has two positions:

```csharp
// Pokemon/Entities/Entity.cs
public int   MapX { get; set; }   // which tile (grid)
public int   MapY { get; set; }
public float X    { get; set; }   // pixel position (tweened between tiles)
public float Y    { get; set; }
```

`MapX` / `MapY` are the authoritative grid position used for tile lookups and collision. `X` / `Y` are the *visual* position smoothly interpolated between tiles by the tween system. This is the correct model for grid-based games: the entity is always on one tile logically, but moves smoothly between tiles visually.

### `EntityWalkState` — `Pokemon/States/EntityStates/EntityWalkState.cs`

`Enter` is called once when the walk begins. `AttemptMove` computes the tile directly in front of the entity, checks whether it is within the map boundary, lets subclasses inspect that destination tile, then:

1. Optionally reacts to the destination tile before movement is committed.
2. Updates `MapX` / `MapY` to the target tile immediately.
3. Schedules a tween that moves `X` / `Y` from the current pixel position to the new pixel position over `WalkTweenDuration` seconds.
4. Calls `OnMovementComplete` when the tween finishes.

```csharp
Point destination = GetDestination();
float targetX = destination.X * GameSettings.TileSize;
float targetY = destination.Y * GameSettings.TileSize - Entity.Height / 2f;

Locator.Tweens.Tween(GameSettings.WalkTweenDuration)
    .Add(v => Entity.X = v, Entity.X, targetX)
    .Add(v => Entity.Y = v, Entity.Y, targetY)
    .Finish(OnMovementComplete);
```

The Y target subtracts half the entity's height to visually center the sprite on its tile.

### `PlayerWalkState` — `Pokemon/States/PlayerStates/PlayerWalkState.cs`

Extends `EntityWalkState`. `OnMovementComplete` checks `GameController.MovementDirection` — if a direction key is still held, it creates a new `PlayerWalkState` immediately (chaining movement), otherwise it transitions to `PlayerIdleState`.

```csharp
protected override void OnMovementComplete()
{
    Direction? dir = GameController.MovementDirection;

    if (dir.HasValue)
    {
        Entity.Direction = dir.Value;
        Entity.ChangeState(new PlayerWalkState(_player, Level, _stateStack));
    }
    else
    {
        Entity.ChangeState(new PlayerIdleState(_player, Level, _stateStack));
    }
}
```

---

## 8. The Overworld and Random Encounters

### `Level` — `Pokemon/World/Level.cs`

The overworld is two `TileMap` layers (base terrain and tall grass) plus the player entity. The map is generated procedurally each session: the base layer is filled with random grass tile variants; the tall grass layer only covers rows from `TallGrassStartRow` downward.

### Random Encounters

Random encounters are checked against the destination tile in `PlayerWalkState.BeforeMove`, i.e. stepping into tall grass can trigger a battle.

```csharp
protected override bool BeforeMove(Point destination)
{
    int tileId = Level.GrassLayer.GetTile(destination.X, destination.Y);
    if (tileId != GameSettings.TileTallGrass) return true;
    if (!RollEncounter()) return true;

    // freeze the player, pause music, fade to battle
    Locator.Audio.PauseFieldMusic();
    Locator.Audio.PlayBattleMusic();

    _stateStack.Push(new FadeState(_stateStack, Color.White, GameSettings.FadeDuration, 0f, 1f,
        () =>
        {
            _stateStack.Push(new BattleState(_player, _stateStack));
            _stateStack.Push(new FadeState(_stateStack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
        }));

    return false;
}
```

The encounter triggers a **fade-out** → **push BattleState** → **fade-in** sequence entirely through state stack operations. `FadeState` tweens a full-screen color overlay, fires its callback at peak opacity (when nothing is visible), and pops itself when done.

---

## 9. The Battle System

The battle system is a sequence of states layered on the stack, each responsible for one phase.

### `BattleState` — `Pokemon/States/GameStates/BattleState.cs`

Owns the battle scene: two `BattleSprite` instances, three `ProgressBar` widgets (player HP, opponent HP, EXP), and the bottom `Panel`. On first `Update` it triggers the **slide-in**: both sprites tween in from opposite edges of the screen while their shadow ellipses track them:

```csharp
Locator.Tweens.Tween(GameSettings.BattleSlideInDuration)
    .Add(v => PlayerSprite.X   = v, PlayerSprite.X,   32f)
    .Add(v => OpponentSprite.X = v, OpponentSprite.X, GameSettings.VirtualWidth - 96f)
    .Add(v => _playerShadowX   = v, _playerShadowX,   66f)
    .Add(v => _opponentShadowX = v, _opponentShadowX, GameSettings.VirtualWidth - 70f)
    .Finish(() =>
    {
        RenderHealthBars = true;
        ShowStartingDialogue();
    });
```

`BattleState` exposes its sprites and progress bars as public properties so `TakeTurnState` can animate them without needing its own references. It is the persistent backdrop for the whole fight. It keeps drawing the scene while other states sit on top of it.

### `BattleMenuState` — `Pokemon/States/GameStates/BattleMenuState.cs`

Displays the Fight / Run `Menu` and waits for selection. Selecting Run pushes a `BattleMessageState` ("You fled successfully!"), then chains fade + stack cleanup to return to the overworld.

### `TakeTurnState` — `Pokemon/States/GameStates/TakeTurnState.cs`

Executes one full battle round. Faster Pokemon (by Speed stat) attacks first. Each attack is a chain of tween callbacks:

```
After(pause) → tween sprite lunge → tween lunge back
             → PlayHit sound
             → blink defender (Every + Limit)
             → tween HP bar down
             → check for faint / victory
```

Everything is driven by `Locator.Tweens` — no `Update` loop, no frame counters. When the sequence ends, `TakeTurnState` pops itself and pushes `BattleMenuState` for the next turn, or pushes `BattleMessageState` for a faint/victory outcome.

This is a nice example of separating **scene ownership** from **event sequencing**:

- `BattleState` owns the scene and HUD
- `BattleMenuState` owns player choice
- `BattleMessageState` owns temporary text overlays
- `TakeTurnState` owns the order of events in one turn

Each class stays smaller because it has one main reason to change.

**Battle state diagram:**

```
BattleState (persistent, always draws)
  │
  ├─► BattleMenuState  ──(Fight)──►  TakeTurnState  ──(survived)──►  BattleMenuState
  │         │                              │
  │       (Run)                      (faint / win)
  │         │                              │
  └─────────▼──────────────────────────────▼
          FadeState → overworld
```

---

## 10. RPG Mechanics

### `PokemonSpecies` — `Pokemon/Mons/PokemonSpecies.cs`

A data record describing a species: name, base stats, individual values (IVs, 1–5 per stat), and battle sprite paths. Loaded from JSON at startup — see section 11.

### `Mon` — `Pokemon/Mons/Mon.cs`

A runtime Pokemon instance. Stats are calculated from the species definition at construction by rolling level-up gains from level 1 up to the target level:

```csharp
// Mon.cs — constructor
for (int i = 0; i < level; i++)
    RollStatsLevelUp();

CurrentHp = Hp;
```

`RollStatsLevelUp` applies the IV-based growth formula: each stat is tested 3 times against a d6 — if the roll falls within the IV range (1–5), the stat increases by 1. Higher IVs mean faster growth.

```csharp
for (int i = 0; i < 3; i++) if (rng.Next(1, 7) <= HpIV)      { Hp++;      hpGain++;  }
for (int i = 0; i < 3; i++) if (rng.Next(1, 7) <= AttackIV)  { Attack++;  atkGain++; }
// ...
```

### Damage Calculation

```csharp
// Mon.cs
public int CalcDamageTo(Mon defender, Move move)
{
    int damage = (Attack * move.BasePower / 10) - defender.Defense;
    return Math.Max(1, damage);
}
```

Attack power scaled by move power minus Defense, minimum 1.

### Experience and Levelling Up

```csharp
// Mon.cs
private static int CalcExpToLevel(int level) => (int)(level * level * 5 * 0.75f);

public (int hpGain, int atkGain, int defGain, int spdGain) LevelUp()
{
    Level++;
    ExpToLevel = CalcExpToLevel(Level);
    return RollStatsLevelUp();
}
```

EXP required grows quadratically with level. `LevelUp` returns the stat gains as a tuple so `TakeTurnState` can display them (e.g. "Level Up! HP+2 ATK+1 DEF+0 SPD+1") without needing to inspect `Mon` state before and after.

### Exp Reward

When a Pokemon is defeated it awards EXP to the winner. The amount is calculated directly on `Mon`:

```csharp
// Mon.cs
public int ExpReward => (HpIV + AttackIV + DefenseIV + SpeedIV) * Level;
```

A Pokemon with higher IVs and a higher level is worth more EXP. The battle state reads this property without knowing the formula.

### `Party` — `Pokemon/Mons/Party.cs`

Holds a read-only list of `Mon` instances and exposes `Current` as the active Pokemon:

```csharp
public Mon Current => _pokemon.Count > 0
    ? _pokemon[0]
    : throw new InvalidOperationException("Party is empty.");
```

---

## 11. Data-Driven Definitions

The rule is: **content lives in JSON, behaviour lives in C#**.

### Pokemon Species — `pokemon_definitions.json`

```json
[
  {
    "name": "Aardart",
    "baseHp": 14, "baseAttack": 9, "baseDefense": 5, "baseSpeed": 6,
    "hpIV": 3, "attackIV": 4, "defenseIV": 2, "speedIV": 3,
    "battleSpriteFront": "images/pokemon/aardart-front",
    "battleSpriteBack":  "images/pokemon/aardart-back"
  },
  ...
]
```

`PokemonDefinitions.LoadContent` deserializes this into `List<PokemonSpecies>` using `System.Text.Json`. No XML dependencies, no manual parsing — the deserializer maps camelCase JSON fields directly to PascalCase C# properties via `PropertyNameCaseInsensitive = true`.

### Entity Animations — `entity_animations.json`

```json
{
  "interval": 0.15,
  "animations": [
    { "name": "walk-down",  "frames": [0, 1, 2, 1] },
    { "name": "idle-down",  "frames": [1] },
    ...
  ]
}
```

`ContentLoader.CreateEntityAnimations` turns these into `Dictionary<string, Animation>` keyed by name. Frame indices are 0-based and map directly to `TextureAtlas` regions — no off-by-one translation needed.

Both files are deserialized into private `record` types that exactly mirror the JSON shape:

```csharp
// ContentLoader.cs
private record EntityAnimationsFile(double Interval, List<AnimationEntry> Animations);
private record AnimationEntry(string Name, int[] Frames);
```

Records are ideal here: they are pure data containers with no behaviour, and their positional constructors match the deserialized properties automatically.
