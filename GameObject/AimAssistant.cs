using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace boba_burst.GameObject
{
    public class AimAssistant : GameObject
    {
        public AimAssistant(Texture2D texture) : base(texture)
        {
            Position = GameConstants.SHOOT_POSITION;
            Position.Y += 256;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {   
            Vector2 temp = GameConstants.SHOOT_PIVOT_POSITION;
            Vector2 origin = new Vector2(texture.Width / 2, 256);
            spriteBatch.Draw(texture, temp, null, Color.White, Rotation, origin, Scale, SpriteEffects.None, 0);
            
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}