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
        public readonly static ImageSource Empty = LoadImg("Empty.png");
        public readonly static ImageSource Body = LoadImg("Body.png");
        public readonly static ImageSource Head = LoadImg("Head.png");
        public readonly static ImageSource AngryHead = LoadImg("AngryHead.png");
        public readonly static ImageSource Food = LoadImg("Apple.png");
        public readonly static ImageSource DeadBody = LoadImg("DeadBody.png");
        public readonly static ImageSource DeadHead = LoadImg("DeadHead.png");

        private static ImageSource LoadImg(string filename)
        {
            return new BitmapImage(new Uri($"Assets/{filename}", UriKind.Relative));
        }
    }
}
