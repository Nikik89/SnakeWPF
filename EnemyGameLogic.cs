
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;

namespace SnakeWPF
{

    public class EnemyGameLogic //определение класса EnemyGameLogic, отвечающего за логику движения змейки компьютера
    {
        //определение свойств класса, отвечающих за количество строк и столбцов в сетке
        public int Rows { get; }
        public int Cols { get; }

        //создание очереди для хранения клеток тела змейки и определение направления движения
        public Queue<Point> Snake = new Queue<Point>();
        private Direction Dir { get; set; }

        //определение состояния игры и объекта для генерации случайных чисел
        public bool GameOver { get; private set; }
        private readonly Random random = new Random();

        //определение координат еды, количества съеденной еды и угла поворота змейки
        private Point Food { get; set; }
        public int FoodCount { get; set; }
        public int Rotation { get; set; }

        //определение двумерного массива изображений, используемых для отображения игрового поля
        public Image[,] MegaGrid { get; set; }

        //конструктор класса EnemyGameLogic для инициализации значений полей Rows, Cols, FoodCount, MegaGrid и
        //заполнения списка позиций змейки, добавления еды на поле и добавления гамильтонова пути для змейки
        public EnemyGameLogic(int rows, int cols, Image[,] grid)
        {
            Rows = rows;
            Cols = cols;
            FoodCount = 0;
            MegaGrid = grid;
            TempPath = new Queue<Point>(); //инициализация очереди точек для построения гамильтонова пути
            HamiltonPath = new List<Point>(rows * cols); //инициализация списка точек, представляющих гамильтонов путь
            Rotation = 90;
            CreateHamiltonPath();
            AddSnake();
            AddFood();
        }
        private void AddSnake()//метод, добавляющий змейку компьютера на поле
        {
            int r = Rows / 2; //определяем расположение змейки в начале игры
            for (int c = 1; c <= 3; c++) //цикл для добавления тела змейки, состоящего из 3 клеток
            {
                UpdateCell(new Point(r, c), Images.Body); //устанавливает изображение "Body" змейки в заданной позиции
                Snake.Enqueue(new Point(r, c)); //добавляет новую клетку тела змейки в очередь ее тела
            }
            GameOver = false; //сбрасывает флаг окончания игры
        }
        private void UpdateCell(Point cell, ImageSource value) //метод, обновляющий содержимое ячейки в указанной позиции
        {
            MegaGrid[cell.X, cell.Y].Source = value; //устанавливает новое изображение в выбранную ячейку
            MegaGrid[cell.X, cell.Y].RenderTransform = Transform.Identity; //сбрасывает любые преобразования, которые могли быть применены к ячейке ранее
        }
        private void AddFood() //метод, добавляющий еду на поле
        {
            List<Point> empty = new(EmptyPositions()); //получает список пустых клеток на поле
            if (empty.Count == 0) //если нет пустых клеток
            {
                System.Windows.MessageBox.Show("No way");  //вывод текста "No way"
                GameOver = true; //устанавливает флаг окончания игры на true
                return;
            }
            Point pos = empty[random.Next(empty.Count)]; //выбирает случайную клетку из списка пустых клеток
            UpdateCell(pos, Images.Food); //обновляет содержимое клетки на изображение еды
            Food = new Point(pos.X, pos.Y); //запоминает позицию новой еды
            CalculatePath(); //пересчитывает путь до еды
        }
        private IEnumerable<Point> EmptyPositions() //метод для получения коллекции пустых клеток на сетке
        {
            for (int r = 0; r < Rows; r++) //проходит по всем строкам и столбцам
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (MegaGrid[r, c].Source == Images.Empty) //если клетка в текущей позиции пустая
                    {
                        yield return new Point(r, c); //добавляет ее координаты в коллекцию
                    }
                }
            }
        }


        public void Move()
        {
            SetDirection(); //устанавливает направление движения змейки
            Point head = Snake.Last(); //получает координаты головы змейки
            //отрисовываем тело(Images.Body) змейки, так как голова(Images.Head) змейки занимает позицию тела и продолжает дублироваться
            MegaGrid[head.X,head.Y].Source = Images.Body;
            switch (Dir)
            {
                case Direction.Up:
                    Rotation = 0; //определяет угол поворота при движении вверх
                    Snake.Enqueue(new Point(head.X - 1, head.Y)); //добавляет новую точку в тело змейки
                    break;
                case Direction.Left:
                    Rotation = 270; //определяет угол поворота при движении влево
                    Snake.Enqueue(new Point(head.X, head.Y - 1)); //добавляет новую точку в тело змейки
                    break;
                case Direction.Down:
                    Rotation = 180; //определяет угол поворота при движении вниз
                    Snake.Enqueue(new Point(head.X + 1, head.Y)); //добавляет новую точку в тело змейки
                    break;
                case Direction.Right:
                    Rotation = 90; //определяет угол поворота при движении вправо
                    Snake.Enqueue(new Point(head.X, head.Y + 1)); //добавляет новую точку в тело змейки
                    break;
            }
            Point newHead = Snake.Last(); //получает новые координаты головы змейки после перемещения
            ImageSource hit = WillHit(newHead); //проверка, ударится ли змейка о стену или себя после перемещения
            if (hit == Images.Outside || hit == Images.Body) //если змейка ударится о стену или саму себя, то игра закончится
            {
                GameOver = true;
            }
            else if (hit == Images.Empty) //если на новой точке пусто, то змейка продолжит движение
            {
                Point tail = Snake.First(); //получает координаты хвоста(последней клетки тела) змейки
                UpdateCell(tail, Images.Empty); //очищает ячейку, на которой был хвост
                UpdateCell(newHead, Images.Body); //устанавливает ячейку тела на клетку, на которой была голова змейки
                Snake.Dequeue(); //удаляет последний элемент из тела змейки
            }
            else if (hit == Images.Food) //если на новой точке была еда, то змейка ее съест
            {
                FoodCount++; //счётчик съеденной еды
                UpdateCell(newHead, Images.Body); //устанавливает ячейку тела на клетку, на которой была голова змейки
                AddFood(); //добавляет еду на поле
            }

            if (Snake.Count < StepsCountAfterCalculatePath) //проверка, если количество элементов змейки меньше заданного порога, то пересчитывает маршрут
            {
                CalculatePath(); //метод пересчета маршрута
            }

        }

        private ImageSource WillHit(Point newHeadPos) //метод проверки ударится ли змейка о границу поля и о саму себя
        {
            if (OutsideGrid(newHeadPos)) //проверка, вышла ли голова змейки за сетку 
            {
                GameOver = true; //заканчивает игру
                return Images.Outside; //возвращает значение "Outside" из класса Images, равное null
            }
            if (newHeadPos == Snake.First()) //если новая позиция совпадает с координатами первого элемента тела змейки, то возвращает значение "Empty"
            {
                return Images.Empty;
            }
            return MegaGrid[newHeadPos.X, newHeadPos.Y].Source; //возвращает текущее значение в сетке по указанным координатам
        }
        private bool OutsideGrid(Point pos) //проверка, находится ли голова вне сетки
        {
            return pos.X < 0 || pos.X >= Rows || pos.Y < 0 || pos.Y >= Cols; //если координаты выходят за пределы сетки, возвращаем true, иначе false
        }

        //habr
        private bool InvertHamiltonPath { get; set; } //переменная, нужно ли инвертировать гамильтонов путь
        private Queue<Point> TempPath { get; set; } //очередь ячеек, используемая для хранения временного гамильтонова пути
        private int StepsCountAfterCalculatePath { get; set; } //счетчик шагов, выполненных после расчета пути
        private List<Point> HamiltonPath { get; set; } //список ячеек, составляющих путь гамильтона

        private void CreateHamiltonPath() //метод, который реализует алгоритм создания гамильтонова пути на сетке
        {
            HamiltonPath.Clear(); //очищает список ячеек, составляющих путь гамильтона
            HamiltonPath.Add(new Point(0, 0)); //добавляет начальную точку (0, 0) в путь гамильтона
            HamiltonStep(HamiltonPath.Last()); //вызывает метод HamiltonStep для расчета гамильтонова пути
        }
        private enum Direction { Up, Down, Left, Right } //перечисление направлений движения
        private bool HamiltonStep(Point current) //метод, который реализует алгоритм поиска гамильтонова пути на сетке
        {
            int xSize = Rows; //количество строк в сетке
            int ySize = Cols; //количество столбцов в сетке

            if (HamiltonPath.Count == HamiltonPath.Capacity) // если путь уже содержит все вершины графа 
            {
                var first = HamiltonPath.First(); //устанавливает первую точку гамильтонова пути
                return (first.X == current.X && first.Y == current.Y - 1)
                    || (first.X == current.X && first.Y == current.Y + 1)
                    || (first.X - 1 == current.X && first.Y == current.Y)
                    || (first.X + 1 == current.X && first.Y == current.Y); //проверка, можно ли вернуться в начальную точку
            }

            foreach (var direction in new[] { Direction.Down, Direction.Right, Direction.Up, Direction.Left }) //для каждого направления движения
            {
                Point newElement = new(0, 0);
                switch (direction)
                {
                    case Direction.Up:
                        newElement = new Point(current.X - 1, current.Y); //вычисляет новую точку, перемещаясь вверх 
                        break;
                    case Direction.Left:
                        newElement = new Point(current.X, current.Y - 1); //вычисляет новую точку, перемещаясь влево
                        break;
                    case Direction.Down:
                        newElement = new Point(current.X + 1, current.Y); //вычисляет новую точку, перемещаясь вниз
                        break;
                    case Direction.Right:
                        newElement = new Point(current.X, current.Y + 1); //вычисляет новую точку, перемещаясь вправо
                        break;
                }
                if (0 <= newElement.X && newElement.X < xSize       //
                    && 0 <= newElement.Y && newElement.Y < ySize    //если новая точка находится в пределах сетки и не посещена ранее
                    && !HamiltonPath.Contains(newElement))          //
                {
                    HamiltonPath.Add(newElement); //добавляет эту точку в гамильтонов путь
                    if (HamiltonStep(newElement)) //рекурсия для новой точки
                    {
                        return true; //если удалось построить гамильтонов путь, возвращает true
                    }
                    HamiltonPath.Remove(newElement); //иначе убирает эту точку из пути и продолжает поиск
                }
            }
            return false; //если гамильтонов путь не найден, возвращает false
        }

        private void SetDirection() //метод устанавливающий направление движения змейки
        {
            Point head = Snake.Last(); //получает координаты головы змейки (последний элемент в списке)
            int currentIndnex = HamiltonPath.FindIndex(p => p.X == head.X && p.Y == head.Y); //находит индекс текущей точки головы в маршруте
            Point currentElement = HamiltonPath[currentIndnex]; //получает координаты текущей точки маршрута
            Point nextElement; //объявляет переменную для хранения координат следующей точки пути
            if (TempPath.Count > 0) //если временный путь не пуст 
            {
                nextElement = TempPath.Dequeue(); //берет из него следующую точку
            }
            else //иначе берет следующую точку из основного пути
            {
                StepsCountAfterCalculatePath+=16; //увеличивает счетчик количества шагов после расчета пути
                if (InvertHamiltonPath) //если путь инвертирован берет предыдущую точку
                {
                    nextElement = (currentIndnex - 1 < 0) ? HamiltonPath[HamiltonPath.Count - 1] : HamiltonPath[currentIndnex - 1];
                }
                else //иначе берет следующую точку
                {
                    nextElement = (currentIndnex + 1 == HamiltonPath.Count) ? HamiltonPath[0] : HamiltonPath[currentIndnex + 1];
                }
            }

            //устанавливает направление движения в соответствии с координатами текущей и следующей точки пути
            if (currentElement.X == nextElement.X && currentElement.Y < nextElement.Y)  
            {
                Dir = Direction.Right;
                return;
            }

            if (currentElement.X == nextElement.X && currentElement.Y > nextElement.Y)
            {
                Dir = Direction.Left;
                return;
            }

            if (currentElement.X < nextElement.X && currentElement.Y == nextElement.Y)
            {
                Dir = Direction.Down;
                return;
            }

            if (currentElement.X > nextElement.X && currentElement.Y == nextElement.Y)
            {
                Dir = Direction.Up;
                return;
            }
        }
        private void CalculatePath() //метод для расчета пути
        {
            StepsCountAfterCalculatePath = 0; //сброс счетчика шагов после расчета пути
            int finalIndexPoint = HamiltonPath.FindIndex(p => p.X == Food.X && p.Y == Food.Y); //находит индекс координаты еды на пути гамильтона
            List<Point> tempPath = new List<Point>();
            List<Point> stepPiton = new(Snake.Select(p => new Point(p.X, p.Y)).ToList());
            //Select Проецирует каждый элемент последовательности в List<Point>
            //метод ToList принудительно выполняет немедленную оценку запросов и возвращает объект List<T>
            //в points хранится тело змейки

            int index = 0; //индекс для хранения текущей позиции в списке stepPiton

            var result = StepTempPath(ref index, GetInvert(stepPiton, Food), Snake.Last(), finalIndexPoint, stepPiton, tempPath); //вызывает метод StepTempPath, чтобы получить путь до конечной точки
            /*Поскольку значение index может изменяться внутри метода и эти изменения должны быть видимыми в вызывающем коде,
            используется модификатор ref для передачи аргумента по ссылке, а не по значению.*/

            if (result.PathIsFound) //если путь найден, сохраняет его в TempPath и InvertHamiltonPath
            {
                TempPath = new Queue<Point>(tempPath);
                InvertHamiltonPath = result.InvertHamiltonPath;
            }
        }
        private bool GetInvert(List<Point> stepPiton, Point finalPoint) //метод проверки, что нужно идти в обратном направлении
        {
            int pitonDirection = stepPiton.Last().Y - stepPiton[stepPiton.Count - 2].Y;
            int foodDirection = stepPiton.Last().Y - finalPoint.Y;
            return (pitonDirection < 0 && foodDirection < 0) || (pitonDirection > 0 && foodDirection > 0);
        }

        class ResultAnlaizePath //Класс ResultAnlaizePath определяет результат анализа пути
        {
            public bool PathIsFound { get; set; } //поле отвечает за наличие пути
            public bool InvertHamiltonPath { get; set; } //поле отвечает за то, нужно ли инвертировать гамильтонов путь
            public ResultAnlaizePath(bool pathIsFound, bool invertHamiltonPath = false) //Конструктор класса принимает два параметра
            {
                PathIsFound = pathIsFound; // обязательный, булевое значение, указывающее на наличие пути
                InvertHamiltonPath = invertHamiltonPath; //необязательный, булевое значение, указывающее нужно ли инвертировать гамильтонов путь (по умолчанию false)
            }
        }

        //метод, вычисляющий путь змейки до еды
        private ResultAnlaizePath StepTempPath(ref int index, bool invert, Point current, int finalIndexPoint, List<Point> stepPiton, List<Point> tempPath)
        {
            index++;
            // Проверка, достигнута ли максимальная длина пути
            if (HamiltonPath.Count < index)
            {
                return new ResultAnlaizePath(false);
            }
            var finalPoint = HamiltonPath[finalIndexPoint];

            // Проверка, дошли ли до еды
            if (current.X == finalPoint.X && current.Y == finalPoint.Y)
            {
                // Поиск альтернативных путей, чтобы змейка не пересекала свой хвост
                foreach (var d in new[] { false, true })
                {
                    var tempPiton = stepPiton.TakeLast(Snake.Count).ToList();
                    bool isFound = true;
                    bool invertHamiltonPath = d;
                    // Проверка каждого элемента змейки на пересечение с путем HamiltonPath
                    for (int j = 1; j < Snake.Count; j++)
                    {
                        Point hamiltonPoint;
                        if (invertHamiltonPath)
                        {
                            // Если invertHamiltonPath равно true, берется точка из HamiltonPath в обратном порядке
                            // Если finalInedexPoint -j < 0, то нужно не учитывать finalIndexPoint, чтобы не выйти за пределы списка
                            hamiltonPoint = (finalIndexPoint - j >= 0) ? HamiltonPath[finalIndexPoint - j] : HamiltonPath[HamiltonPath.Count - j];
                        }
                        else
                        {
                            // Если invertHamiltonPath равно false, берется точка из HamiltonPath в прямом порядке
                            hamiltonPoint = (finalIndexPoint + j < HamiltonPath.Count) ? HamiltonPath[finalIndexPoint + j] : HamiltonPath[finalIndexPoint + j - HamiltonPath.Count];
                        }
                        // Проверяем, содержится ли hamiltonPoint в Snake элементах списка tempPiton
                        if (tempPiton.TakeLast(Snake.Count).Contains(hamiltonPoint))
                        {
                            // Если точка уже содержится в tempPiton, устанавливаем флаг isFound в false и прерываем цикл
                            isFound = false;
                            break;
                        }
                        tempPiton.Add(hamiltonPoint);
                    }
                    if (isFound)
                    {
                        // Если не было пересечений c телом, возвращаем результат с флагом true и значением invertHamiltonPath
                        return new ResultAnlaizePath(true, invertHamiltonPath);
                    }
                }
                return new ResultAnlaizePath(false);
            }

            // Проверка, достигнуто ли ограничение на количество элементов во временном пути
            if ((Rows + Cols * 2) <= tempPath.Count)
            {
                return new ResultAnlaizePath(false);
            }

            Point newElement = new(0, 0);

            // Вычисление следующую точку пути
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

            // Проверка, не содержит ли новый элемент змейку
            if (!stepPiton.TakeLast(Snake.Count).Contains(newElement))
            {
                tempPath.Add(newElement);
                stepPiton.Add(newElement);
                var retult = StepTempPath(ref index, !invert, newElement, finalIndexPoint, stepPiton, tempPath);

                // Если путь найден, вернуть результат
                if (retult.PathIsFound)
                {
                    return retult;
                }
            }
            return new ResultAnlaizePath(false);
        }
    }
}