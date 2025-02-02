using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace boba_burst.GameObject
{
    public class Player : GameObject
    {
        public Player(Texture2D texture) : base(texture)
        {
            Position.X = GameConstants.BOARD_POSITION.X;
            Position.Y = GameConstants.BOARD_POSITION.Y + (GameConstants.TILE_SIZE * 9);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {   
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}