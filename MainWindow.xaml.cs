using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            {GridValue.Empty, Images.Empty },
            {GridValue.Snake, Images.Body },
            {GridValue.Food, Images.Food  },
        };

        private readonly Dictionary<Direction, int> dirToRot = new()
        {
            { Direction.Up, 0},
            { Direction.Right, 90},
            { Direction.Down, 180 },
            { Direction.Left, 270}
        };


        private readonly int rows = 14, cols = 14;
        private readonly Image[,] PgridImages;
        private readonly Image[,] EgridImages;
        private GameLogic PgameLogic;
        private EnemyGameLogic EgameLogic;

        private StreamWriter logfile;

        public MainWindow()
        {
            
            InitializeComponent();
            PgridImages = CreateGrid(playerGrid);
            EgridImages = CreateGrid(enemyGrid);
            PgameLogic = new GameLogic(rows, cols);
            EgameLogic = new EnemyGameLogic(rows, cols);
            logfile = new StreamWriter("log.txt");
        }


        public Image[,] CreateGrid(UniformGrid gridName)
        {
            Image[,] images = new Image[rows, cols];
            gridName.Rows = rows;
            gridName.Columns = cols;
            
            for(int r = 0; r < rows; r++)
            {
                for(int c = 0; c < cols; c++)
                {
                    Image image = new()
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };
                    images[r, c] = image;
                    gridName.Children.Add(image);
                }
            }
            return images;

        }

        private async Task RunGame()
        {
            Draw(PgridImages);
            Draw(EgridImages);
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            PgameLogic = new GameLogic(rows, cols);
            EgameLogic = new EnemyGameLogic(rows, cols);
        }
        private async void Window_StartKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }
            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }
        bool gameRunning = false;
        private async Task ShowGameOver()
        {
            await Task.Delay(500);
            Overlay.Visibility = Visibility.Visible;
        }
        private void Draw(Image[,] gridImg)
        {
            DrawGrid(gridImg);
            DrawSnakeHead(gridImg);
        }
        public long astralstep = 0;  
        private void DrawGrid(Image[,] gridImg)
        {
            astralstep++;
            if (gridImg == PgridImages)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        GridValue gridVal = PgameLogic.Grid[r, c];
                        gridImg[r, c].Source = gridValToImage[gridVal];
                        gridImg[r, c].RenderTransform = Transform.Identity;
                    }
                }
            }
            else
            {
                
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        GridValue gridVal = EgameLogic.Grid[r, c];
                        logfile.Write(gridVal.ToString());
                        gridImg[r, c].Source = gridValToImage[gridVal];
                        gridImg[r, c].RenderTransform = Transform.Identity;
                    }
                    logfile.WriteLine();
                }
                logfile.WriteLine(Convert.ToString(astralstep));
            }
        }

        private void DrawSnakeHead(Image[,] gridImg)
        {
            if (gridImg == PgridImages)
            {
                Position headPos = PgameLogic.HeadPosition();
                Image image = PgridImages[headPos.Row, headPos.Col];
                image.Source = Images.Head;

                int rotation = dirToRot[PgameLogic.Dir];
                image.RenderTransform = new RotateTransform(rotation);
            }
            else
            {
                var headPos = EgameLogic.Snake.Last();
                Image image = EgridImages[headPos.X, headPos.Y];
                image.Source = Images.AngryHead;
                image.RenderTransform = new RotateTransform(EgameLogic.rotation);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (PgameLogic.GameOver)
            {
                return;
            }
            switch (e.Key)
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

        private async Task GameLoop()
        {
            Draw(PgridImages);
            Draw(EgridImages);
            while (StopGame())
            {
                await Task.Delay(150);
                PgameLogic.Move();
                Draw(PgridImages);
                EgameLogic.Move();
                if (!EgameLogic.GameOver)
                {
                    Draw(EgridImages);
                }
                PlayerScore.Text = "SCORE " + Convert.ToString(PgameLogic.FoodCount);
                EnemyScore.Text ="SCORE " + Convert.ToString(EgameLogic.FoodCount);
            }
            EnemyWins.Text = "Wins: " + Convert.ToString(EnemyWinCount);
            PlayerWins.Text = "Wins: " + Convert.ToString(PlayerWinCount);
        }

        public int EnemyWinCount = 0;
        public int PlayerWinCount = 0;

        private bool StopGame()
        {
            
            if (PgameLogic.GameOver && !EgameLogic.GameOver)
            {
                if(EgameLogic.FoodCount > PgameLogic.FoodCount)
                {
                    MessageBox.Show("Компютер победил");
                    EnemyWinCount++;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (EgameLogic.GameOver && !PgameLogic.GameOver)
            {
                if(PgameLogic.FoodCount > EgameLogic.FoodCount)
                {
                    MessageBox.Show("Человек крут");
                    PlayerWinCount++;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (EgameLogic.GameOver && PgameLogic.GameOver)
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }
    }
}
