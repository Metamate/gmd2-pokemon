# Feature Walkthrough — GMD2 Pokemon

The goal of this walkthrough is not to explain every line. The goal is to help answer:

- what problem is this feature solving?
- which files are the important ones?
- what is the key control flow?
- what are the main design ideas worth learning?

If you are reading the code for the first time, use this file together with `README.md`.
`README.md` gives the big architectural picture. This file slows down and walks feature by feature.

---

## 1. Game Loop and Rendering

### What this feature does

This feature sets up the window, tracks input every frame, and renders the game at a fixed virtual resolution before scaling it into the real window.

### Important files

- `GMDCore/Core.cs`
- `Pokemon/Game1.cs`
- `Pokemon/GameSettings.cs`

### Big idea

The game logic thinks in terms of a small virtual resolution (`384x216`), not the real window size. That keeps coordinates simple and predictable.

The whole frame is first drawn into a `RenderTarget2D`, then that finished image is scaled into a centered destination rectangle in the window.

This gives us:

- consistent virtual coordinates
- aspect-ratio preservation
- letterboxing or pillarboxing when needed
- fewer pixel-art artifacts than scaling every draw call separately

### Important code

#### `Core.Update`

`Core` owns the per-frame order for input and game logic:

```csharp
protected sealed override void Update(GameTime gameTime)
{
    Input.Update();
    UpdateGame(gameTime);
    base.Update(gameTime);
}
```

The important lesson here is that input is refreshed before gameplay code reads it.

#### `Core.UpdatePresentation`

This computes `DestinationRectangle`, the area of the real window where the final image should be drawn.

If the window aspect ratio does not match the virtual resolution, the rectangle is centered and black bars appear around it.

#### `Game1.Draw`

The game is rendered in two steps:

```csharp
GraphicsDevice.SetRenderTarget(_renderTarget);
GraphicsDevice.Clear(Color.Black);
StateStack.Draw(SpriteBatch);
GraphicsDevice.SetRenderTarget(null);

SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
SpriteBatch.Draw(_renderTarget, DestinationRectangle, Color.White);
SpriteBatch.End();
```

This is a common pixel-art rendering pattern because it keeps gameplay drawing simple and scales only one finished image.

### Why this design is good for students

- The game uses one coordinate system for gameplay.
- Window resizing is handled in one place.
- The rendering pipeline is explicit without being too clever.

---

## 2. State Stack

### What this feature does

The state stack controls game flow.

Instead of replacing one active state with another, the game can layer states on top of each other.

### Important files

- `GMDCore/States/StateStack.cs`
- `GMDCore/States/GameStateBase.cs`
- `Pokemon/States/GameStates/*.cs`

### Big idea

Only the top state updates, but all states draw.

That means:

- menus can appear over gameplay
- fade overlays can appear over any scene
- dialogue boxes can appear over battles

### Important code

#### `StateStack.Update`

```csharp
public void Update(GameTime gameTime)
{
    if (_states.Count > 0)
        _states[^1].Update(gameTime);
}
```

Only the top-most state gets input and game logic.

#### `StateStack.Draw`

```csharp
public void Draw(SpriteBatch spriteBatch)
{
    foreach (var state in _states)
        state.Draw(spriteBatch);
}
```

Everything on the stack is rendered from bottom to top.

### Example

During battle, the stack can look like this:

```text
PlayState
BattleState
BattleMenuState
```

If the player attacks, it can temporarily become:

```text
PlayState
BattleState
TakeTurnState
BattleMessageState
```

That is the key reason a stack is more flexible than a single-state machine here.

### Why this design is good for students

- The rule is easy to remember: top updates, all draw.
- Overlay behavior becomes natural instead of special-cased.
- It demonstrates a useful game-architecture pattern very clearly.

---

## 3. Start Screen and Transitions

### What this feature does

The start screen plays intro music, cycles through random monster sprites, and transitions into gameplay with a fade.

### Important files

- `Pokemon/States/GameStates/StartState.cs`
- `Pokemon/States/GameStates/FadeState.cs`

### Big idea

Transitions are just states too.

Rather than putting fade logic inside every other state, the game pushes a `FadeState` on top of whatever is already running.

