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
        public GameBoard(Texture2D texture) : base(texture)
        {
            board = new int[13, 8];
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
                        //currentGameState = GameState.BubbleMatch;
                    }
                    break;
                case GameState.BubbleMatch:
                    // currentGameState = GameState.BubbleReload;
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
                    board[i, j] = -1;
                }
            }

            //Destroy all bubbles
            bubbles.Clear();

            //Spawn new bubbles for first 3 rows by first and second row have 8 bubbles and third row have 7 bubbles

            for (int j = 0; j < 8; j++)
            {
                Bubble bubble = new Bubble(null);
                bubble.Position = new Vector2((GameConstants.TILE_SIZE * j) + Position.X, (GameConstants.TILE_SIZE * 0) + Position.Y);
                int bubleType = bubble.RandomBubbleType(0.5f, new BubbleType[] { BubbleType.Red, BubbleType.Green, BubbleType.Blue, BubbleType.Yellow });
                bubble.setTexture(bubbleTexture[bubleType]);
                bubbles.Add(bubble);

                board[0, j] = bubleType;
                bubble.row = 0;
                bubble.col = j;
            }

            for (int i = 0; i < 7; i++)
            {
                Bubble bubble = new Bubble(null);
                bubble.Position = new Vector2((GameConstants.TILE_SIZE * i) + Position.X + (Position.X / 2), (GameConstants.TILE_SIZE * 1) + Position.Y);
                int bubleType = bubble.RandomBubbleType(0.5f, new BubbleType[] { BubbleType.Red, BubbleType.Green, BubbleType.Blue, BubbleType.Yellow });
                bubble.setTexture(bubbleTexture[bubleType]);
                bubbles.Add(bubble);

                board[1, i] = bubleType;
                bubble.row = 1;
                bubble.col = i;
            }

            for (int j = 0; j < 8; j++)
            {
                Bubble bubble = new Bubble(null);
                bubble.Position = new Vector2((GameConstants.TILE_SIZE * j) + Position.X, (GameConstants.TILE_SIZE * 2) + Position.Y);
                int bubleType = bubble.RandomBubbleType(0.5f, new BubbleType[] { BubbleType.Red, BubbleType.Green, BubbleType.Blue, BubbleType.Yellow });
                bubble.setTexture(bubbleTexture[bubleType]);
                bubbles.Add(bubble);

                board[2, j] = bubleType;
                bubble.row = 2;
                bubble.col = j;
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
                    if (!visited.Contains(neighbor) && neighbor.currentBubbleType == currentBubble.currentBubbleType)
                    {
                        visited.Add(neighbor);
                        stack.Push(neighbor);
                    }
                }
            }

            //Console.WriteLine(matchBubbles.Count);

            return matchBubbles;
        }

        private List<Bubble> GetNeighbors(Bubble bubble)
        {
            List<Bubble> neighbors = new List<Bubble>();

            int[][] positions = new int[][]
            {
                new int[] { -1,  0 }, // Top-left
                new int[] { -1,  1 }, // Top-right
                new int[] {  0, -1 }, // Left
                new int[] {  0,  1 }, // Right
                new int[] {  1, -1 }, // Bottom-left
                new int[] {  1,  0 }  // Bottom-right
            };

            foreach (int[] position in positions)
            {
                int newRow = bubble.row + position[0];
                int newCol = bubble.col + position[1];

                if (newRow >= 0 && newRow < board.GetLength(0) && newCol >= 0 && newCol < board.GetLength(1))
                {
                    // Check if there is a bubble in the specified position
                    Bubble neighbor = null;
                    foreach (Bubble b in bubbles)
                    {
                        if (b.row == newRow && b.col == newCol)
                        {
                            neighbor = b;
                            break;
                        }
                    }

                    if (neighbor != null)
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }

        public int checkFall(List<Bubble> bubbles)
        {
            int fallCount = 0;

            return fallCount;
        }

    }
}