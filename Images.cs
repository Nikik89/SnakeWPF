using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SnakeWPF
{
    public static class Images //класс, который загружает изображения
    {
        //задаем константы для каждого изображения
        public readonly static ImageSource Empty = LoadImg("Empty.png");
        public readonly static ImageSource Body = LoadImg("Body.png");
        public readonly static ImageSource Head = LoadImg("Head.png");
        public readonly static ImageSource AngryHead = LoadImg("AngryHead.png");
        public readonly static ImageSource Food = LoadImg("Apple.png");

        public readonly static ImageSource Outside = null; //константа для изображения, находящегося на пределами игровой сетки

        private static ImageSource LoadImg(string filename) //метод для загрузки изображения из указанного файла в проекте
        {
            return new BitmapImage(new Uri($"Assets/{filename}", UriKind.Relative));
        }
    }
}
