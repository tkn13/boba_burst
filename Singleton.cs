using bubble_puzzle.GameObject;
using Microsoft.Xna.Framework.Input;

namespace bubbleTea
{
    class Singleton
    {
        private static Singleton instance;

        public GameBoard gameBoard;
        public Score scoreObject;
        public int score;
        public MouseState PreviousMouse, CurrentMouse;

        private Singleton()
        {
            gameBoard = new GameBoard(null);
            scoreObject = new Score(null);
            score = 0;
        }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }
    }
}
