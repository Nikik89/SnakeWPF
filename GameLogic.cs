using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeWPF
{
    internal class GameLogic
    {
        public int Rows { get; }
        public int Columns { get; }

        public GridValue[,] Grid { get; }

        public GameLogic(int rows, int cols)
        {
            Rows = rows;
            Columns = cols;
            Grid = new GridValue[rows, cols];

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;
            for(int c = 1; c<= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                
            }
        }

        private void AddFood()
        {

        }
        public void Move()
        {
            //newHeadPos =
            //RemoveTail();
            //AddHead(newHeadPos);
        }

        private void AddHead()
        {

        }
        private void RemoveTail()
        {

        }
    }
}