### Important code

#### Sprite carousel

`StartState` uses `Locator.Tweens.Every(3f, CycleSprite)` to move one sprite out and the next one in.

#### Starting the game

When the player confirms:

1. a white `FadeState` is pushed
2. `StartState` is popped
3. `PlayState` is pushed
4. a `DialogueState` is pushed
5. another fade state removes the white overlay

This is a good example of the state stack acting like a little script.

---

## 4. UI System

### What this feature does

The UI system provides reusable building blocks for dialogue, menus, and bars.

### Important files

- `GMDCore/GUI/Panel.cs`
- `GMDCore/GUI/ProgressBar.cs`
- `Pokemon/GUI/Textbox.cs`
- `Pokemon/GUI/Selection.cs`
- `Pokemon/GUI/Menu.cs`
- `Pokemon/GUI/Layout.cs`

### Big idea

The UI is built by composition:

- `Panel` draws a box
- `Textbox` is a `Panel` plus wrapped/paged text
- `Selection` handles choice navigation
- `Menu` is a `Panel` plus a `Selection`

### Important code

#### `Textbox`

`Textbox` wraps a long string into lines, shows only a few lines per page, and advances when the player presses confirm.

This is a good example of a small class doing one complete UI job.

#### `Selection`

Each menu item is a pair:

```csharp
new("Fight", OnFightSelected)
```

That means the menu stores both the label and the code that should run when the player chooses it.

### Why this design is good for students

- Small classes with clear responsibilities
- Reuse by combining pieces instead of subclassing everything
- Easy to extend with new menus and dialogue

---

## 5. Service Locator

### What this feature does

The service locator gives global access to a small set of shared systems:

- tweens
- audio
- shared loaded assets

### Important files

- `Pokemon/Locator.cs`
- `Pokemon/GameAssets.cs`
- `Pokemon/Audio/IAudio.cs`
- `Pokemon/Audio/NullAudio.cs`
- `Pokemon/Audio/SoundManager.cs`

### Big idea

This project uses a service locator because it keeps small gameplay classes easy to read.

Instead of passing audio, tweening, fonts, cursor textures, and shadow textures through many constructors, states can use:

- `Locator.Audio`
- `Locator.Tweens`
- `Locator.Assets`

### Important code

#### `NullAudio`

Before the real audio system is registered, `Locator.Audio` points at `NullAudio`.

That means this call is always safe:

```csharp
Locator.Audio.PlayHit();
```

No null checks are needed.

### Tradeoff to understand

This pattern is convenient, but it does hide dependencies. That is worth discussing with students:

- good: simpler gameplay code
- cost: dependencies are less visible in constructors

That makes it a useful teaching example rather than just a convenience trick.

---

## 6. Tweening

### What this feature does

The tween manager handles:

- animating float values over time
- firing callbacks after delays
- repeating callbacks at intervals

### Important files

- `GMDCore/Tweening/TweenManager.cs`
- `GMDCore/Tweening/ITweenManager.cs`

### Big idea

The tween system is used as the main sequencing tool for animations and battle events.

The three main tools are:

- `Tween(duration)` for interpolation
- `After(delay, callback)` for one delayed action
- `Every(interval, callback)` for repeated timed actions

### Important code

#### Tile movement

```csharp
Locator.Tweens.Tween(GameSettings.WalkTweenDuration)
    .Add(v => Entity.X = v, Entity.X, targetX)
    .Add(v => Entity.Y = v, Entity.Y, targetY)
    .Finish(OnMovementComplete);
```

#### Fade

```csharp
Locator.Tweens.Tween(duration)
    .Add(v => _opacity = v, fromOpacity, toOpacity)
    .Finish(() =>
    {
        _stack.Pop();
        onComplete();
    });
```

#### Repeating blink

```csharp
Locator.Tweens.Every(GameSettings.AttackBlinkInterval,
    () => defenderSprite.Blinking = !defenderSprite.Blinking)
    .Limit(GameSettings.AttackBlinkCount)
    .Finish(() => defenderSprite.Blinking = false);
```

### Why this design is good for students

