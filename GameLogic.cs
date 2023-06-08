using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;


namespace SnakeWPF
{
    public class Position //класс, который хранит координаты клетки на игровом поле
    {

        //свойства, отвечающие за номер строки и столбца
        public int Row { get; }
        public int Col { get; }

        public Position(int row, int col) //конструктор класса для установки значений Row и Col
        {
            Row = row;
            Col = col;
        }

        public Position Translate(Direction dir) //метод, который принимает направление движения и возвращает новые координаты клетки
        {
            return new Position(Row + dir.RowOffset, Col + dir.ColOffset);
        }
    }
    public class Direction //определение класса Direction, который представляет собой перечисление направлений движения
    {
        public readonly static Direction Left = new(0, -1);     //
        public readonly static Direction Right = new(0, 1);     // создание 4 статических "экземпляров" направлений 
        public readonly static Direction Up = new(-1, 0);       //          с фиксированными значениями
        public readonly static Direction Down = new(1, 0);      //

        public int RowOffset { get; }   //переменные для хранения 
        public int ColOffset { get; }   //смещений по строкам и столбцам
        private Direction(int rowOffset, int colOffset) //конструктор класса Direction для определения значений полей RowOffset и ColOffset
        {
            RowOffset = rowOffset;
            ColOffset = colOffset;
        }
    }
    public class GameLogic //определение класса GameLogic, отвечающего за логику движения змейки игрока
    {
        public int Rows { get; } //переменные для хранения кол-ва строк
        public int Cols { get; } //и столбцов
        public Direction Dir { get; private set; } //переменная для хранения направления движения змейки
        private readonly LinkedList<Position> snakePositions = new(); //связанный список позиций змейки
        public int FoodCount { get; set; } //переменная для хранения кол-ва съеденной еды
        public bool GameOver { get; private set; } //флаг окончания игры
        private readonly Random random = new(); //переменная для генерации случайных чисел
        public Image[,] MegaGrid { get; set; } //двумерный массив (игровая сетка)

        //конструктор класса GameLogic для инициализации значений полей Rows, Cols, MegaGrid, Dir, FoodCount
        //и заполнения списка позиций змейки и добавления еды на поле
        public GameLogic(int rows, int cols, Image[,] grid) 
        {
            Rows = rows;
            Cols = cols;
            MegaGrid = grid;
            Dir = Direction.Right;
            FoodCount = 0;
            AddSnake();
            AddFood();
        }

        private void AddSnake() //метод для создания змейки в начале игры
        {
            int r = Rows / 2; //определяем положение змейки (находится в середине поля)
            for (int c = 1; c <= 3; c++) //цикл для добавления тела змейки, состоящего из 3 клеток
            {
                UpdateCell(new Position(r, c), Images.Body); //устанавливает изображение "Body" змейки в заданной позиции
                snakePositions.AddFirst(new Position(r, c)); //добавляет новую клетку тела змейки в список ее тела
                
            }
            GameOver = false; //сбрасывает флаг окончания игры
        }

        private void AddFood() //метод для добавления еды на поле
        {
            List<Position> empty = new(EmptyPositions()); //получает список пустых клеток на сетке
            if (empty.Count == 0) //если нет пустых клеток игра заканчивается
            {
                GameOver = true; //устанавливает флаг окончания игры на true 
                return;
            }
            Position pos = empty[random.Next(empty.Count)]; //получает случайную пустую клетку на поле 
            UpdateCell(pos, Images.Food); //устанавливает изображение "Food" в заданной клетке
        }
        private IEnumerable<Position> EmptyPositions() //метод возвращающий пустые клетки на поле
        {
            for (int r = 0; r < Rows; r++) //цикл по строкам сетки
            {
                for (int c = 0; c < Cols; c++) //вложенный цикл по столбцам сетки
                {
                    if (MegaGrid[r, c].Source == Images.Empty) //проверка, является ли клетка пустой
                    {
                        yield return new Position(r, c); //если клетка пустая, возвращает ее координаты
                    }
                }
            }
        }

        


