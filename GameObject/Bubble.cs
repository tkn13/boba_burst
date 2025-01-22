using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace bubble_puzzle.GameObject
{
    public class Bubble : GameObject
    {
        public bool isHighlighted = false;
        public BubbleType currentBubbleType;
        public int row, col;
        public Bubble(Texture2D texture) : base(texture)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);

            //draw highlight red square cover the bubble with transparent
            if (isHighlighted)
            {
                Texture2D highlightTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                highlightTexture.SetData(new Color[] { Color.Red * 0.5f });

                spriteBatch.Draw(highlightTexture, Position, null, Color.White, Rotation, Vector2.Zero, GameConstants.HITBOX_SIZE, SpriteEffects.None, 0);
            }

            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            base.Reset();
        }

        //Random Bubble Type Function with weight 
        //base from it's child bubbles there have a chance to spawn with the same type
        public int RandomBubbleType(float weight, BubbleType[] type)
        {
            currentBubbleType = type[GameConstants.random.Next(0, type.Length)];

            return (int)currentBubbleType;
        }

        public void setTexture(Texture2D texture)
        {
            this.texture = texture;
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
    }
}