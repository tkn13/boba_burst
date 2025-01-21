using System;
using System.Collections.Generic;
using bubbleTea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace bubble_puzzle.GameObject
{
    public class GameBoard : GameObject
    {

        public int[,] board;
        private float _tick;
        List<Bubble> bubbles;
        Bubble currentBubble;
        public AimAssistant aimAssistant;
        public Texture2D[] bubbleTexture;
        public string mapText;
        public GameBoard(Texture2D texture) : base(texture)
        {
            board = new int[13, 9];
            bubbles = new List<Bubble>();
            bubbleTexture = new Texture2D[6];
            aimAssistant = new AimAssistant(null);

            _tick = 0;
        }

        enum GameState
        {
            BubbleReload,
            Aim,
            Shoot,
            BubbleBounce,
            BubbleMatch,
            BubbleFall,

        }
        GameState currentGameState;

        public override void Update(GameTime gameTime)
        {
            Singleton.Instance.CurrentMouse = Mouse.GetState();
            _tick += (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            //rotate the aim assistant clockwise every 1 second 10 degree
            if (_tick > 5)
            {
                //aimAssistant.Rotation += MathHelper.ToRadians(10);
                ceilingDrop();
                _tick = 0;
            }

            switch (currentGameState)
            {
                case GameState.BubbleReload:

                    currentBubble = new Bubble(null);
                    currentBubble.Position = GameConstants.SHOOT_POSITION;
                    int bubleType = currentBubble.RandomBubbleType(0.5f, new BubbleType[] { BubbleType.Red, BubbleType.Green, BubbleType.Blue, BubbleType.Yellow });
                    currentBubble.setTexture(bubbleTexture[bubleType]);

                    currentGameState = GameState.Aim;

                    break;
                case GameState.Aim:
                    //roate the aim assistant with mouse position the angle is calculated by the angle between the mouse position and the shoot pivot position
                    Vector2 direction = Singleton.Instance.CurrentMouse.Position.ToVector2() - GameConstants.SHOOT_PIVOT_POSITION;
                    aimAssistant.Rotation = (float)Math.Atan2(direction.Y, direction.X);
                    // minus 90 degree to make the aim assistant point to the mouse position
                    aimAssistant.Rotation -= MathHelper.ToRadians(-90);

                    //limit the rotation of the aim assistant to 80 degree
                    if (aimAssistant.Rotation > MathHelper.ToRadians(80))
                    {
                        aimAssistant.Rotation = MathHelper.ToRadians(80);
                    }
                    else if (aimAssistant.Rotation < MathHelper.ToRadians(-80))
                    {
                        aimAssistant.Rotation = MathHelper.ToRadians(-80);
                    }

                    //if left mouse button is pressed then shoot the bubble
                    if (Singleton.Instance.CurrentMouse.LeftButton == ButtonState.Pressed && Singleton.Instance.PreviousMouse.LeftButton == ButtonState.Released)
                    {
                        currentGameState = GameState.Shoot;
                    }
                    break;
                case GameState.Shoot:
                    //shoot the bubble with the angle of the aim assistant
                    currentBubble.Velocity = new Vector2((float)Math.Cos(aimAssistant.Rotation - MathHelper.ToRadians(90)), (float)Math.Sin(aimAssistant.Rotation - MathHelper.ToRadians(90))) * 1000;
                    currentGameState = GameState.BubbleBounce;
                    break;
                case GameState.BubbleBounce:
                    //move the bubble with the velocity with and elapsed time
                    currentBubble.Position += currentBubble.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    //check if the bubble is collide with the right wall
                    if (currentBubble.Position.X + GameConstants.TILE_SIZE > GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * 8))
                    {
                        currentBubble.Position = new Vector2(GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * 8) - GameConstants.TILE_SIZE, currentBubble.Position.Y);
                        currentBubble.Velocity = new Vector2(-currentBubble.Velocity.X, currentBubble.Velocity.Y);
                    }
                    //check if the bubble is collide with the left wall
                    if (currentBubble.Position.X < GameConstants.BOARD_POSITION.X)
                    {
                        currentBubble.Position = new Vector2(GameConstants.BOARD_POSITION.X, currentBubble.Position.Y);
                        currentBubble.Velocity = new Vector2(-currentBubble.Velocity.X, currentBubble.Velocity.Y);
                    }

                    //check if the bubble is collide with the another bubble
                    if (currentBubble.isCollide(bubbles) != null)
                    {
                        Bubble colledBubble = currentBubble.isCollide(bubbles);
                        placeBubble(currentBubble, colledBubble);
                        bubbles.Add(currentBubble);
                        currentGameState = GameState.BubbleReload;
                    }
                    break;
                case GameState.BubbleMatch:

                    break;
                case GameState.BubbleFall:
                    break;
            }

            Singleton.Instance.PreviousMouse = Singleton.Instance.CurrentMouse;
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);

            foreach (Bubble bubble in bubbles)
            {
                bubble.Draw(spriteBatch);
            }

            switch (currentGameState)
            {
                case GameState.BubbleReload:
                    break;
                case GameState.Aim:
                    aimAssistant.Draw(spriteBatch);
                    // Draw a red line from the pivot point to the mouse position
                    Texture2D lineTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                    lineTexture.SetData(new[] { Color.Red });

                    Vector2 pivot = GameConstants.SHOOT_PIVOT_POSITION;
                    Vector2 mousePosition = Singleton.Instance.CurrentMouse.Position.ToVector2();
                    Vector2 direction = mousePosition - pivot;
                    float angle = (float)Math.Atan2(direction.Y, direction.X);
                    float length = direction.Length();

                    spriteBatch.Draw(lineTexture, pivot, null, Color.Red, angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
                    checkFall(new List<Bubble>());
                    break;
                case GameState.Shoot:

                    break;
                case GameState.BubbleBounce:

                    break;
                case GameState.BubbleMatch:
                    break;
                case GameState.BubbleFall:
                    break;
            }

            currentBubble.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            //Reset the board
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    board[i, j] = -1;
                }
            }

            //Destroy all bubbles
            bubbles.Clear();

            string[] lines = mapText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < 13; i++)
            {  
                string trimmed = lines[i].Trim();
                trimmed = trimmed.Replace(" ", "");
                string[] line = trimmed.Split(',');
                for (int j = 0; j < 9; j++)
                {
                    board[i, j] = int.Parse(line[j]);
                    if (board[i, j] != -1 && j < 8)
                    {
                        Bubble bubble = new Bubble(null);
                        //if last element of the row is 99 then the bubble will be placed as a even row
                        //else if -99 the bubble will be placed as a odd row
                        if(int.Parse(line[8]) == 99)
                        {
                            bubble.Position = new Vector2(GameConstants.BOARD_POSITION.X  + (GameConstants.TILE_SIZE * j), GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * i));
                        }
                        else
                        {
                            bubble.Position = new Vector2(GameConstants.BOARD_POSITION.X  + (GameConstants.TILE_SIZE * j) + (GameConstants.TILE_SIZE / 2), GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * i));
                        }
                        bubble.setTexture(bubbleTexture[board[i, j]]);
                        bubble.row = i;
                        bubble.col = j;
                        bubbles.Add(bubble);
                    }
                }
            }

            base.Reset();
        }

        //find the right position for the bubble to be placed
        public void placeBubble(Bubble currentBubble, Bubble colledBubble)
        {
            //if the reference point of current bubble is more than the half of the colled bubble then the current bubble will be placed on the right side of the colled bubble
            if (currentBubble.Position.X < colledBubble.Position.X + GameConstants.TILE_SIZE / 2)
            {
                currentBubble.Position = new Vector2(colledBubble.Position.X - (GameConstants.TILE_SIZE / 2), colledBubble.Position.Y + GameConstants.TILE_SIZE);
            }
            else
            {
                currentBubble.Position = new Vector2(colledBubble.Position.X + (GameConstants.TILE_SIZE / 2), colledBubble.Position.Y + GameConstants.TILE_SIZE);
            }
        }

        // public bool checkGameOver()
        // {

        // }
        public void gameOver()
        {

        }
        //drop the bubble from the top
        public void ceilingDrop()
        {

        }

        //check the current placement of the bubble
        public List<Bubble> checkMatch(Bubble currentBubble)
        {
            List<Bubble> matchBubbles = new List<Bubble>();
            int row = currentBubble.row;
            int col = currentBubble.col;

            return matchBubbles;

        }

        public int checkFall(List<Bubble> bubbles)
        {
            int fallCount = 0;

            foreach (Bubble curBubble in bubbles)
            {
                board[curBubble.row, curBubble.col] = -1;
            }

            bool[,] boardVisited = new bool[board.GetLength(0), board.GetLength(1)];
            for (int i = 0; i < boardVisited.GetLength(0); i++)
            {
                for (int j = 0; j < boardVisited.GetLength(1); j++)
                {
                    if (board[i, j] == -1 || board[i, j] == 99 || board[i, j] == -99)
                    {
                        boardVisited[i, j] = true;
                    }
                    else
                    {
                        boardVisited[i, j] = false;
                    }
                }
            }
            Print2DArray(boardVisited);

            var group = (Points: new List<Point>(), isConnectTop: false);
            for (int i = 0; i < board.GetLength(0); i++)
            {
                int endArray = board[i, 0] == 99 ? 9 : 8;

                for (int j = 1; j < endArray; j++)
                {
                    if (!boardVisited[i, j])
                    {

                    }
                }
            }

            return fallCount;
        }

        static void Print2DArray(bool[,] array)
        {
            int rows = array.GetLength(0);
            int columns = array.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Console.Write(array[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}