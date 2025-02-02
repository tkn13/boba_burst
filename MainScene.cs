using System.IO;
using boba_burst.GameObject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace boba_burst;

public class MainScene : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private Texture2D _backgroudTexture;
    private Texture2D _gameBoardTexture;
    private Texture2D _freezeTexture;
    private Texture2D _playerBaseTexture;
    private Texture2D _playerTubeTexture;
    private Texture2D _buttonTexture;
    private Texture2D _scoreBoardTexture;
    private GameButton _startButton;
    private GameButton _restartButton;

    public Texture2D victoryTexture;
    public Texture2D gameOverTexture;
    public Texture2D _restartWinTexture;
    public Texture2D _restartLoseTexture;
    private string _maptext;

    private enum  MainGameState
    {
        MainMenu,
        GamePlay,
        GameOver,
        GameWin
    }

    private MainGameState _currentMainGameState;

    public MainScene()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = GameConstants.GAME_WINDOW_WIDTH;
        _graphics.PreferredBackBufferHeight = GameConstants.GAME_WINDOW_HEIGHT;
        _graphics.ApplyChanges();

        Singleton.Instance.gameBoard.Position = GameConstants.BOARD_POSITION;
        Singleton.Instance.scoreObject.Position = GameConstants.SCORE_POSITION;

        _currentMainGameState = MainGameState.MainMenu;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _backgroudTexture = Content.Load<Texture2D>("image/game_background");
        _gameBoardTexture = Content.Load<Texture2D>("image/game_board");
        _freezeTexture = Content.Load<Texture2D>("image/freezeWall");
        Singleton.Instance.gameBoard.texture = _gameBoardTexture;
        Singleton.Instance.gameBoard.freezeTexture = _freezeTexture;

        //create highlight texture
        Texture2D _highlight = new Texture2D(GraphicsDevice, GameConstants.HITBOX_SIZE, GameConstants.HITBOX_SIZE);
        Color[] data2 = new Color[GameConstants.HITBOX_SIZE * GameConstants.HITBOX_SIZE];
        for (int i = 0; i < data2.Length; ++i) data2[i] = new Color(255, 0, 0, 100);
        _highlight.SetData(data2);
        Singleton.Instance.gameBoard.highlightTexture = _highlight;

        _maptext = LoadTextFile("Content/map.txt");
        Singleton.Instance.gameBoard.mapText = _maptext;
        //load all of buble texture into array
        for (int i = 0; i < Singleton.Instance.gameBoard.bubbleTexture.Length; i++)
        {
            if (i<10) Singleton.Instance.gameBoard.bubbleTexture[i] = Content.Load<Texture2D>("image/bubbles/bubble0" + i);
            else Singleton.Instance.gameBoard.bubbleTexture[i] = Content.Load<Texture2D>("image/bubbles/bubble" + i);
        }
        //load all of score texture into array
        for (int i = 0; i < Singleton.Instance.scoreObject.scoreTexture.Length; i++)
        {
            Singleton.Instance.scoreObject.scoreTexture[i] = Content.Load<Texture2D>("image/number/num" + i);
        }
        _scoreBoardTexture = Content.Load<Texture2D>("image/scoreBoard");
        Singleton.Instance.scoreObject.texture = _scoreBoardTexture;

        _font = Content.Load<SpriteFont>("GameFont");

        //load player texture
        _playerBaseTexture = Content.Load<Texture2D>("image/player/base");
        Singleton.Instance.gameBoard.player.texture = _playerBaseTexture;

        _playerTubeTexture = Content.Load<Texture2D>("image/player/tube");


        //load aim assistant texture
        Singleton.Instance.gameBoard.aimAssistant.texture = _playerTubeTexture;

        _buttonTexture = Content.Load<Texture2D>("image/button/play_button");
        _restartWinTexture = Content.Load<Texture2D>("image/button/restart_win");
        _restartLoseTexture = Content.Load<Texture2D>("image/button/restart_lose");

        victoryTexture = Content.Load<Texture2D>("image/victory");
        gameOverTexture = Content.Load<Texture2D>("image/game_over");
        //create button
        _startButton = new GameButton(_buttonTexture);
        _restartButton = new GameButton(_restartWinTexture);
        _startButton.Position = GameConstants.PLAY_BUTTON_POSITION;
        _restartButton.Position = new Vector2(0, 0);


    }

    protected override void Update(GameTime gameTime)
    {
        
        Singleton.Instance.CurrentKeyboard = Keyboard.GetState();
        Singleton.Instance.CurrentMouse = Mouse.GetState();

        switch(_currentMainGameState)
        {
            case MainGameState.MainMenu:
                if(_startButton.IsClicked())
                {   
                    Singleton.Instance.gameBoard.Reset();
                    _currentMainGameState = MainGameState.GamePlay;
                }
                break;
            case MainGameState.GamePlay:
                if(Singleton.Instance.gameBoard.currentGameState == GameBoard.GameState.GameOver)
                {
                    _restartButton.texture = _restartLoseTexture;
                    _currentMainGameState = MainGameState.GameOver;
                }
                else if(Singleton.Instance.gameBoard.currentGameState == GameBoard.GameState.GameWin)
                {
                    _restartButton.texture = _restartWinTexture;
                    _currentMainGameState = MainGameState.GameWin;
                }
                else
                {
                    Singleton.Instance.gameBoard.Update(gameTime);
                }
                

                break;
            case MainGameState.GameOver:
                if(_restartButton.IsClicked())
                {
                    Singleton.Instance.gameBoard.Reset();
                    _currentMainGameState = MainGameState.GamePlay;
                }
                break;
            case MainGameState.GameWin:
                if(_restartButton.IsClicked())
                {
                    Singleton.Instance.gameBoard.Reset();
                    _currentMainGameState = MainGameState.GamePlay;
                }
                break;
        }

        Singleton.Instance.PreviousKeyboard = Singleton.Instance.CurrentKeyboard;
        Singleton.Instance.PreviousMouse = Singleton.Instance.CurrentMouse;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

            _spriteBatch.Draw(_backgroudTexture, new Vector2(0, 0), Color.White);

        if(GameConstants.DEBUG_MODE)
        {  
            Texture2D _react = new Texture2D(GraphicsDevice, 64* 5, 64 * 7 );
            Color[] data = new Color[64 * 64 * 5 * 7];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Black ;
            _react.SetData(data);
            _spriteBatch.Draw(_react, GameConstants.DEBUG_POSITION, Color.White);
            _spriteBatch.DrawString(_font, "Debug Mode", GameConstants.DEBUG_POSITION, Color.White);
           //parint array of board
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Vector2 position = new Vector2(GameConstants.DEBUG_POSITION.X + (j * 32), GameConstants.DEBUG_POSITION.Y + (i * 32) + 32);
                    string bubbleType = Singleton.Instance.gameBoard.board[i, j] == null ? "-1" : ((int)(Singleton.Instance.gameBoard.board[i, j].currentBubbleType)) + "";
                    _spriteBatch.DrawString(_font, bubbleType, position, Color.White);
                }
                string rowType = Singleton.Instance.gameBoard.rowType[i] ? "99" : "-99";
                _spriteBatch.DrawString(_font, rowType, new Vector2(GameConstants.DEBUG_POSITION.X + 32 * 8, GameConstants.DEBUG_POSITION.Y + (i * 32) + 32), Color.White);
            }
            _spriteBatch.DrawString(_font, "Mouse Rotate Value: " + Singleton.Instance.MouseRotateValue, new Vector2(GameConstants.DEBUG_POSITION.X, GameConstants.DEBUG_POSITION.Y + 32 * 18), Color.White);

        }


        switch(_currentMainGameState)
        {
            case MainGameState.MainMenu:
                _startButton.Draw(_spriteBatch);
                break;
            case MainGameState.GamePlay:
                Singleton.Instance.gameBoard.Draw(_spriteBatch);
                Singleton.Instance.scoreObject.Draw(_spriteBatch);
                break;
            case MainGameState.GameOver:
                _restartButton.Draw(_spriteBatch);
                _spriteBatch.Draw(gameOverTexture, new Vector2(0, 0), Color.White);
                break;
            case MainGameState.GameWin:
                _restartButton.Draw(_spriteBatch);
                _spriteBatch.Draw(victoryTexture, new Vector2(0, 0), Color.White);
                break;

        }

        _spriteBatch.End();
        _graphics.BeginDraw();

        base.Draw(gameTime);
    }

    private string LoadTextFile(string filePath)
    {
        using (var stream = TitleContainer.OpenStream(filePath))
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}
