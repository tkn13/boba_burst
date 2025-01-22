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

        //row type is infromation about the row type true is even row and false is odd row
        public Bubble[,] board;
        public bool[] rowType;
        private float _tick;
        List<Bubble> bubbles;
        Bubble currentBubble;
        public AimAssistant aimAssistant;
        public Texture2D highlightTexture;
        public Texture2D[] bubbleTexture;
        public string mapText;
        public GameBoard(Texture2D texture) : base(texture)
        {
            board = new Bubble[13, 8];
            rowType = new bool[13];
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
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = null;
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
                //create new bubble;
                for (int j = 0; j < 8; j++)
                {
                    //update row type
                    if (line[8] == "99")
                    {
                        rowType[i] = true;
                    }
                    else
                    {
                        rowType[i] = false;
                    }

                    if (line[j] != "-1")
                    {
                        BubbleType type = (BubbleType)int.Parse(line[j]);
                        Bubble bubble = new Bubble(bubbleTexture[(int)type]);
                        bubble.row = i;
                        bubble.col = j;
                        bubble.currentBubbleType = type;
                        board[i, j] = bubble;
                        bubbles.Add(bubble);

                        //set the position of the bubble base on the row type
                        if(rowType[i])
                        {
                            bubble.Position = new Vector2(GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * j), GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * i));
                        }
                        else
                        {
                            bubble.Position = new Vector2(GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * j) + (GameConstants.TILE_SIZE / 2), GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * i));

                        }
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
                if (rowType[0] == true)
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
                    else if (placePosition > 6)
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

                        if (rowType[colledBubble.row] == true && colledBubble.col == 0)
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
                        if (rowType[colledBubble.row] == true && colledBubble.col == 7)
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

            if (rowType[row] == true)
            {
                col = ((int)(currentBubble.Position.X / GameConstants.TILE_SIZE)) - 1;
            }
            else
            {
                col = ((int)((currentBubble.Position.X - (GameConstants.TILE_SIZE / 2)) / GameConstants.TILE_SIZE)) - 1;
            }

            currentBubble.row = row;
            currentBubble.col = col;
            board[row, col] = currentBubble;
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

            List<Tuple<List<Point>, bool>> group = new List<Tuple<List<Point>, bool>>();
            for (int i = 0; i < board.GetLength(0); i++)
            {
                int endArray = board[i, 0] == 99 ? 8 : 7;

                for (int j = 0; j < endArray; j++)
                {
                    if (!boardVisited[i, j])
                    {
                        mst(new Point(i, j), boardVisited, group);
                    }
                }
            }

            foreach(Tuple<List<Point>, bool> curGroup in group)
            {
                if(!curGroup.Item2) {
                    fallCount += curGroup.Item1.Count;
                }
            }

            return fallCount;
        }

        private void mst(Point currentPoint, bool[,] boardVisited, List<Tuple<List<Point>, bool>> group)
        {
            Stack<Point> stack = new Stack<Point>();
            List<Point> groupTemp = new List<Point>();
            bool isConnectTop = false;
            stack.Push(new Point(0, 0));
            boardVisited[currentPoint.X, currentPoint.Y] = true;
            while (stack.Count != 0)
            {
                Point cur = stack.Pop();
                if (cur.X == 0) isConnectTop = true;

                if (cur.Y >= 0 && !boardVisited[cur.X, cur.Y - 1])
                {
                    stack.Push(new Point(cur.X, cur.Y - 1));
                    groupTemp.Add(new Point(cur.X, cur.Y - 1));
                    boardVisited[cur.X, cur.Y - 1] = true;
                }
                if (cur.Y + 1 < 8 && !boardVisited[cur.X, cur.Y + 1])
                {
                    stack.Push(new Point(cur.X, cur.Y + 1));
                    groupTemp.Add(new Point(cur.X, cur.Y + 1));
                    boardVisited[cur.X, cur.Y + 1] = true;
                }
                if (cur.X - 1 >= 0 && !boardVisited[cur.X - 1, cur.Y])
                {
                    stack.Push(new Point(cur.X - 1, cur.Y));
                    groupTemp.Add(new Point(cur.X - 1, cur.Y));
                    boardVisited[cur.X - 1, cur.Y] = true;
                }
                if (cur.X + 1 <= 8 && !boardVisited[cur.X + 1, cur.Y])
                {
                    stack.Push(new Point(cur.X + 1, cur.Y));
                    groupTemp.Add(new Point(cur.X + 1, cur.Y));
                    boardVisited[cur.X + 1, cur.Y] = true;
                }

                if (board[cur.X, 8] == 99)
                {
                    if (cur.X - 1 >= 0 && cur.Y - 1 >= 0 && !boardVisited[cur.X - 1, cur.Y - 1])
                    {
                        stack.Push(new Point(cur.X - 1, cur.Y - 1));
                        groupTemp.Add(new Point(cur.X - 1, cur.Y - 1));
                        boardVisited[cur.X - 1, cur.Y - 1] = true;
                    }
                    if (cur.X + 1 <= 8 && cur.Y - 1 >= 0 && !boardVisited[cur.X + 1, cur.Y - 1])
                    {
                        stack.Push(new Point(cur.X + 1, cur.Y - 1));
                        groupTemp.Add(new Point(cur.X + 1, cur.Y - 1));
                        boardVisited[cur.X + 1, cur.Y - 1] = true;
                    }
                }
                else
                {
                    if (cur.X - 1 >= 0 && cur.Y + 1 <= 8 && !boardVisited[cur.X - 1, cur.Y + 1])
                    {
                        stack.Push(new Point(cur.X - 1, cur.Y + 1));
                        groupTemp.Add(new Point(cur.X - 1, cur.Y + 1));
                        boardVisited[cur.X - 1, cur.Y + 1] = true;
                    }
                    if (cur.X + 1 <= 8 && cur.Y + 1 <= 8 && !boardVisited[cur.X + 1, cur.Y + 1])
                    {
                        stack.Push(new Point(cur.X + 1, cur.Y + 1));
                        groupTemp.Add(new Point(cur.X + 1, cur.Y + 1));
                        boardVisited[cur.X + 1, cur.Y + 1] = true;
                    }
                }
                group.Add(new Tuple<List<Point>, bool>(groupTemp, isConnectTop));
            }
        }
    }
}