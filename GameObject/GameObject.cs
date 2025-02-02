using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace boba_burst.GameObject
{   
    public class GameObject
    {
        public Texture2D texture;
        public Vector2 Position;
        public float Rotation;
        public Vector2 Scale;

        public Vector2 Velocity;

        public string Name;
        public GameObject(Texture2D texture)
        {
            this.texture = texture;
            Position = Vector2.Zero;
            Rotation = 0;
            Scale = Vector2.One;
            Velocity = Vector2.Zero;
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        public virtual void Reset()
        {
        }
    }
}