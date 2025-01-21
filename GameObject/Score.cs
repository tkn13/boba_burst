using System;
using bubbleTea;
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
            string number = Singleton.Instance.score.ToString("D8");
            int numberLength = number.Length;

            for (int i = 0; i < numberLength; i++)
            {
                spriteBatch.Draw(scoreTexture[int.Parse(number[i].ToString())], new Vector2(Position.X + (GameConstants.SCORE_TILE_SIZE * i), Position.Y), Color.White);
            }
            //Console.WriteLine(number);
            // string[] numberArray = number.Split("");
            // Console.WriteLine(numberArray);
            // Console.WriteLine(numberArray.Length);
            // for (int i = 0; i < numberArray.Length; i++)
            // {
            //     spriteBatch.Draw(scoreTexture[int.Parse(numberArray[i])], new Vector2(Position.X + (GameConstants.SCORE_TILE_SIZE * i), Position.Y), Color.White);
            // }
            
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}