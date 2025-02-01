using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace bubble_puzzle.GameObject
{
    public class GameButton : GameObject
    {
        public GameButton(Texture2D texture) : base(texture)
        {
            this.texture = texture;
           
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            base.Reset();
        }

        public bool IsClicked()
        {
            if (Singleton.Instance.CurrentMouse.LeftButton == ButtonState.Pressed &&
                Singleton.Instance.PreviousMouse.LeftButton == ButtonState.Released
                )
            {
                if(Singleton.Instance.CurrentMouse.X > Position.X &&
                   Singleton.Instance.CurrentMouse.X < Position.X + texture.Width &&
                   Singleton.Instance.CurrentMouse.Y > Position.Y &&
                   Singleton.Instance.CurrentMouse.Y < Position.Y + texture.Height)
                {
                    return true;
                }
            }
            return false;
        }
    }
}