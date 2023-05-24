using Lucene.Net.Messages;
using Lucene.Net.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnakeWPF
{

    public class EnemyGameLogic
    {
        public int Rows { get; }
        public int Cols { get; }

        public GridValue[,] Grid { get; }
        public Queue<Point> Snake = new Queue<Point>();
        private Direction Dir { get; set; }
        public bool GameOver { get; private set; }
        private readonly Random random = new Random();
        private Point Mouse { get; set; }
        public int FoodCount { get; set; }
        public int rotation { get; set; }
        public EnemyGameLogic(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            FoodCount = 0;
            Grid = new GridValue[rows, cols];
            TempPath = new Queue<Point>();
            HamiltonPath = new List<Point>(rows * cols);
            rotation = 90;
            CreateHamiltonPath();
            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;
            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                Snake.Enqueue(new Point(r, c));
            }
            GameOver = false;
        }
        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0)
            {
                System.Windows.MessageBox.Show("No way");
                GameOver = true;
                return;
            }
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
            Mouse = new Point(pos.Row, pos.Col);
            CalculatePath();
        }
        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        
        public void Move()
        {
            SetDirection();
            Point head = Snake.Last();
            switch (Dir)
            {
                case Direction.Up:
                    rotation = 0;
                    Snake.Enqueue(new Point(head.X - 1, head.Y));
                    break;
                case Direction.Left:
                    rotation = 270;
                    Snake.Enqueue(new Point(head.X, head.Y - 1));
                    break;
                case Direction.Down:
                    rotation = 180;
                    Snake.Enqueue(new Point(head.X + 1, head.Y));
                    break;
                case Direction.Right:
                    rotation = 90;
                    Snake.Enqueue(new Point(head.X, head.Y + 1));
                    break;
            }
            Point newHead = Snake.Last();
            GridValue hit = WillHit(newHead);
            if(hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                Point tail = Snake.First();
                Grid[tail.X, tail.Y] = GridValue.Empty;
                Grid[newHead.X, newHead.Y] = GridValue.Snake;
                Snake.Dequeue();
            }
            else if (hit == GridValue.Food)
            {
                FoodCount++;
                Grid[newHead.X, newHead.Y] = GridValue.Snake;
                AddFood();
            }
            if (Snake.Count < StepsCountAfterCalculatePath)
            {
                CalculatePath();
            }
        }

        private GridValue WillHit(Point newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                GameOver = true;
                return GridValue.Outside;
            }
            if (newHeadPos == Snake.First())
            {
                return GridValue.Empty;
            }
            return Grid[newHeadPos.X, newHeadPos.Y];
        }
        private bool OutsideGrid(Point pos)
        {
            return pos.X < 0 || pos.X >= Rows || pos.Y < 0 || pos.Y >= Cols;
        }

        //habr
        private bool InvertHamiltonPath { get; set; }
        private Queue<Point> TempPath { get; set; }
        private int StepsCountAfterCalculatePath { get; set; }
        private List<Point> HamiltonPath { get; set; }

        private void CreateHamiltonPath()
        {
            HamiltonPath.Clear();
            HamiltonPath.Add(new Point(0, 0));
            HamiltonStep(HamiltonPath.Last());
        }
        private enum Direction { Up, Down, Left, Right }
        private bool HamiltonStep(Point current)
        {
            int xSize = Rows;
            int ySize = Cols;

            if (HamiltonPath.Count == HamiltonPath.Capacity)
            {
                var first = HamiltonPath.First();
                return (first.X == current.X && first.Y == current.Y - 1)
                    || (first.X == current.X && first.Y == current.Y + 1)
                    || (first.X - 1 == current.X && first.Y == current.Y)
                    || (first.X + 1 == current.X && first.Y == current.Y);
            }

            foreach (var direction in new[] { Direction.Down, Direction.Right, Direction.Up, Direction.Left })
            {
                Point newElement = new(0,0);
                switch (direction)
                {
                    case Direction.Up:
                        newElement = new Point(current.X - 1, current.Y);
                        break;
                    case Direction.Left:
                        newElement = new Point(current.X, current.Y - 1);
                        break;
                    case Direction.Down:
                        newElement = new Point(current.X + 1, current.Y);
                        break;
                    case Direction.Right:
                        newElement = new Point(current.X, current.Y + 1);
                        break;
                }
                if (0 <= newElement.X && newElement.X < xSize
                    && 0 <= newElement.Y && newElement.Y < ySize
                    && !HamiltonPath.Contains(newElement))
                {
                    HamiltonPath.Add(newElement);
                    if (HamiltonStep(newElement))
                    {
                        return true;
                    }
                    HamiltonPath.Remove(newElement);
                }
            }
            return false;
        }

        private void SetDirection()
        {
            Point head = Snake.Last();
            int currentIndnex = HamiltonPath.FindIndex(p => p.X == head.X && p.Y == head.Y);
            Point currentElement = HamiltonPath[currentIndnex];
            Point nextElement = new(0,0);
            if (TempPath.Count > 0)
            {
                nextElement = TempPath.Dequeue();
            }
            else
            {
                StepsCountAfterCalculatePath++;
                if (InvertHamiltonPath)
                {
                    nextElement = (currentIndnex - 1 < 0) ? HamiltonPath[HamiltonPath.Count - 1] : HamiltonPath[currentIndnex - 1];
                }
                else
                {
                    nextElement = (currentIndnex + 1 == HamiltonPath.Count) ? HamiltonPath[0] : HamiltonPath[currentIndnex + 1];
                }
            }

            if (currentElement.X < nextElement.X && currentElement.Y == nextElement.Y)
            {
                Dir = Direction.Down;
                return;
            }

            if (currentElement.X == nextElement.X && currentElement.Y < nextElement.Y)
            {
                Dir = Direction.Right;
                return;
            }

            if (currentElement.X > nextElement.X && currentElement.Y == nextElement.Y)
            {
                Dir = Direction.Up;
                return;
            }

            if (currentElement.X == nextElement.X &&  currentElement.Y > nextElement.Y)
            {
                Dir = Direction.Left;
                return;
            }

            

            
        }
        private void CalculatePath()
        {
            StepsCountAfterCalculatePath = 0;
            int finalIndexPoint = HamiltonPath.FindIndex(p => p.X == Mouse.X && p.Y == Mouse.Y);
            List<Point> tempPath = new List<Point>();
            List<Point> points = Snake.Select(p => new Point(p.X, p.Y)).ToList();
            List<Point> stepPiton = new List<Point>(points);
            int index = 0;
            var result = StepTempPath(ref index, GetInvert(stepPiton, Mouse), Snake.Last(), finalIndexPoint, stepPiton, tempPath);
            if (result.PathIsFound)
            {
                TempPath = new Queue<Point>(tempPath);
                InvertHamiltonPath = result.InvertHamiltonPath;
            }
        }
        private bool GetInvert(List<Point> stepPiton, Point finalPoint)
        {
            if (Snake.Count > 1)
            {
                int pitonDirection = stepPiton.Last().Y - stepPiton[stepPiton.Count - 2].Y;
                int mouseDirection = stepPiton.Last().Y - finalPoint.Y;
                return (pitonDirection < 0 && mouseDirection < 0) || (pitonDirection > 0 && mouseDirection > 0);
            }
            return false;
        }

        class ResultAnlaizePath
        {
            public bool PathIsFound { get; set; }
            public bool InvertHamiltonPath { get; set; }
            public ResultAnlaizePath(bool pathIsFound, bool invertHamiltonPath = false)
            {
                PathIsFound = pathIsFound;
                InvertHamiltonPath = invertHamiltonPath;
            }
        }
        private ResultAnlaizePath StepTempPath(ref int index, bool invert, Point current, int finalIndexPoint, List<Point> stepPiton, List<Point> tempPath)
        {
            index++;
            if (HamiltonPath.Count < index)
            {
                return new ResultAnlaizePath(false);
            }
            var finalPoint = HamiltonPath[finalIndexPoint];
            if (current.X == finalPoint.X && current.Y == finalPoint.Y)
            {
                if (Snake.Count == 1)
                {
                    return new ResultAnlaizePath(true);
                }
                foreach (var d in new[] { false, true })
                {
                    var tempPiton = stepPiton.TakeLast(Snake.Count).ToList();
                    bool isFound = true;
                    bool invertHamiltonPath = d;
                    for (int j = 1; j < Snake.Count; j++)
                    {
                        Point hamiltonPoint;
                        if (invertHamiltonPath)
                        {
                            hamiltonPoint = (finalIndexPoint - j >= 0) ? HamiltonPath[finalIndexPoint - j] : HamiltonPath[HamiltonPath.Count - j];
                        }
                        else
                        {
                            hamiltonPoint = (finalIndexPoint + j < HamiltonPath.Count) ? HamiltonPath[finalIndexPoint + j] : HamiltonPath[finalIndexPoint + j - HamiltonPath.Count];
                        }
                        if (tempPiton.TakeLast(Snake.Count).Contains(hamiltonPoint))
                        {
                            isFound = false;
                            break;
                        }
                        tempPiton.Add(hamiltonPoint);
                    }
                    if (isFound)
                    {
                        return new ResultAnlaizePath(true, invertHamiltonPath);
                    }
                }
                return new ResultAnlaizePath(false);
            }
            if ((Rows + Cols * 2) <= tempPath.Count)
            {
                return new ResultAnlaizePath(false);
            }
            Point newElement = new(0,0);

            if (invert)
            {
                if (current.X < finalPoint.X)
                {
                    newElement = new Point(current.X + 1, current.Y);
                }
                else if (finalPoint.X < current.X)
                {
                    newElement = new Point(current.X - 1, current.Y);
                }
                else if (current.Y < finalPoint.Y)
                {
                    newElement = new Point(current.X, current.Y + 1);
                }
                else if (finalPoint.Y < current.Y)
                {
                    newElement = new Point(current.X, current.Y - 1);
                }
            }
            else
            {
                if (current.Y < finalPoint.Y)
                {
                    newElement = new Point(current.X, current.Y + 1);
                }
                else if (finalPoint.Y < current.Y)
                {
                    newElement = new Point(current.X, current.Y - 1);
                }
                else if (current.X < finalPoint.X)
                {
                    newElement = new Point(current.X + 1, current.Y);
                }
                else if (finalPoint.X < current.X)
                {
                    newElement = new Point(current.X - 1, current.Y);
                }
            }

            if (!stepPiton.TakeLast(Snake.Count).Contains(newElement))
            {
                tempPath.Add(newElement);
                stepPiton.Add(newElement);
                var retult = StepTempPath(ref index, !invert, newElement, finalIndexPoint, stepPiton, tempPath);
                if (retult.PathIsFound)
                {
                    return retult;
                }
                if (HamiltonPath.Count < index)
                {
                    return new ResultAnlaizePath(false);
                }
                tempPath.Remove(newElement);
                stepPiton.Remove(newElement);
            }

            Point nextFinalPoint;
            if (this.InvertHamiltonPath)
            {
                nextFinalPoint = (finalIndexPoint - 1 < 0) ? HamiltonPath[HamiltonPath.Count - 1] : HamiltonPath[finalIndexPoint - 1];
            }
            else
            {
                nextFinalPoint = (finalIndexPoint + 1 == HamiltonPath.Count) ? HamiltonPath[0] : HamiltonPath[finalIndexPoint + 1];
            }
            List<Direction> directions = new List<Direction>(4);
            directions.Add(finalPoint.Y < nextFinalPoint.Y ? Direction.Left : Direction.Right);
            directions.Add(finalPoint.X < nextFinalPoint.X ? Direction.Up : Direction.Down);
            directions.Add(finalPoint.Y < nextFinalPoint.Y ? Direction.Right : Direction.Left);
            directions.Add(finalPoint.X < nextFinalPoint.X ? Direction.Down : Direction.Up);

            foreach (var direction in directions)
            {
                switch (direction)
                {
                    case Direction.Up:
                        newElement = new Point(current.X - 1, current.Y);
                        break;
                    case Direction.Left:
                        newElement = new Point(current.X, current.Y - 1);
                        break;
                    case Direction.Down:
                        newElement = new Point(current.X + 1, current.Y);
                        break;
                    case Direction.Right:
                        newElement = new Point(current.X, current.Y + 1);
                        break;
                }
                if (0 <= newElement.X && newElement.X < Rows
                 && 0 <= newElement.Y && newElement.Y < Cols
                 && !stepPiton.TakeLast(Snake.Count).Contains(newElement))
                {
                    tempPath.Add(newElement);
                    stepPiton.Add(newElement);
                    var retult = StepTempPath(ref index, GetInvert(stepPiton, finalPoint), newElement, finalIndexPoint, stepPiton, tempPath);
                    if (retult.PathIsFound)
                    {
                        return retult;
                    }
                    if (HamiltonPath.Count < index)
                    {
                        return new ResultAnlaizePath(false);
                    }
                    tempPath.Remove(newElement);
                    stepPiton.Remove(newElement);
                }
            }
            return new ResultAnlaizePath(false);
        }
    }
}