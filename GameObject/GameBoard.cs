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
        List<Bubble> matchedBubbles;
        List<Bubble> falledBubbles;
        List<Bubble> killedBubbles;
        Bubble currentBubble;
        public AimAssistant aimAssistant;
        public Texture2D highlightTexture;
        public Texture2D[] bubbleTexture;
        public string mapText;
        public Player player;

        public bool isFrozen;
        public float frozenDuration;
        public float frozenTick;

        public GameBoard(Texture2D texture) : base(texture)
        {
            board = new Bubble[13, 8];
            rowType = new bool[13];
            bubbles = new List<Bubble>();
            matchedBubbles = new List<Bubble>();
            falledBubbles = new List<Bubble>();
            killedBubbles = new List<Bubble>();
            bubbleTexture = new Texture2D[6];
            aimAssistant = new AimAssistant(null);
            player = new Player(null);
            isFrozen = false;
            frozenDuration = 3;
            frozenTick = 0;

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
            _tick += (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            //rotate the aim assistant clockwise every 1 second 10 degree
            if (_tick > 5 && !isFrozen)
            {
                //aimAssistant.Rotation += MathHelper.ToRadians(10);
                ceilingDrop();
                _tick = 0;
            }
            else if (isFrozen)
            {
                if (_tick > frozenTick + frozenDuration)
                {
                    isFrozen = false;
                    _tick = frozenTick;
                }
            }

            foreach (Bubble bubble in falledBubbles)
            {
                //check if the bubble Y is more than the board position then remove the bubble
                if (bubble.Position.Y > GameConstants.GAME_WINDOW_HEIGHT)
                {
                    killedBubbles.Add(bubble);
                }
                bubble.Update(gameTime);
            }

            foreach (Bubble bubble in killedBubbles)
            {
                falledBubbles.Remove(bubble);
            }
            killedBubbles.Clear();


            switch (currentGameState)
            {
                case GameState.BubbleReload:

                    currentBubble = new Bubble(null);
                    //currentBubble.isHighlighted = true;
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

                    //check if spacebar is pressed then reset the game
                    if (Singleton.Instance.CurrentKeyboard.IsKeyDown(Keys.Space) && Singleton.Instance.PreviousKeyboard.IsKeyUp(Keys.Space))
                    {
                        Reset();
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
                        //currentGameState = GameState.BubbleReload;
                        currentGameState = GameState.BubbleMatch;
                    }
                    break;
                case GameState.BubbleMatch:
                    matchedBubbles = checkMatch(currentBubble);
                    if (matchedBubbles == null)
                    {
                        currentGameState = GameState.BubbleReload;
                    }
                    else
                    {
                        currentGameState = GameState.BubbleFall;
                    }
                    break;
                case GameState.BubbleFall:
                    falledBubbles = checkFall();
                    matchedBubbles.Clear();
                    calculateScore();

                    foreach (Bubble bubble in falledBubbles)
                    {
                        if (bubble.currentBubbleType == BubbleType.Frozen)
                        {
                            frozenTick = _tick;
                            isFrozen = true;
                        }
                        bubble.setFall(true);
                    }

                    currentGameState = GameState.BubbleReload;
                    break;
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);

            foreach (Bubble bubble in bubbles)
            {
                bubble.Draw(spriteBatch);
            }

            foreach (Bubble bubble in falledBubbles)
            {
                bubble.Draw(spriteBatch);
            }

            switch (currentGameState)
            {
                case GameState.BubbleReload:
                    break;
                case GameState.Aim:
                    aimAssistant.Draw(spriteBatch);
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


            player.Draw(spriteBatch);
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
                        Bubble bubble = new Bubble(null);
                        bubble.setTexture(bubbleTexture[(int)type], highlightTexture);
                        bubble.row = i;
                        bubble.col = j;
                        bubble.currentBubbleType = type;
                        board[i, j] = bubble;
                        bubbles.Add(bubble);

                        //set the position of the bubble base on the row type
                        if (rowType[i])
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

            currentGameState = GameState.BubbleReload;
            aimAssistant.Rotation = 0;
            Singleton.Instance.score = 0;

            base.Reset();
        }

        //find the right position for the bubble to be placed
        public void placeBubble(Bubble currentBubble, Bubble colledBubble)
        {

            Bubble getLeftChild(Bubble bubble)
            {
                bool isEven = rowType[bubble.row];
                if (isEven)
                {
                    if (bubble.col - 1 >= 0 && bubble.row + 1 < 13)
                    {
                        return board[bubble.row + 1, bubble.col - 1];
                    }
                }
                else
                {
                    if (bubble.row + 1 < 13)
                    {
                        return board[bubble.row + 1, bubble.col];
                    }

                }
                return null;
            }

            Bubble getRightChild(Bubble bubble)
            {
                bool isEven = rowType[bubble.row];
                if (isEven)
                {
                    if (bubble.row + 1 < 13)
                    {
                        return board[bubble.row + 1, bubble.col];
                    }
                }
                else
                {
                    if (bubble.col + 1 < 8 || bubble.row + 1 < 13)
                    {
                        return board[bubble.row + 1, bubble.col + 1];
                    }

                }
                return null;
            }

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
                //First Element of this list is the closest bubble to the current bubble then we will use this as a main reference when placing the bubble is not occupied
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
                        currentBubble.Position = new Vector2(colledBubble.Position.X - GameConstants.TILE_SIZE, colledBubble.Position.Y);
                    }
                    //bottom
                    else
                    {
                        /*
                        check if the colled bubble is in the even row and is the first element of the row
                        then the current bubble which is the odd line have to shift to the bottom right
                        */

                        if (rowType[colledBubble.row] == true && colledBubble.col == 0)
                        {
                            currentBubble.Position = new Vector2(colledBubble.Position.X + half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                        }
                        else
                        {
                            /*
                            check if the bottom left is empty if not then compare with the second element of the colled bubble
                            if current bubble colled with the second element at the right then place the current bubble at the bottom right at main reference
                            if current buuble coolled with the second element at the left then place the current bubble at the top left at main reference
                            */
                            if (getLeftChild(colledBubble) == null)
                            {
                                currentBubble.Position = new Vector2(colledBubble.Position.X - half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                            }
                            else
                            {
                                Console.WriteLine("LEFT Child is not null adjust the position");
                                currentBubble.Position = new Vector2(colledBubble.Position.X + half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                            }
                        }
                    }
                }
                else
                {
                    //top
                    if (diffY > 0)
                    {
                        currentBubble.Position = new Vector2(colledBubble.Position.X + GameConstants.TILE_SIZE, colledBubble.Position.Y);
                    }
                    //bottom
                    else
                    {
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
                            if (getRightChild(colledBubble) == null)
                            {
                                currentBubble.Position = new Vector2(colledBubble.Position.X + half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                            }
                            else
                            {
                                Console.WriteLine("RIGHT Child is not null adjust the position");
                                currentBubble.Position = new Vector2(colledBubble.Position.X - half, colledBubble.Position.Y + GameConstants.TILE_SIZE);
                            }
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
            HashSet<Bubble> visited = new HashSet<Bubble>();
            Stack<Bubble> stack = new Stack<Bubble>();

            stack.Push(currentBubble);
            visited.Add(currentBubble);

            while (stack.Count > 0)
            {
                Bubble bubble = stack.Pop();
                matchBubbles.Add(bubble);

                // Check all neighboring bubbles
                foreach (Bubble neighbor in GetNeighbors(bubble))
                {
                    if (!visited.Contains(neighbor) &&
                        (neighbor.currentBubbleType == currentBubble.currentBubbleType ||
                        neighbor.currentBubbleType == BubbleType.Frozen ||
                        neighbor.currentBubbleType == BubbleType.Bomb))
                    {
                        visited.Add(neighbor);
                        stack.Push(neighbor);                       
                    }
                }
            }

            //Console.WriteLine("Match Count: " + matchBubbles.Count);

            if (matchBubbles.Count < 3)
            {
                matchBubbles = null;
                //Console.WriteLine("matchBubbles is null");
            }

            return matchBubbles;
        }

        private List<Bubble> GetNeighbors(Bubble bubble)
        {
            List<Bubble> neighbors = new List<Bubble>();
            int[][] positions;

            if (rowType[bubble.row])
            {
                // Even row
                positions =
                [
                    [-1, -1], // Top-left
                    [-1,  0], // Top-right
                    [0, -1], // Left
                    [0,  1], // Right
                    [1, -1], // Bottom-left
                    [1,  0]  // Bottom-right
                ];
            }
            else
            {
                // Odd row
                positions =
                [
                    [-1,  0], // Top-left
                    [-1,  1], // Top-right
                    [0, -1], // Left
                    [0,  1], // Right
                    [1,  0], // Bottom-left
                    [1,  1]  // Bottom-right
                ];
            }

            // Add valid neighbors
            foreach (var position in positions)
            {
                int newRow = bubble.row + position[0];
                int newCol = bubble.col + position[1];

                if (newRow >= 0 && newRow < board.GetLength(0) &&
                    newCol >= 0 && newCol < board.GetLength(1))
                {
                    Bubble neighbor = board[newRow, newCol];
                    if (neighbor != null)
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }

        public List<Bubble> checkFall()
        {
            List<Bubble> fallBubble = new List<Bubble>();
            foreach (Bubble curBubble in matchedBubbles)
            {
                fallBubble.Add(curBubble);
                bubbles.Remove(curBubble);
                board[curBubble.row, curBubble.col] = null;
            }

            bool[,] boardVisited = new bool[board.GetLength(0), board.GetLength(1)];
            for (int i = 0; i < boardVisited.GetLength(0); i++)
            {
                for (int j = 0; j < boardVisited.GetLength(1); j++)
                {
                    if (board[i, j] == null)
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
                int endArray = rowType[i] ? 8 : 7;

                for (int j = 0; j < endArray; j++)
                {
                    if (!boardVisited[i, j])
                    {
                        // Console.WriteLine(i + " " + j);
                        mst(new Point(i, j), boardVisited, group);
                    }
                }
            }

            foreach (Tuple<List<Point>, bool> curGroup in group)
            {
                if (!curGroup.Item2)
                {
                    foreach (Point i in curGroup.Item1)
                    {
                        Bubble removedBubble = board[i.X, i.Y];
                        fallBubble.Add(removedBubble);
                        bubbles.Remove(removedBubble);
                        board[i.X, i.Y] = null;

                        // Console.WriteLine(i.X + " " + i.Y);
                    }
                    Console.WriteLine();
                }
            }

            return fallBubble;
        }

        private void mst(Point currentPoint, bool[,] boardVisited, List<Tuple<List<Point>, bool>> group)
        {
            //Setup
            Stack<Point> stack = new Stack<Point>();
            List<Point> groupTemp = new List<Point>();
            bool isConnectTop = false;

            stack.Push(new Point(currentPoint.X, currentPoint.Y));
            groupTemp.Add(new Point(currentPoint.X, currentPoint.Y));
            boardVisited[currentPoint.X, currentPoint.Y] = true;

            while (stack.Count != 0)
            {
                Point cur = stack.Pop();

                //Check this region is connect top wall
                if (cur.X == 0) isConnectTop = true;
                else if (board[cur.X - 1, cur.Y] != null && (int)board[cur.X - 1, cur.Y].currentBubbleType >= 6) isConnectTop = true;

                //Check Left
                if (cur.Y - 1 >= 0 && !boardVisited[cur.X, cur.Y - 1])
                {
                    stack.Push(new Point(cur.X, cur.Y - 1));
                    groupTemp.Add(new Point(cur.X, cur.Y - 1));
                    boardVisited[cur.X, cur.Y - 1] = true;
                }

                //Check Right
                if (cur.Y + 1 < GameConstants.BOARD_WIDTH && !boardVisited[cur.X, cur.Y + 1])
                {
                    stack.Push(new Point(cur.X, cur.Y + 1));
                    groupTemp.Add(new Point(cur.X, cur.Y + 1));
                    boardVisited[cur.X, cur.Y + 1] = true;
                }

                //Check Top
                if (cur.X - 1 >= 0 && !boardVisited[cur.X - 1, cur.Y])
                {
                    stack.Push(new Point(cur.X - 1, cur.Y));
                    groupTemp.Add(new Point(cur.X - 1, cur.Y));
                    boardVisited[cur.X - 1, cur.Y] = true;
                }

                //Check Bottom
                if (cur.X + 1 < GameConstants.BOARD_HEIGHT && !boardVisited[cur.X + 1, cur.Y])
                {
                    stack.Push(new Point(cur.X + 1, cur.Y));
                    groupTemp.Add(new Point(cur.X + 1, cur.Y));
                    boardVisited[cur.X + 1, cur.Y] = true;
                }

                //Split odd
                if (rowType[cur.X])
                {
                    //Check Top left
                    if (cur.X - 1 >= 0 && cur.Y - 1 >= 0 && !boardVisited[cur.X - 1, cur.Y - 1])
                    {
                        stack.Push(new Point(cur.X - 1, cur.Y - 1));
                        groupTemp.Add(new Point(cur.X - 1, cur.Y - 1));
                        boardVisited[cur.X - 1, cur.Y - 1] = true;
                    }

                    //Check Bottom left
                    if (cur.X + 1 < GameConstants.BOARD_HEIGHT && cur.Y - 1 >= 0 && !boardVisited[cur.X + 1, cur.Y - 1])
                    {
                        stack.Push(new Point(cur.X + 1, cur.Y - 1));
                        groupTemp.Add(new Point(cur.X + 1, cur.Y - 1));
                        boardVisited[cur.X + 1, cur.Y - 1] = true;
                    }
                }
                //Split even
                else
                {
                    //Check Top right
                    if (cur.X - 1 >= 0 && cur.Y + 1 < GameConstants.BOARD_WIDTH && !boardVisited[cur.X - 1, cur.Y + 1])
                    {
                        stack.Push(new Point(cur.X - 1, cur.Y + 1));
                        groupTemp.Add(new Point(cur.X - 1, cur.Y + 1));
                        boardVisited[cur.X - 1, cur.Y + 1] = true;
                    }

                    //Check Bottom right
                    if (cur.X + 1 < GameConstants.BOARD_HEIGHT && cur.Y + 1 < GameConstants.BOARD_WIDTH && !boardVisited[cur.X + 1, cur.Y + 1])
                    {
                        stack.Push(new Point(cur.X + 1, cur.Y + 1));
                        groupTemp.Add(new Point(cur.X + 1, cur.Y + 1));
                        boardVisited[cur.X + 1, cur.Y + 1] = true;
                    }
                }
            }
            group.Add(new Tuple<List<Point>, bool>(groupTemp, isConnectTop));
        }

        public void calculateScore() 
        {
            Singleton.Instance.score += falledBubbles.Count * 5;
        }
    }
}