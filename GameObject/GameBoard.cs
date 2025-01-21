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
        public Texture2D highlightTexture;
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
                    currentBubble.setTexture(bubbleTexture[bubleType], highlightTexture);

                    currentGameState = GameState.Aim;

                    break;
                case GameState.Aim:

                    //check if mouse is in the game window
                    if (Singleton.Instance.CurrentMouse.X < 0 || Singleton.Instance.CurrentMouse.X > GameConstants.GAME_WINDOW_WIDTH || Singleton.Instance.CurrentMouse.Y < 0 || Singleton.Instance.CurrentMouse.Y > GameConstants.GAME_WINDOW_HEIGHT)
                    {
                        break;
                    }

                    //roate the aim assistant with mouse position the angle is calculated by the angle between the mouse position and the shoot pivot position
                    Vector2 direction = Singleton.Instance.CurrentMouse.Position.ToVector2() - GameConstants.SHOOT_PIVOT_POSITION;
                    aimAssistant.Rotation = (float)Math.Atan2(direction.Y, direction.X);
                    // minus 90 degree to make the aim assistant point to the mouse position
                    aimAssistant.Rotation += MathHelper.ToRadians(90);

                    Singleton.Instance.MouseRotateValue = MathHelper.ToDegrees(aimAssistant.Rotation).ToString();
                    //limit the rotation of the aim assistant to 80 degree



                    if (aimAssistant.Rotation > MathHelper.ToRadians(80) && aimAssistant.Rotation < MathHelper.ToRadians(170))
                    {
                        aimAssistant.Rotation = MathHelper.ToRadians(80);
                    }

                    if (aimAssistant.Rotation < MathHelper.ToRadians(-80) || aimAssistant.Rotation >= MathHelper.ToRadians(170))
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

                    currentBubble.Update(gameTime);
                    //check if the bubble is collide with the another bubble
                    if (currentBubble.isCollide(bubbles) != null || currentBubble.isCollideWithRoof())
                    {
                        Bubble colledBubble = currentBubble.isCollide(bubbles);
                        placeBubble(currentBubble, colledBubble);
                        currentBubble.Velocity = Vector2.Zero;
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
                        if (int.Parse(line[8]) == 99)
                        {
                            bubble.Position = new Vector2(GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * j), GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * i));
                        }
                        else
                        {
                            bubble.Position = new Vector2(GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * j) + (GameConstants.TILE_SIZE / 2), GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * i));
                        }
                        bubble.setTexture(bubbleTexture[board[i, j]], highlightTexture);
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
            //if the colled bubbles is null means that the bubble is colled with the roof place the bubble at the top
            if (colledBubble == null)
            {
                currentBubble.Position.Y = GameConstants.BOARD_POSITION.Y;
                float centerX = currentBubble.Position.X + GameConstants.TILE_SIZE / 2;
                //minus board position to normalize the position
                centerX -= GameConstants.BOARD_POSITION.X;
                int placePosition = 0;
                int divider = 0;
                if (board[0, 8] == 99)
                {
                    divider = GameConstants.TILE_SIZE;
                    placePosition = (int)(centerX / divider);

                    currentBubble.Position.X = GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * placePosition);
                }
                else
                {
                    //minus the half of the tile size to get the center of the bubble
                    centerX -= GameConstants.TILE_SIZE / 2;
                    divider = GameConstants.TILE_SIZE;
                    placePosition = (int)(centerX / divider);
                    if (placePosition < 0)
                    {
                        currentBubble.Position.X = GameConstants.BOARD_POSITION.X;
                    }
                    else if(placePosition > 6)
                    {
                        currentBubble.Position.X = GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * 7);
                    }
                    else
                    {
                        currentBubble.Position.X = GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * placePosition) + (GameConstants.TILE_SIZE / 2);
                    }

                }
            }

            else
            {
                Vector2 currentBubbleCenter = new Vector2(currentBubble.Position.X + GameConstants.TILE_SIZE / 2, currentBubble.Position.Y + GameConstants.TILE_SIZE / 2);
                Vector2 colledBubbleCenter = new Vector2(colledBubble.Position.X + GameConstants.TILE_SIZE / 2, colledBubble.Position.Y + GameConstants.TILE_SIZE / 2);

                float diffX = colledBubbleCenter.X - currentBubbleCenter.X;
                float diffY = colledBubbleCenter.Y - currentBubbleCenter.Y;

                float half = GameConstants.TILE_SIZE / 2;

                //if left
                if (diffX > 0)
                {
                    //top
                    if (diffY > 0)
                    {
                        Console.WriteLine("topleft");
                        currentBubble.Position = new Vector2(colledBubble.Position.X - GameConstants.TILE_SIZE, colledBubble.Position.Y);
                    }
                    //bottom
                    else
                    {
                        Console.WriteLine("bottomleft");

                        /*
                        check if the colled bubble is in the even row and is the first element of the row
                        then the current bubble which is the odd line have to shift to the bottom right
                        */

                        if (board[colledBubble.row, 8] == 99 && colledBubble.col == 0)
                        {
                            Console.WriteLine("SHIFT");
                            currentBubble.Position = new Vector2(colledBubble.Position.X + half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                        }
                        else
                        {
                            Console.WriteLine("NO SHIFT");
                            currentBubble.Position = new Vector2(colledBubble.Position.X - half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                        }


                    }
                }
                else
                {
                    //top
                    if (diffY > 0)
                    {
                        Console.WriteLine("topright");
                        currentBubble.Position = new Vector2(colledBubble.Position.X + GameConstants.TILE_SIZE, colledBubble.Position.Y);
                    }
                    //bottom
                    else
                    {
                        Console.WriteLine("bottomright");
                        /*
                        check if the colled bubble is in the even row and is the last element of the row
                        then the current bubble which is the odd line have to shift to the bottom left
                        */
                        if (board[colledBubble.row, 8] == 99 && colledBubble.col == 7)
                        {
                            currentBubble.Position = new Vector2(colledBubble.Position.X - half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                        }
                        else
                        {
                            currentBubble.Position = new Vector2(colledBubble.Position.X + half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                        }

                    }
                }
            }

            //update the row and col of the current bubble
            // the row can be find by the y position divided by the tile size
            // the col have to check first that if it is in the even row or odd row
            // if in even row then the col will be the x position divided by the tile size
            // if in odd row then the col will be the x position minus the half of the tile size divided by the tile size

            int row, col;

            row = ((int)(currentBubble.Position.Y / GameConstants.TILE_SIZE)) - 1;

            if (board[row, 8] == 99)
            {
                col = ((int)(currentBubble.Position.X / GameConstants.TILE_SIZE)) - 1;
            }
            else
            {
                col = ((int)((currentBubble.Position.X - (GameConstants.TILE_SIZE / 2)) / GameConstants.TILE_SIZE)) - 1;
            }

            currentBubble.row = row;
            currentBubble.col = col;
            board[row, col] = (int)currentBubble.currentBubbleType;
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

            return fallCount;
        }
    }
}