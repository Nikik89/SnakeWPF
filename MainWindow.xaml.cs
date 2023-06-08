using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SnakeWPF
{
    public partial class MainWindow : Window //объявление класса MainWindow, который наследуется от Window
    {
        private readonly int rows = 26, cols = 26; //объявление констант rows и cols со значениями 26 (размер игровой сетки)
        //объявление двумерных массивов изображений для сеток игрока и компьютера
        private Image[,] PgridImages;
        private Image[,] EgridImages;
        //объекты логики игры игрока и компьютера
        private GameLogic PgameLogic;
        private EnemyGameLogic EgameLogic;

        private readonly Dictionary<Direction, int> dirToRotation = new() //словарь содержащий соответствие направлений и угла поворота изображения
        {
            {Direction.Up, 0},
            {Direction.Right,90 },
            {Direction.Down,180 },
            {Direction.Left,-90 }
        };
        public MainWindow() //конструктор класса MainWindow
        {
            InitializeComponent(); //инициализация компонентов на форме
            //создание сеток для изображений игрока и компьютера
            PgridImages = CreateGrid(playerGrid);
            EgridImages = CreateGrid(enemyGrid);
            
            PgameLogic = new GameLogic(rows, cols, PgridImages); //инициализация игровой логики для игрока
            
            EgameLogic = new EnemyGameLogic(rows, cols, EgridImages); //инициализация игровой логики для компьютера
        }

        //метод для создания сетки изображений Image в сетке UniformGrid, в которой все элементы будут одинакового размера
        public Image[,] CreateGrid(UniformGrid gridName) 
        {
            Image[,] images = new Image[rows, cols]; //создание массива изображений с заданными размерами
            gridName.Rows = rows;       //присвоение количества строк
            gridName.Columns = cols;    //и столбцов сетке UniformGrid
            for (int r = 0; r < rows; r++) //проход по каждому элементу сетки для добавления отдельного изображения Image
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new() //создание нового объекта Image и задание начальных свойств изображения 
                    {
                        Source = Images.Empty, //изначально все элементы сетки имеют пустое изображение
                        RenderTransformOrigin = new Point(0.5, 0.5) //точка преобразования координат для поворота изображения
                    };
                    //добавление созданного изображения в массив и на сетку
                    images[r, c] = image;
                    gridName.Children.Add(image);
                }
            }
            return images; //возвращает созданный массив изображений
        }


        private static void ClearGrid(Image[,] gridName) //метод для очистки всех ячеек в сетке изображений
        {
            foreach (var image in gridName) //перебор всех изображений в сетке
            {
                image.Source = Images.Empty; //устанавливает для каждого элемента пустое изображение
            }
        }

        private async Task RunGame() //метод, запускающий игру
        {
            Overlay.Visibility = Visibility.Hidden; //срывает оверлей
            await GameLoop(); //ничанает цикл игры
            await ShowGameOver(); //показывает экран окончания игры
            ClearGrid(PgridImages); //очищает сетки игрока
            ClearGrid(EgridImages); //и компьютера
            PgameLogic = new GameLogic(rows, cols, PgridImages);            //создает новые объекты классов GameLogic и EnemyGameLogic, используя
            EgameLogic = new EnemyGameLogic(rows, cols, EgridImages);       //переданные параметры rows, cols и PgridImages/EgridImages

            //переменные PgameLogic и EgameLogic являются ссылками на заново созданные объекты классов GameLogic и EnemyGameLogic
        }
        private async void Window_StartKeyDown(object sender, KeyEventArgs e) //обработчик события нажатия клавиши для начала игры
        {
            if (Overlay.Visibility == Visibility.Visible) //если оверлей видимый, то отменяет обработкку события
            {
                e.Handled = true;
            }
            if (!gameRunning) //если игра не запущена, то запускает ее и дожидается завершения
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }
        bool gameRunning = false; //устанавливает флаг окончания игры
        private async Task ShowGameOver() //асинхронный метод показа экрана завершения игры
        {
            await Task.Delay(500); //задержка в 0.5 секунды
            Overlay.Visibility = Visibility.Visible; //установка видимости оверлея на экране
        }
       
        private void DrawSnakeHead(Image[,] gridImg) //метод для отображения головы змейки
        {
            if (gridImg == PgridImages)
            {
                var headPos = PgameLogic.HeadPosition(); //получение координат головы змейки игрока
                gridImg[headPos.Row, headPos.Col].Source = Images.Head; //установка изображения головы змейки на сетке
                int rotation = dirToRotation[PgameLogic.Dir]; //получение угла поворота головы, отностиельно текущего направления движения 
                gridImg[headPos.Row, headPos.Col].RenderTransform = new RotateTransform(rotation); //установка поворота головы на игровом поле
            }
            else if (!EgameLogic.GameOver)
            {
                var headPos = EgameLogic.Snake.Last(); //получение координат головы змейки компьютера
                Image image = EgridImages[headPos.X, headPos.Y]; //установка изображения
                image.Source = Images.AngryHead;                 // головы змейки на сетке
                image.RenderTransform = new RotateTransform(EgameLogic.Rotation); //получение угла поворота головы, относительно текущего направления движения
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e) //обработчик события нажатия клавиши
        {
            if (PgameLogic.GameOver) //если игра закончилась, то прекращаем обработку
            {
                return;
            }
            switch (e.Key) //обрабатывает нажатие на стрелки клавиатуры
            {
                case Key.Left:
                    PgameLogic.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    PgameLogic.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    PgameLogic.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    PgameLogic.ChangeDirection(Direction.Down);
                    break;
            }
        }

        private enum Speed //перечисление, задающее скорость движения змейки в игре 
        {
            LateGame = 25,
            MidGame = 60,
            NormGame = 90,
            StartGame = 100
        }

        private int ChangeSpeed() //метод для изменения скорости движения змейки
        {
            if (PgameLogic.GameOver) //если игрок проиграл, то устанавливает максимальную скорость
            {
                return (int)Speed.LateGame;
            }
            if (PgameLogic.FoodCount > (rows * cols - (rows * cols) / 2))
            {
                return (int)Speed.LateGame;
            }
            else if (PgameLogic.FoodCount > (rows * cols - (rows * cols) / 1.5))
            {
                return (int)Speed.MidGame;
            }
            else if (PgameLogic.FoodCount > (rows * cols - (rows * cols) / 1.3))
            {
                return (int)Speed.NormGame;
            }
            else //иначе устанавливает начальную скорость
            {
                return (int)Speed.StartGame;
            }
        }

        private async Task GameLoop() //асинхронный метод, зацикливающий игру
        {
            while (StopGame()) //выполняется, StopGame не будет равен false
            {
                await Task.Delay(ChangeSpeed()); //задержка на период скорости передвижения змейки
                PgameLogic.Move(); //перемещение змейки игрока
                DrawSnakeHead(PgridImages); //рисует голову змейки игрока на сетке
                EgameLogic.Move(); //перемещение змейки компьютера
                if (!EgameLogic.GameOver) //если игра еще не закончилась
                {
                    DrawSnakeHead(EgridImages); //рисует голову змейки компьютера
                }
                PlayerScore.Text = "SCORE: " + Convert.ToString(PgameLogic.FoodCount);      //обновляет значение счета игрока
                EnemyScore.Text = "SCORE: " + Convert.ToString(EgameLogic.FoodCount);       //и компьютера на экране
            }
            EnemyWins.Text = "Wins: " + Convert.ToString(EnemyWinCount);                    //обновляет значение побед игрока
            PlayerWins.Text = "Wins: " + Convert.ToString(PlayerWinCount);                  //и компьютера на экране
        }

        public int EnemyWinCount = 0;       //счетчики побед игрока
        public int PlayerWinCount = 0;      //и компьютера

        private bool StopGame() //метод, проверяющий закончилась ли игра
        {
            //если игра окончилась и компьютер победил
            if ((PgameLogic.GameOver && !EgameLogic.GameOver) && (EgameLogic.FoodCount > PgameLogic.FoodCount))
            {
                MessageBox.Show("Компьютер победил"); //вывод сообщения "Компьютер победил"
                EnemyWinCount++; //увеличение счетчика побед у копьютера
                return false; //возвращает значение false
            }
            //если игра окончилась и игрок победил
            else if (EgameLogic.GameOver && !PgameLogic.GameOver && PgameLogic.FoodCount > EgameLogic.FoodCount)
            {
                MessageBox.Show("Читер?"); //вывод сообщения "Человек крут"
                PlayerWinCount++; //увеличение счетчика побед у копьютера
                return false; //возврат значения false
            }
            //если оба игрока умерли
            else if (EgameLogic.GameOver && PgameLogic.GameOver)
            {
                return false; //возвращает false
            }
            //иначе игра продолжается
            else
            {
                return true; //возвращает true
            }

        }
    }
}
