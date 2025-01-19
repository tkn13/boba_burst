using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace bubble_puzzle.GameObject
{
    public class Score : GameObject
    {
        public Texture2D [] scoreTexture;
    
        public Score(Texture2D texture) : base(texture)
        {
            scoreTexture = new Texture2D[10];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        //Draw score until 8 digits base on Singletons score by using scoreTexture[i]
        //first index is 0 and last index is 9
        public override void Draw(SpriteBatch spriteBatch)
        {   
            for(int i = 0; i < 8; i++)
            {
                spriteBatch.Draw(scoreTexture[0], new Vector2(Position.X + (i * 32), Position.Y), null, Color.White, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}