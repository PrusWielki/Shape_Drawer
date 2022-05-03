using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;

namespace Shape_Drawer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum Mode
    {
        None,
        Line,
        Circle,

    }
    public partial class MainWindow : Window
    {
        System.Windows.Point a;
        System.Windows.Point b;
        bool isASet;
        bool isBSet;
        SymmetricLine symmetric = new SymmetricLine();

        Mode mode=Mode.None;


        System.Drawing.Image imageDrawing;

        public IEnumerable<KeyValuePair<String, Color>> NamedColors
        {
            get;
            private set;
        }

        public MainWindow()
        {
            InitializeComponent();

            this.NamedColors = this.GetColors();

            this.DataContext = this;


        }

        private IEnumerable<KeyValuePair<String, System.Windows.Media.Color>> GetColors()
        {
            return typeof(Colors)
                .GetProperties()
                .Where(prop =>
                    typeof(Color).IsAssignableFrom(prop.PropertyType))
                .Select(prop =>
                    new KeyValuePair<String, Color>(prop.Name, (Color)prop.GetValue(null)));
        }
        private void ConvertToDrawing(System.Windows.Controls.Image img)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.Windows.Media.Imaging.BmpBitmapEncoder bbe = new BmpBitmapEncoder();
            if (null != img.Source)
                bbe.Frames.Add(BitmapFrame.Create(new Uri("../../../transparent.png", UriKind.RelativeOrAbsolute)));

            else
                return;


            bbe.Save(ms);
            imageDrawing = System.Drawing.Image.FromStream(ms);
        }
        private BitmapImage ToWpfImage(System.Drawing.Image img)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage ix = new BitmapImage();
            ix.BeginInit();
            ix.CacheOption = BitmapCacheOption.OnLoad;
            ix.StreamSource = ms;
            ix.EndInit();
            return ix;
        }
        private void backgroundImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(backgroundImage);
            // MessageBox.Show(p.X + ", " + p.Y);
            double pixelWidth = backgroundImage.Source.Width;
            double pixelHeight = backgroundImage.Source.Height;
            double x = pixelWidth * p.X / backgroundImage.ActualWidth;
            double y = pixelHeight * p.Y / backgroundImage.ActualHeight;
            if (mode != Mode.None)
            {
                if (!isASet)
                {
                    a = new System.Windows.Point(x, y);
                    isASet = true;
                }
                else if (!isBSet)
                {
                    b = new System.Windows.Point(x, y);
                    isBSet = true;

                }
                if (isASet && isBSet)
                {

                    if (mode == Mode.Line)
                    {
                        ConvertToDrawing(backgroundImage);
                        imageDrawing = symmetric.Draw(a, b, imageDrawing);
                        backgroundImage.Source = ToWpfImage(imageDrawing);
                        mode = Mode.None;

                    }

                }

            }
            
            //MessageBox.Show(x + ", " + y);
        }

        private void DrawALineButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Line;
        }
    }
}