- The public API is small.
- Time-based behavior stays out of random `Update` counters.
- It shows how asynchronous-looking game flow can still be organized clearly.

---

## 7. Tile Movement

### What this feature does

The player moves on a tile grid, but movement is drawn smoothly between tiles.

### Important files

- `Pokemon/Entities/Entity.cs`
- `Pokemon/States/EntityStates/EntityWalkState.cs`
- `Pokemon/States/PlayerStates/PlayerWalkState.cs`
- `Pokemon/States/PlayerStates/PlayerIdleState.cs`

### Big idea

Each entity has two positions:

- `MapX`, `MapY` for logical tile position
- `X`, `Y` for visual pixel position

This is one of the most important architecture ideas in the project.

### Important code

#### `EntityWalkState`

The flow is:

1. compute the tile in front of the entity
2. stop if it is outside the map
3. let subclasses inspect the destination tile
4. update logical position immediately
5. tween visual position to match
6. call `OnMovementComplete`

That separation makes tile-based game rules much easier to implement.

#### `PlayerWalkState`

`PlayerWalkState` overrides `BeforeMove` so it can trigger encounters when the destination tile is tall grass.

That means “step into grass” and “maybe start battle” happen at exactly the right moment.

---

## 8. Overworld and Encounters

### What this feature does

The overworld builds the field, updates the player, and triggers random encounters.

### Important files

- `Pokemon/World/Level.cs`
- `Pokemon/States/GameStates/PlayState.cs`
- `Pokemon/States/PlayerStates/PlayerWalkState.cs`

### Big idea

The overworld scene is just:

- base tile layer
- tall grass layer
- player

That simplicity is good. It keeps the play state focused on flow rather than world simulation complexity.

### Important code

#### Encounter flow

When the player steps into tall grass and the random roll succeeds:

1. movement is cancelled
2. field music pauses and battle music starts
3. a white fade is pushed
4. `BattleState` is pushed
5. another fade removes the overlay

This is another nice example of the stack acting like a script.

---

## 9. Battle Scene

### What this feature does

`BattleState` owns the battle scene itself:

- monster sprites
- shadows
- HP bars
- EXP bar
- lower panel

### Important files

- `Pokemon/States/GameStates/BattleState.cs`
- `Pokemon/Battle/BattleSprite.cs`
- `GMDCore/GUI/ProgressBar.cs`

### Big idea

`BattleState` is the persistent scene under the rest of the battle flow.

Other states come and go on top of it, but `BattleState` keeps drawing the battle.

### Important code

#### Battle intro

When the state first updates, it starts a slide-in tween for:

- player sprite
- opponent sprite
- player shadow
- opponent shadow

When that tween finishes, health bars appear and intro messages are pushed.

That is a good example of using a tween completion callback to move to the next step of the scene.

---

## 10. Battle Menu and Messages

### What this feature does

These states handle temporary battle overlays:

- choosing Fight or Run
- showing short battle messages

### Important files

- `Pokemon/States/GameStates/BattleMenuState.cs`
- `Pokemon/States/GameStates/BattleMessageState.cs`

### Big idea

The menu and messages do not need to know how to draw the whole battle scene. They rely on `BattleState` still being underneath them on the stack.

This keeps them small and focused.

### Important code

#### Running away

The Run option:

1. removes the menu
2. shows a “fled successfully” message
3. waits briefly
4. fades out
5. removes battle states
6. resumes field music

That is a good teaching example of layered state flow.

---

## 11. Turn-Based Sequence

### What this feature does

`TakeTurnState` runs one whole battle round.

### Important files

- `Pokemon/States/GameStates/TakeTurnState.cs`
- `Pokemon/Mons/Move.cs`
- `Pokemon/Mons/Mon.cs`

### Big idea

This state is mostly a sequencer. It does not draw anything itself.

Its job is to decide the order of events and schedule them:

- who goes first
- when to show messages
- when to lunge
- when to blink
- when to apply damage
- when to check victory or fainting

### Important code

#### Speed decides order

At construction time, the state compares `Speed` and stores:

- first attacker
- second attacker
- matching sprites
- matching bars

That avoids repeating those decisions later.

#### Attack sequence

