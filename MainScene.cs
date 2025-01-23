using System;
using System.IO;
using bubble_puzzle.GameObject;
using bubbleTea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace bubble_puzzle;

public class MainScene : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private Texture2D _backgroudTexture;
    private Texture2D _gameBoardTexture;
    private string _maptext;

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

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _backgroudTexture = Content.Load<Texture2D>("image/game_asset_05");
        _gameBoardTexture = Content.Load<Texture2D>("image/game_asset_04");
        Singleton.Instance.gameBoard.texture = _gameBoardTexture;

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
            Singleton.Instance.gameBoard.bubbleTexture[i] = Content.Load<Texture2D>("image/eggs/bubble0" + i);
        }
        Singleton.Instance.gameBoard.Reset();
        //load all of score texture into array
        for (int i = 0; i < Singleton.Instance.scoreObject.scoreTexture.Length; i++)
        {
            Singleton.Instance.scoreObject.scoreTexture[i] = Content.Load<Texture2D>("image/game_asset_03");
        }

        _font = Content.Load<SpriteFont>("GameFont");

        //create aim assistant texture
        Texture2D _react = new Texture2D(GraphicsDevice, 64, 64*5);
        Color[] data = new Color[64 * 64 * 5];
        for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
        _react.SetData(data);
        Singleton.Instance.gameBoard.aimAssistant.texture = _react;
    }

    protected override void Update(GameTime gameTime)
    {
        
        Singleton.Instance.CurrentKeyboard = Keyboard.GetState();
        Singleton.Instance.CurrentMouse = Mouse.GetState();

        Singleton.Instance.gameBoard.Update(gameTime);

        Singleton.Instance.PreviousKeyboard = Singleton.Instance.CurrentKeyboard;
        Singleton.Instance.PreviousMouse = Singleton.Instance.CurrentMouse;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        _spriteBatch.Draw(_backgroudTexture, new Vector2(0, 0), Color.White);
        Singleton.Instance.gameBoard.Draw(_spriteBatch);
        Singleton.Instance.scoreObject.Draw(_spriteBatch);

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
