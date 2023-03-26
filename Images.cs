using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SnakeWPF
{
    public static class Images
    {
        public readonly static ImageSource Empty = new BitmapImage(new Uri("C:/Users/sdasf/Desktop/2курс/2семак/Прогр/тестовыеПроги/MyGames/SnakeGameTest/Assets/Empty.png"));
        public readonly static ImageSource Body = new BitmapImage(new Uri("C:/Users/sdasf/Desktop/2курс/2семак/Прогр/тестовыеПроги/MyGames/SnakeGameTest/Assets/Body.png"));
        public readonly static ImageSource Head = new BitmapImage(new Uri("C:/Users/sdasf/Desktop/2курс/2семак/Прогр/тестовыеПроги/MyGames/SnakeGameTest/Assets/Head.png"));
        public readonly static ImageSource Food = new BitmapImage(new Uri("C:/Users/sdasf/Desktop/2курс/2семак/Прогр/тестовыеПроги/MyGames/SnakeGameTest/Assets/Food.png"));
        public readonly static ImageSource DeadBody = new BitmapImage(new Uri("C:/Users/sdasf/Desktop/2курс/2семак/Прогр/тестовыеПроги/MyGames/SnakeGameTest/Assets/DeadBody.png"));
        public readonly static ImageSource DeadHead = new BitmapImage(new Uri("C:/Users/sdasf/Desktop/2курс/2семак/Прогр/тестовыеПроги/MyGames/SnakeGameTest/Assets/DeadHead.png"));

    }
}