Each attack goes through:

1. push attack message
2. wait briefly
3. lunge attacker
4. move attacker back
5. blink defender
6. animate HP bar
7. commit final HP value
8. continue to the next step

This is a clean example of turn-based flow without a huge phase enum or giant switch statement.

---

## 12. RPG Model

### What this feature does

The RPG layer models:

- species data
- runtime monster instances
- parties
- damage
- experience
- leveling

### Important files

- `Pokemon/Mons/PokemonSpecies.cs`
- `Pokemon/Mons/PokemonDefinitions.cs`
- `Pokemon/Mons/Mon.cs`
- `Pokemon/Mons/Party.cs`
- `Pokemon/Mons/Move.cs`

### Big idea

The battle states do not own RPG formulas. The model does.

That is an important architecture decision:

- `Mon` knows how to calculate damage
- `Mon` knows how to level up
- `Mon` knows how much EXP it gives

The battle code asks questions and displays results. It does not own the formulas.

### Important code

#### IV-based growth

Each level-up rolls each stat three times against a d6. Higher IVs make growth more likely.

That is a nice educational compromise:

- more interesting than flat stat growth
- still simple enough to understand and tweak

#### Damage

Damage depends on:

- attacker Attack
- move BasePower
- defender Defense

with a minimum of 1 damage.

#### Party

`Party.Current` simply means “the first monster in the list.”

That keeps the battle system simple because this demo only supports one active monster.

---

## 13. Data-Driven Content

### What this feature does

Species definitions and animation definitions are loaded from JSON instead of being hardcoded in logic.

### Important files

- `Pokemon/Content/data/pokemon_definitions.json`
- `Pokemon/Content/data/entity_animations.json`
- `Pokemon/Mons/PokemonDefinitions.cs`
- `Pokemon/Definitions/ContentLoader.cs`

### Big idea

This project uses a useful rule:

- data belongs in files
- behavior belongs in code

That means students can change monster stats or animation frame lists without editing gameplay logic.

### Important code

`PokemonDefinitions.LoadContent` deserializes the species list once.

`ContentLoader.CreateEntityAnimations` turns animation JSON into a dictionary of `Animation` objects keyed by names like:

- `walk-down`
- `walk-left`
- `idle-up`

That is a clean example of data-driven design without adding too much abstraction.

---

## 14. Suggested Reading Order by Topic

If you want to study one topic at a time:

### State stack

- `GMDCore/States/GameStateBase.cs`
- `GMDCore/States/StateStack.cs`
- `Pokemon/States/GameStates/FadeState.cs`
- `Pokemon/States/GameStates/DialogueState.cs`

### UI

- `GMDCore/GUI/Panel.cs`
- `Pokemon/GUI/Textbox.cs`
- `Pokemon/GUI/Selection.cs`
- `Pokemon/GUI/Menu.cs`

### Tweening

- `GMDCore/Tweening/TweenManager.cs`
- `Pokemon/States/GameStates/FadeState.cs`
- `Pokemon/States/EntityStates/EntityWalkState.cs`
- `Pokemon/States/GameStates/TakeTurnState.cs`

### Turn-based systems

- `Pokemon/States/GameStates/BattleState.cs`
- `Pokemon/States/GameStates/BattleMenuState.cs`
- `Pokemon/States/GameStates/TakeTurnState.cs`

### RPG mechanics

- `Pokemon/Mons/PokemonSpecies.cs`
- `Pokemon/Mons/Mon.cs`
- `Pokemon/Mons/Move.cs`
- `Pokemon/Mons/Party.cs`

### Data-driven architecture

- `Pokemon/Content/data/pokemon_definitions.json`
- `Pokemon/Content/data/entity_animations.json`
- `Pokemon/Mons/PokemonDefinitions.cs`
- `Pokemon/Definitions/ContentLoader.cs`

---

## 15. Final Advice for Students

When reading a project like this, do not ask “how do I memorize all of it?”

Instead ask:

- what is the responsibility of this class?
- what data does it own?
- who calls it?
- what happens before it runs?
- what happens after it finishes?

If you can answer those questions, you understand the architecture much better than if you only recognize syntax.