        public void Move() //определение метода для перемещения змейки на игровом поле
        {
            if(dirChanges.Count > 0) //проверка, есть ли изменения направления движения в очереди
            {
                //если есть, изменяет направление движение змейки
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }
            //отрисовываем тело(Images.Body) змейки, так как голова(Images.Head) змейки занимает позицию тела и продолжает дублитроваться
            Position bodyPos = HeadPosition(); //определяет позицию головы змейки
            MegaGrid[bodyPos.Row, bodyPos.Col].Source = Images.Body; //обновляет изображение тела змейки в соответствии с ее текущим положение на игровом поле
            
            Position newHeadPos = HeadPosition().Translate(Dir); //***определяет новую позицию головы змейки после перемещения в указанном направлении****
            ImageSource hit = WillHit(newHeadPos); //проверка, на что наткнется змейка на этой позиции
            if(hit == Images.Outside || hit == Images.Body) //врезается ли змейка в стену или свое тело
            {
                GameOver = true; //устанавливает флаг окончания игры
            }
            else if (hit == Images.Empty) //наступает ли змейка на пустую клетку
            {
                RemoveTail(); //удаляет хвост
                AddHead(newHeadPos); //добавляет новую голову
            }
            else if (hit == Images.Food) //наступает ли змейка на клетку с едой
            {
                FoodCount++; //увеличивает счетчик съеденной еды
                AddHead(newHeadPos); //добавляет новую голову
                AddFood(); //добавляет еду на поле
            }
        }

        private bool OutsideGrid(Position pos) //метод проверяет, находится ли переданная позиция за границами игровой сетки
        {
            //если строка позиции меньше нуля или больше или равна кол-ву строк в сетке,
            //или если столбец позиции меньше нуля или больше или равен кол-ву столбцов в сетке,
            //то позиция находится за границами сетки и метод возвращает true
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        private ImageSource WillHit(Position newHeadPos) //метод проверяет, во что врезается голова змейки
        {
            if (OutsideGrid(newHeadPos)) //находится ли новая позиция головы змейки за границами игровой сетки
            {
                return Images.Outside; //возвращает Images.Outside равный null
            }
            if(newHeadPos == TailPos()) //находится ли новая позиция головы змейки в ее хвосте 
            {
                return Images.Empty; //возвращает Images.Empty
            }
            return MegaGrid[newHeadPos.Row, newHeadPos.Col].Source; //иначе метод возвращает изображение головы змейки
        }

        private Direction GetLastDirection() //метод возвращает последнее направление движения змейки
        {
            if(dirChanges.Count == 0) //не было ли изменения направления
            {
                return Dir; //возвращает текущее направление
            }
            return dirChanges.Last.Value; //иначе, возвращает последний элемент списка изменений направления
        }

        private bool CanChangeDirection(Direction newDir) //метод проверяет возможность изменить направление движения змейки
        {
            if(dirChanges.Count == 2) //если уже было 2 изменения направления, то доп. изменение невозможно
            {
                return false;
            }
            
            Direction lastDir = GetLastDirection(); //получает последнее направление движения
            return newDir != lastDir && newDir != Opposite(lastDir); //проверяет не равно ли новое направление последнему и противоположному ему направлению
        }

        private static Direction Opposite(Direction lastDir) //метод возвращает противоположное направление движения переданного направления
        {
            if (lastDir == Direction.Right) return Direction.Left; //выбирает противоположное направление в зависимости от переданного
            else if (lastDir == Direction.Up) return Direction.Down;
            else if (lastDir == Direction.Left) return Direction.Right;
            else return Direction.Up;
        }

        private readonly LinkedList<Direction> dirChanges = new(); //хранит список изменений направления движения змейки
        public void ChangeDirection(Direction dir) //метод, меняющий направление движения на переданное
        {
            if (CanChangeDirection(dir)) //проверка, можно ли изменить направление
            {
                dirChanges.AddLast(dir); //если да, то предает новое направление в конец списка изменений направления
            }
        }


        private void AddHead(Position pos) //метод добавляет новую клетку в начало змейки
        {
            snakePositions.AddFirst(pos); //добавляет новую клетку в начало списка клеток змейки
            UpdateCell(pos, Images.Body); //обновляет изображение ячейки
        }
        private void RemoveTail() //метод удаляет последнюю клетку хвоста змейки
        {
            Position tail = snakePositions.Last.Value; //получает последнюю клетку тела змейки
            UpdateCell(tail, Images.Empty); //обновляет изображение ячейки с удаленной позицией на изображение пустой клетки
            snakePositions.RemoveLast(); //удаляет последнюю позицию из списка клеток змейки
        }

        private void UpdateCell(Position cell, ImageSource value) //****метод обновляет изображение ячейки с переданной позицией и устанавливает трансформацию по умолчанию
        {
            MegaGrid[cell.Row, cell.Col].Source = value; //обновляет изображение ячейки с переданной позицией
            MegaGrid[cell.Row, cell.Col].RenderTransform = Transform.Identity; //****устанавливает трансформацию по умолчанию
        }

        public Position HeadPosition() //метод возращает положение головы змейки
        {
            return snakePositions.First.Value; //возращает первый элемент списка позиций змейки (клетка головы)
        }
        public Position TailPos() //метод возращает позицию хвоста змейки
        {
            return snakePositions.Last.Value; //возвращает последний элемент списка позиций змейки (клетка хвоста)
        }
    }
}
