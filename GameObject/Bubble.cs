using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace bubble_puzzle.GameObject
{
    public class Bubble : GameObject
    {
        public bool isHighlighted = false;
        public Texture2D highlightTexture;
        public BubbleType currentBubbleType;
        public int row, col;
        public Bubble(Texture2D texture) : base(texture)
        {
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 direaction = Velocity;
            direaction.Normalize();

            Position += direaction * GameConstants.MOVE_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //check if the bubble is collide with the right wall
            if (Position.X + GameConstants.TILE_SIZE > GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * 8))
            {
                Position.X = GameConstants.BOARD_POSITION.X + (GameConstants.TILE_SIZE * 8) - GameConstants.TILE_SIZE;
                Velocity.X *= -1;
            }

            //check if the bubble is collide with the left wall
            if (Position.X < GameConstants.BOARD_POSITION.X)
            {
                Position.X = GameConstants.BOARD_POSITION.X;
                Velocity.X *= -1;
            }


            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);

            //draw highlight red square cover the bubble with transparent
            if (isHighlighted)
            {
                //Texture2D highlightTexture = new Texture2D(spriteBatch.GraphicsDevice, GameConstants.HITBOX_SIZE, GameConstants.HITBOX_SIZE);

                Vector2 center = new Vector2(GameConstants.HITBOX_SIZE / 2, GameConstants.HITBOX_SIZE / 2);
                Vector2 centerBubble = new Vector2(Position.X + GameConstants.TILE_SIZE / 2, Position.Y + GameConstants.TILE_SIZE / 2);

                spriteBatch.Draw(highlightTexture, centerBubble, null, Color.White, Rotation, center, 1, SpriteEffects.None, 0);
            }

            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            base.Reset();
        }

        //Random Bubble Type Function with weight 
        //base from it's child bubbles there have a chance to spawn with the same type
        public int RandomBubbleType(List<Bubble> bubbles)
        {
            do
            {
                int randomIndex = GameConstants.random.Next(bubbles.Count);
                if (bubbles.Count > 0)
                {
                    currentBubbleType = bubbles[randomIndex].currentBubbleType;
                }
            } 
            while (currentBubbleType == BubbleType.Frozen || currentBubbleType == BubbleType.Bomb);

            return (int)currentBubbleType;
        }

        public void setTexture(Texture2D texture, Texture2D highlightTexture)
        {
            this.texture = texture;
            this.highlightTexture = highlightTexture;
        }

        /*
        Loop through all bubbles and check if the bubble is collide with other bubbles
        and select the closest bubble to collide with
        */
        public Bubble isCollide(List<Bubble> bubbles)
        {
            Bubble closestBubble = null;
            float minDistance = float.MaxValue;

            foreach (Bubble bubble in bubbles)
            {
                if (bubble != this)
                {
                    Vector2 centerRef = new Vector2(Position.X + GameConstants.TILE_SIZE / 2, Position.Y + GameConstants.TILE_SIZE / 2);
                    Vector2 centerBubble = new Vector2(bubble.Position.X + GameConstants.TILE_SIZE / 2, bubble.Position.Y + GameConstants.TILE_SIZE / 2);
                    if (Vector2.Distance(centerRef, centerBubble) < GameConstants.HITBOX_SIZE)
                    {
                        float distance = Vector2.Distance(centerRef, centerBubble);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestBubble = bubble;
                        }
                    }
                }
            }
            return closestBubble;
        }

        public bool isCollideWithRoof()
        {
            if (Position.Y < GameConstants.BOARD_POSITION.Y)
            {
                return true;
            }
            return false;
        }

        public void setFall(bool isFall)
        {   
            //set current position move a bit by random

            float randomX = GameConstants.random.Next(-GameConstants.TILE_SIZE / 2, GameConstants.TILE_SIZE / 2);
            float randomY = GameConstants.random.Next(-GameConstants.TILE_SIZE / 2, GameConstants.TILE_SIZE / 2);

            Position = new Vector2(Position.X + randomX, Position.Y + randomY);

            if (isFall)
            {
                Velocity = new Vector2(0, 1);
            }
            else
            {
                Velocity = Vector2.Zero;
            }
        }
    }
}