using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly Image[,] gridImages;
        private readonly GameLogic gameLogic;
        public MainWindow()
        {
            
            InitializeComponent();
            CreateGrid(playerGrid);
            CreateGrid(enemyGrid);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GameLoop();
        }

        private void GameLoop()
        {
            Draw();
            while (true)
            {
                gameLogic.Move();
                Draw();
                Console.WriteLine("moving");
            }
        }

        private void Draw()
        {
            DrawGrid();
        }
        private void DrawGrid()
        {
            for(int r = 0; r<rows; r++)
            {
                for(int c = 0; c <cols; c++)
                {
                    GridValue gridVal = gameLogic.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                }
            }
        }

    }
}
