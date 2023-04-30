using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;
using System.Windows.Media.Media3D;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using System.Drawing;

namespace SnakeWPF
{
    class Node
    {
        public int x;
        public int y;
        public int f;
        public int g;
        public int h;
        public Node parent;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public class EnemyGameLogic
    {
        public int Rows { get; }
        public int Cols { get; }
        public Direction Dir { get; set; }
        public GridValue[,] Grid { get; }
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();

        public bool GameOver { get; private set; }
        private readonly Random random = new Random();

        public Position FoodPos { get; set; }
        public int foodCount { get; set; }
        public string text { get; set; }
        public int[,] map { get; set; }
        public EnemyGameLogic(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            foodCount = 0;
            Grid = new GridValue[rows, cols];
            map = new int[rows, cols];
            Step = 0;
            AddSnake();
            AddFood();
            FindPathToFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;
            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
            GameOver = false;
        }
        private void AddFood()
        {

            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0)
            {
                return;
            }
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
            FoodPos = pos;
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
            Position newHeadPos = GetNewDir();
            
            GridValue hit = WillHit(newHeadPos);
            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Food)
            {
                foodCount++;
                AddHead(newHeadPos);
                AddFood();
                FindPathToFood();
                

            }
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }
            return Grid[newHeadPos.Row, newHeadPos.Col];
        }


        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }
        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }
        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }
        public int Step { get; set; }
        public List<(int, int)> directions { get; set; }
        public Position GetNewDir()
        {
            var move = directions[0];
            int x = move.Item1;
            int y = move.Item2;
            directions.RemoveAt(0);
            text = Convert.ToString(x)+ " " + Convert.ToString(y);
            Position dir = new Position(x,y);
            return dir;
        }
        public void FindPathToFood()
        {
            int startX = snakePositions.First.Value.Row;
            int startY = snakePositions.First.Value.Col;
            int endX = FoodPos.Row;
            int endY = FoodPos.Col;
            //Если еда закрыта со всех сторон телом змейки или концом карты, попробуем найти безопасный путь
            bool flag = false;
            // Проверка наличия свободной клетки слева от искомой точки
            if (endX > 0 && map[endX - 1, endY] == 0)
            {
                // есть свободная клетка слева
                flag = true;
            }
            // Проверка наличия свободной клетки справа от искомой точки
            if (endX < Rows - 1 && map[endX + 1, endY] == 0)
            {
                // есть свободная клетка справа
                flag = true;
            }

            // Проверка наличия свободной клетки сверху от искомой точки
            if (endY > 0 && map[endX, endY - 1] == 0)
            {
                // есть свободная клетка сверху
                flag = true;
            }

            // Проверка наличия свободной клетки снизу от искомой точки
            if (endY < Cols - 1 && map[endX, endY + 1] == 0)
            {
                // есть свободная клетка снизу
                flag = true;
            }
            if (flag)
            {
                List<(int, int)> path = FindPath(startX, startY, endX, endY, map);
                directions = path;
            }
            else
            {
                
                List<(int, int)> path = FindSavePath();
                directions = path;
                
            }
            
            
        }
        public List<(int,int)> FindSavePath()
        {
            List<(int, int)> directions = new List<(int, int)>
            {
                (1, 1),
                (1, 3)
            };
            return directions;
        }
        static List<(int, int)> FindPath(int startX, int startY, int endX, int endY, int[,] map)
        {
            Node startNode = new Node(startX, startY);
            Node endNode = new Node(endX, endY);
            List<Node> openList = new List<Node>() { startNode };
            HashSet<Node> closedList = new HashSet<Node>();

            while (openList.Count > 0)
            {
                Node current = openList[0];

                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].f < current.f)
                    {
                        current = openList[i];
                    }
                }

                openList.Remove(current);
                closedList.Add(current);

                if (current.x == endX && current.y == endY)
                {
                    List<(int, int)> path = new List<(int, int)>();
                    Node node = current;

                    while (node != null)
                    {
                        path.Add((node.x, node.y));
                        node = node.parent;
                    }

                    path.Reverse();
                    return path;
                }

                foreach (Node neighbor in GetNeighbors(current,map))
                {
                    if (neighbor == null || closedList.Contains(neighbor))
                    {
                        continue;
                    }

                    int newCost = current.g + 1;

                    if (newCost < neighbor.g || !openList.Contains(neighbor))
                    {
                        neighbor.g = newCost;
                        neighbor.h = CalculateHeuristic(neighbor, endNode);
                        neighbor.f = neighbor.g + neighbor.h;
                        neighbor.parent = current;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }
            return null;
        }

        static List<Node> GetNeighbors(Node node, int[,] map)
        {
            List<Node> neighbors = new List<Node>();

            if (node.x > 0 && map[node.x - 1, node.y] != 1)
            {
                neighbors.Add(new Node(node.x - 1, node.y));
            }

            if (node.y > 0 && map[node.x, node.y - 1] != 1)
            {
                neighbors.Add(new Node(node.x, node.y - 1));
            }

            if (node.x < map.GetLength(0) - 1 && map[node.x + 1, node.y] != 1)
            {
                neighbors.Add(new Node(node.x + 1, node.y));
            }

            if (node.y < map.GetLength(1) - 1 && map[node.x, node.y + 1] != 1)
            {
                neighbors.Add(new Node(node.x, node.y + 1));
            }

            return neighbors;
        }
        static int CalculateHeuristic(Node node, Node endNode)
        {
            int dx = Math.Abs(node.x - endNode.x);
            int dy = Math.Abs(node.y - endNode.y);

            return dx + dy;
        }
    }
}