using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Battle;
using Pokemon.Definitions;
using Pokemon.Entities;
using GMDCore.GUI;
using Pokemon.Mons;
using GMDCore.Tweening;
using GMDCore.States;
using GMDCore;
using GMDCore.Graphics;

namespace Pokemon.States.GameStates;

// The main battle screen. Manages the two battle sprites, health bars, and EXP bar.
// On first update the Pokemon slide in from the screen edges, then starting dialogue is shown.
public sealed class BattleState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Player     _player;

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
    private float _playerCircleX;
    private float _opponentCircleX;

    private readonly Panel    _bottomPanel;
    private readonly Texture2D _shadowTex;

    private const int ShadowRx = 72;
    private const int ShadowRy = 24;

    private static readonly Color BattleBg    = new(214, 214, 214);
    private static readonly Color ShadowColor = new(45, 184, 45, 124);
    private static readonly Color HpColor     = new(189, 32, 32);
    private static readonly Color ExpColor    = new(32, 32, 189);

    public BattleState(Player player, StateStack stack)
        : base(Game1.Current)
    {
        _player      = player;
        _stack       = stack;
        WildOpponent = Opponent.CreateWild();

        PlayerPokemon   = player.Party.Pokemon[0];
        OpponentPokemon = WildOpponent.Party.Pokemon[0];

        // Battle sprites start off-screen
        var playerTex   = ContentLoader.GetPokemonSprite(PlayerPokemon.BattleSpriteBack);
        var opponentTex = ContentLoader.GetPokemonSprite(OpponentPokemon.BattleSpriteFront);

        PlayerSprite   = new BattleSprite(playerTex,  -64f, GameSettings.VirtualHeight - 128f);
        OpponentSprite = new BattleSprite(opponentTex, GameSettings.VirtualWidth, 8f);

        _playerCircleX   = -68f;
        _opponentCircleX =  GameSettings.VirtualWidth + 32f;

        PlayerHealthBar = new ProgressBar(
            GameSettings.VirtualWidth - 160, GameSettings.VirtualHeight - 80, 152, 6,
            HpColor, PlayerPokemon.CurrentHp, PlayerPokemon.Hp);

        OpponentHealthBar = new ProgressBar(
            8, 8, 152, 6,
            HpColor, OpponentPokemon.CurrentHp, OpponentPokemon.Hp);

        PlayerExpBar = new ProgressBar(
            GameSettings.VirtualWidth - 160, GameSettings.VirtualHeight - 73, 152, 6,
            ExpColor, PlayerPokemon.CurrentExp, PlayerPokemon.ExpToLevel);

        _bottomPanel = new Panel(0, GameSettings.VirtualHeight - 64,
                                 GameSettings.VirtualWidth, 64);

        _shadowTex = TextureFactory.CreateEllipse(Game1.Current.GraphicsDevice, ShadowRx, ShadowRy, ShadowColor);
    }

    public override void Exit()
    {
        SoundManager.StopMusic();
    }

    public override void Update(GameTime gameTime)
    {
        if (!_slideStarted)
            StartSlideIn();
    }

    private void StartSlideIn()
    {
        _slideStarted = true;
        TweenManager.Instance.Tween(GameSettings.BattleSlideInDuration)
            .Add(v => PlayerSprite.X    = v, PlayerSprite.X,   32f)
            .Add(v => OpponentSprite.X  = v, OpponentSprite.X, GameSettings.VirtualWidth - 96f)
            .Add(v => _playerCircleX    = v, _playerCircleX,   66f)
            .Add(v => _opponentCircleX  = v, _opponentCircleX, GameSettings.VirtualWidth - 70f)
            .Finish(() =>
            {
                RenderHealthBars = true;
                ShowStartingDialogue();
            });
    }

    private void ShowStartingDialogue()
    {
        _stack.Push(new BattleMessageState(Game, _stack,
            $"A wild {OpponentPokemon.Name} appeared!",
            () =>
            {
                _stack.Push(new BattleMessageState(Game, _stack,
                    $"Go, {PlayerPokemon.Name}!",
                    () => _stack.Push(new BattleMenuState(Game, _stack, this))));
            }));
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        spriteBatch.Draw(Core.Pixel,
            new Rectangle(0, 0, GameSettings.VirtualWidth, GameSettings.VirtualHeight),
            BattleBg);

        spriteBatch.Draw(_shadowTex, new Vector2(_opponentCircleX - ShadowRx, 60          - ShadowRy), Color.White);
        spriteBatch.Draw(_shadowTex, new Vector2(_playerCircleX  - ShadowRx, GameSettings.VirtualHeight - 64 - ShadowRy), Color.White);

        PlayerSprite.Draw(spriteBatch);
        OpponentSprite.Draw(spriteBatch);

        if (RenderHealthBars)
        {
            PlayerHealthBar.Draw(spriteBatch);
            OpponentHealthBar.Draw(spriteBatch);
            PlayerExpBar.Draw(spriteBatch);

            Game1.SmallFont.Draw(spriteBatch,
                $"LV {PlayerPokemon.Level}",
                new Vector2(PlayerHealthBar.X, PlayerHealthBar.Y - 10),
                Color.Black);

            Game1.SmallFont.Draw(spriteBatch,
                $"LV {OpponentPokemon.Level}",
                new Vector2(OpponentHealthBar.X, OpponentHealthBar.Y + 8),
                Color.Black);
        }

        _bottomPanel.Draw(spriteBatch);

        spriteBatch.End();
    }

}
