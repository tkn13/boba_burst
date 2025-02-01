using System;
using System.Collections.Generic;
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
        List<Bubble> walls;
        List<Bubble> falledBubbles;
        List<Bubble> killedBubbles;
        HashSet<BubbleType> availableTypes;
        Bubble currentBubble;
        Bubble previousBubble;
        int sameTypeCount;
        int maxSameType;
        public AimAssistant aimAssistant;
        public Texture2D highlightTexture;
        public Texture2D freezeTexture;
        public Texture2D[] bubbleTexture;
        public string mapText;
        public Player player;
        public int currentRootRow;

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
            bubbleTexture = new Texture2D[15];
            availableTypes = new HashSet<BubbleType>();
            aimAssistant = new AimAssistant(null);
            player = new Player(null);
            walls = new List<Bubble>();
            isFrozen = false;
            sameTypeCount = 0;
            maxSameType = 3;
            frozenDuration = 3;
            frozenTick = 0;
            currentRootRow = 0;

            _tick = 0;
        }

        public enum GameState
        {
            BubbleReload,
            Aim,
            Shoot,
            BubbleBounce,
            BubbleMatch,
            BubbleFall,
            GameOver,
            GameWin

        }
        public GameState currentGameState;

        public override void Update(GameTime gameTime)
        {
            _tick += (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            //rotate the aim assistant clockwise every 1 second 10 degree
            if (_tick > 5 && !isFrozen)
            {
                //aimAssistant.Rotation += MathHelper.ToRadians(10);
                ceilingDrop();
                if (isgameOver())
                {
                    currentGameState = GameState.GameOver;
                }
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
            killedBubbles?.Clear();


            switch (currentGameState)
            {
                case GameState.BubbleReload:

                    availableTypes?.Clear();
                    currentBubble = new Bubble(null);
                    //currentBubble.isHighlighted = true;
                    currentBubble.Position = GameConstants.SHOOT_POSITION;

                    // create available types from bubbles in the board
                    foreach (var bubble in bubbles)
                    {
                        if (!(bubble.currentBubbleType == BubbleType.Frozen || bubble.currentBubbleType == BubbleType.Bomb))
                        {
                            availableTypes.Add(bubble.currentBubbleType);
                        }
                    }

                    // check if the game random the same type too much
                    do
                    {
                        int bubleType = (int)currentBubble.RandomBubbleType(new List<BubbleType>(availableTypes), new List<BubbleType>());
                        currentBubble.setTexture(bubbleTexture[bubleType], highlightTexture);
                    }
                    while (currentBubble == previousBubble && sameTypeCount >= maxSameType);

                    // same type streak counter and update previous bubble 
                    if (currentBubble == previousBubble)
                    {
                        sameTypeCount++;
                    }
                    else
                    {
                        sameTypeCount = 0;
                    }
                    previousBubble = currentBubble;

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
                    if (currentBubble.isCollide(bubbles) != null || currentBubble.isCollideWithRoof(currentRootRow))
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
                        if (isgameOver())
                        {
                            currentGameState = GameState.GameOver;
                        }
                        else
                        {
                            currentGameState = GameState.BubbleReload;
                        }
                    }
                    else
                    {
                        currentGameState = GameState.BubbleFall;
                    }
                    break;
                case GameState.BubbleFall:
                    falledBubbles = checkFall();
                    matchedBubbles?.Clear();
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

                    if (checkWin())
                    {
                        currentGameState = GameState.GameWin;
                    }
                    else
                    {
                        currentGameState = GameState.BubbleReload;
                    }
                    break;
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);

            foreach (Bubble wall in walls)
            {
                wall.Draw(spriteBatch);
            }

            player.Draw(spriteBatch);

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

            foreach (Bubble bubble in bubbles)
            {
                bubble.Draw(spriteBatch);
            }

            foreach (Bubble bubble in falledBubbles)
            {
                bubble.Draw(spriteBatch);
            }
            currentBubble.Draw(spriteBatch);

            if (isFrozen)
            {
                spriteBatch.Draw(freezeTexture, Position, Color.White);
            }

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
            bubbles?.Clear();
            killedBubbles?.Clear();
            falledBubbles?.Clear();

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

            currentBubble = new Bubble(null);
            //currentBubble.isHighlighted = true;
            currentBubble.Position = GameConstants.SHOOT_POSITION;
            int bubleType = (int)currentBubble.RandomBubbleType(new List<BubbleType>()
                    {
                       BubbleType.Red,
                       BubbleType.Green,
                       BubbleType.Blue,
                       BubbleType.Yellow
                    }, new List<BubbleType>());

            currentBubble.setTexture(bubbleTexture[bubleType], highlightTexture);
            aimAssistant.Rotation = 0;
            Singleton.Instance.score = 0;
            currentRootRow = 0;
            walls?.Clear();
            _tick = 0;
            isFrozen = false;

            currentGameState = GameState.Aim;

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
                currentBubble.Position.Y = GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * currentRootRow);
                float centerX = currentBubble.Position.X + GameConstants.TILE_SIZE / 2;
                //minus board position to normalize the position
                centerX -= GameConstants.BOARD_POSITION.X;
                int placePosition = 0;
                int divider = 0;
                if (rowType[currentRootRow] == true)
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

        public bool isgameOver()
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[GameConstants.GAME_OVER_LINE, j] != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool checkWin()
        {
            // check if the board has no more bubble, player win.
            if (bubbles.Count == 0)
            {
                return true;
            }

            return false;
        }

        //drop the bubble from the top
        public void ceilingDrop()
        {
            //Move Bubble down
            for (int i = board.GetLength(0) - 1; i > 0; i--)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = board[i - 1, j];
                    board[i - 1, j] = null;

                    if (board[i, j] != null)
                    {
                        board[i, j].Position.Y += 64;
                        board[i, j].row += 1;
                    }
                }
            }

            //Swap rowType
            for (int i = 0; i < rowType.Length; i++)
            {
                rowType[i] = !rowType[i];
            }

            //Spawn Wall
            for (int j = 0; j < GameConstants.BOARD_WIDTH; j++)
            {
                Bubble bubble = new Bubble(null);

                if (currentRootRow > 0)
                {
                    if (j == 0) bubble.currentBubbleType = BubbleType.LeftWall;
                    else if (j == GameConstants.BOARD_WIDTH - 1) bubble.currentBubbleType = BubbleType.RightWall;
                    else bubble.currentBubbleType = BubbleType.CenterWall;
                }
                else
                {
                    if (j == 0) bubble.currentBubbleType = BubbleType.BottomLeftWall;
                    else if (j == GameConstants.BOARD_WIDTH - 1) bubble.currentBubbleType = BubbleType.BottomRightWall;
                    else bubble.currentBubbleType = BubbleType.BottomWall;
                }

                bubble.setTexture(bubbleTexture[(int)bubble.currentBubbleType], highlightTexture);
                bubble.row = 0;
                bubble.col = j;
                board[0, j] = bubble;
                bubble.Position = new Vector2(GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * j), GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * 0));
                walls.Add(bubble);
            }
            currentRootRow++;
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

            if (matchBubbles.Count < 3)
            {
                matchBubbles = null;
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
            while (matchedBubbles.Count > 0)
            {
                Bubble curBubble = matchedBubbles[0];
                matchedBubbles.RemoveAt(0);

                if (curBubble.currentBubbleType == BubbleType.Bomb)
                {
                    matchedBubbles.AddRange(bombActivate(curBubble));
                }

                fallBubble.Add(curBubble);
                bubbles.Remove(curBubble);
                board[curBubble.row, curBubble.col] = null;
            }

            //Make visited check list
            bool[,] boardVisited = new bool[board.GetLength(0), board.GetLength(1)];
            for (int i = 0; i < boardVisited.GetLength(0); i++)
            {
                for (int j = 0; j < boardVisited.GetLength(1); j++)
                {
                    if (board[i, j] == null || (int)board[i, j].currentBubbleType >= 6)
                    {
                        boardVisited[i, j] = true;
                    }
                    else
                    {
                        boardVisited[i, j] = false;
                    }
                }
            }

            //Loop each bubble to find region
            List<Tuple<List<Bubble>, bool>> group = new List<Tuple<List<Bubble>, bool>>();
            for (int i = 0; i < board.GetLength(0); i++)
            {
                int endArray = rowType[i] ? 8 : 7;

                for (int j = 0; j < endArray; j++)
                {
                    if (!boardVisited[i, j])
                    {
                        // Console.WriteLine(i + " " + j);
                        mst(board[i, j], boardVisited, group);
                    }
                }
            }

            //Remove region that not connect the roof
            foreach (Tuple<List<Bubble>, bool> curGroup in group)
            {
                if (!curGroup.Item2)
                {
                    foreach (Bubble i in curGroup.Item1)
                    {
                        Bubble removedBubble = board[i.row, i.col];
                        fallBubble.Add(removedBubble);
                        bubbles.Remove(removedBubble);
                        board[i.row, i.col] = null;

                        // Console.WriteLine(i.X + " " + i.Y);
                    }
                    Console.WriteLine();
                }
            }

            return fallBubble;
        }

        private void mst(Bubble currentBubble, bool[,] boardVisited, List<Tuple<List<Bubble>, bool>> group)
        {
            //Setup
            Stack<Bubble> stack = new Stack<Bubble>();
            List<Bubble> groupTemp = new List<Bubble>();
            bool isConnectTop = false;

            stack.Push(currentBubble);
            groupTemp.Add(currentBubble);
            boardVisited[currentBubble.row, currentBubble.col] = true;

            while (stack.Count != 0)
            {
                Bubble cur = stack.Pop();

                //Check this region is connect top wall
                if (cur.row == 0) isConnectTop = true;
                else if (board[cur.row - 1, cur.col] != null && (int)board[cur.row - 1, cur.col].currentBubbleType >= 6) isConnectTop = true;

                List<Bubble> neighbors = GetNeighbors(cur);

                foreach (Bubble tmp in neighbors)
                {
                    if (!boardVisited[tmp.row, tmp.col])
                    {
                        stack.Push(tmp);
                        groupTemp.Add(tmp);
                        boardVisited[tmp.row, tmp.col] = true;
                    }
                }
            }
            group.Add(new Tuple<List<Bubble>, bool>(groupTemp, isConnectTop));
        }

        public void calculateScore()
        {
            Singleton.Instance.score += falledBubbles.Count * Math.Max(falledBubbles.Count - 2, 1) * 20;
        }

        public List<Bubble> bombActivate(Bubble bombBubble)
        {
            var neighbors = GetNeighbors(bombBubble);
            return neighbors;
        }
    }
}