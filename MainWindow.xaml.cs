using System;
using System.Collections.Generic;
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

        private readonly int rows = 10, cols = 10;
        private readonly Image[,] PgridImages;
        private readonly Image[,] EgridImages;
        private GameLogic PgameLogic;
        private EnemyGameLogic EgameLogic;
        public MainWindow()
        {
            
            InitializeComponent();
            PgridImages = CreateGrid(playerGrid);
            EgridImages = CreateGrid(enemyGrid);
            PgameLogic = new GameLogic(rows, cols);
            EgameLogic = new EnemyGameLogic(rows, cols);
            
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
                        Source = Images.Empty
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

        }

        private void DrawGrid(Image[,] gridImg)
        {
            if(gridImg == PgridImages)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        GridValue gridVal = PgameLogic.Grid[r, c];
                        gridImg[r, c].Source = gridValToImage[gridVal];
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
                        switch (gridVal)
                        {
                            case GridValue.Snake:
                                EgameLogic.map[r, c] = 1;
                                break;
                            case GridValue.Empty:
                                EgameLogic.map[r, c] = 0;
                                break;
                            case GridValue.Food:
                                EgameLogic.map[r, c] = 2;
                                break;
                        }
                        gridImg[r, c].Source = gridValToImage[gridVal];
                    }
                }
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
            while (!PgameLogic.GameOver || !EgameLogic.GameOver)
            {
                await Task.Delay(25);
                //PgameLogic.Move();
                Draw(PgridImages);
                EgameLogic.Move();
                Draw(EgridImages);
                Text.Text = EgameLogic.text;

            }

        }
    }
}
