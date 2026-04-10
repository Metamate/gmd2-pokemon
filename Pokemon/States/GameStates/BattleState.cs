using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon;
using Pokemon.Battle;
using Pokemon.Definitions;
using Pokemon.Entities;
using GMDCore.GUI;
using Pokemon.GUI;
using Pokemon.Mons;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

// The main battle screen. Manages the two battle sprites, health bars, and EXP bar.
// On first update the Pokemon slide in from the screen edges, then starting dialogue is shown.
public sealed class BattleState : GameStateBase
{
    private readonly StateStack _stack;

    public  Opponent     WildOpponent   { get; }
    public  BattleSprite PlayerSprite   { get; private set; }
    public  BattleSprite OpponentSprite { get; private set; }

    public  ProgressBar PlayerHealthBar   { get; private set; }
    public  ProgressBar OpponentHealthBar { get; private set; }
    public  ProgressBar PlayerExpBar      { get; private set; }

    public  Mon PlayerPokemon   { get; }
    public  Mon OpponentPokemon { get; }

    private bool _slideStarted;
    public  bool RenderHealthBars { get; set; }

    // Shadow ellipse x-positions (tweened during slide-in)
    private float _playerShadowX;
    private float _opponentShadowX;

    private readonly Panel _bottomPanel;

    private const int ShadowRx        = 72;
    private const int ShadowRy        = 24;
    private const int OpponentShadowY = 60;

    private static readonly Color BattleBg = new(214, 214, 214);
    private static readonly Color HpColor  = new(189, 32, 32);
    private static readonly Color ExpColor = new(32, 32, 189);

    public BattleState(Player player, StateStack stack)
    {
        _stack       = stack;
        WildOpponent = Opponent.CreateWild();

        PlayerPokemon   = player.Party.Current;
        OpponentPokemon = WildOpponent.Party.Current;

        // Battle sprites start off-screen
        var playerTex   = ContentLoader.GetPokemonSprite(PlayerPokemon.BattleSpriteBack);
        var opponentTex = ContentLoader.GetPokemonSprite(OpponentPokemon.BattleSpriteFront);

        PlayerSprite   = new BattleSprite(playerTex,  -64f, GameSettings.VirtualHeight - 128f);
        OpponentSprite = new BattleSprite(opponentTex, GameSettings.VirtualWidth, 8f);

        _playerShadowX   = -68f;
        _opponentShadowX =  GameSettings.VirtualWidth + 32f;

        var pBarPos = Layout.GetPosition(Anchor.BottomRight, 152, 6, -8, -74);
        PlayerHealthBar = new ProgressBar(
            pBarPos.X, pBarPos.Y, 152, 6,
            HpColor, PlayerPokemon.CurrentHp, PlayerPokemon.Hp);

        var oBarPos = Layout.GetPosition(Anchor.TopLeft, 152, 6, 8, 8);
        OpponentHealthBar = new ProgressBar(
            oBarPos.X, oBarPos.Y, 152, 6,
            HpColor, OpponentPokemon.CurrentHp, OpponentPokemon.Hp);

        var expBarPos = Layout.GetPosition(Anchor.BottomRight, 152, 6, -8, -67);
        PlayerExpBar = new ProgressBar(
            expBarPos.X, expBarPos.Y, 152, 6,
            ExpColor, PlayerPokemon.CurrentExp, PlayerPokemon.ExpToLevel);

        var panelPos = Layout.GetPosition(Anchor.BottomLeft, GameSettings.VirtualWidth, 64);
        _bottomPanel = new Panel(panelPos.X, panelPos.Y, GameSettings.VirtualWidth, 64);
    }

    public override void Exit()
    {
        Locator.Audio.StopMusic();
    }

    public override void Update(GameTime gameTime)
    {
        if (!_slideStarted)
            StartSlideIn();
    }

    private void StartSlideIn()
    {
        _slideStarted = true;
        Locator.Tweens.Tween(GameSettings.BattleSlideInDuration)
            .Add(v => PlayerSprite.X    = v, PlayerSprite.X,   32f)
            .Add(v => OpponentSprite.X  = v, OpponentSprite.X, GameSettings.VirtualWidth - 96f)
            .Add(v => _playerShadowX    = v, _playerShadowX,   66f)
            .Add(v => _opponentShadowX  = v, _opponentShadowX, GameSettings.VirtualWidth - 70f)
            .Finish(() =>
            {
                RenderHealthBars = true;
                ShowStartingDialogue();
            });
    }

    private void ShowStartingDialogue()
    {
        _stack.Push(new BattleMessageState(_stack,
            $"A wild {OpponentPokemon.Name} appeared!",
            () =>
            {
                _stack.Push(new BattleMessageState(_stack,
                    $"Go, {PlayerPokemon.Name}!",
                    () => _stack.Push(new BattleMenuState(_stack, this))));
            }));
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        spriteBatch.Draw(Core.Pixel,
            new Rectangle(0, 0, GameSettings.VirtualWidth, GameSettings.VirtualHeight),
            BattleBg);

        spriteBatch.Draw(Locator.Assets.ShadowTex, new Vector2(_opponentShadowX - ShadowRx, OpponentShadowY - ShadowRy), Color.White);
        spriteBatch.Draw(Locator.Assets.ShadowTex, new Vector2(_playerShadowX  - ShadowRx, GameSettings.VirtualHeight - 64 - ShadowRy), Color.White);

        PlayerSprite.Draw(spriteBatch);
        OpponentSprite.Draw(spriteBatch);

        if (RenderHealthBars)
        {
            PlayerHealthBar.Draw(spriteBatch);
            OpponentHealthBar.Draw(spriteBatch);
            PlayerExpBar.Draw(spriteBatch);

            Locator.Assets.SmallFont.Draw(spriteBatch,
                $"LV {PlayerPokemon.Level}",
                new Vector2(PlayerHealthBar.X, PlayerHealthBar.Y - 10),
                Color.Black);

            Locator.Assets.SmallFont.Draw(spriteBatch,
                $"LV {OpponentPokemon.Level}",
                new Vector2(OpponentHealthBar.X, OpponentHealthBar.Y + 8),
                Color.Black);
        }

        _bottomPanel.Draw(spriteBatch);

        spriteBatch.End();
    }
}
