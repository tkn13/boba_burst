using boba_burst.GameObject;
using Microsoft.Xna.Framework.Input;

namespace boba_burst
{
    class Singleton
    {
        private static Singleton instance;

        public GameBoard gameBoard;
        public Score scoreObject;
        public int score;
        public MouseState PreviousMouse, CurrentMouse;
        public KeyboardState PreviousKeyboard, CurrentKeyboard;

        //debug value
        public string MouseRotateValue = "0";
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
