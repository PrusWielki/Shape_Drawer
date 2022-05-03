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
        ThickLine,
        Circle,
        Deletion,
        Position,

    }
    public partial class MainWindow : Window
    {
        System.Windows.Point a;
        System.Windows.Point b;

        System.Windows.Point changePositionA;
        System.Windows.Point changePositionB;


        bool isASet;
        bool isBSet;
        List<ShapeDrawer> shapes=new List<ShapeDrawer>();

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
            ConvertToDrawing(backgroundImage);
           



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

        private bool shapeClicked(System.Windows.Point a)
        {
            foreach(var shape in shapes)
            {
                //MessageBox.Show(a.X+" "+a.Y);
                //for(int i = 0; i < shape.points.Count; i++)
                //{
                //    MessageBox.Show(shape.points[i].X + " " + shape.points[i].Y);
                //}
                if (shape.points.Contains(a))
                {
                    if (mode == Mode.Deletion)
                    {
                        shapes.Remove(shape);
                        DrawShapes();
                        break;


                    }
    
                   
                    return true;
                }
               
                

            }

            return false;

        }

        private void DrawShapes()
        {
            ConvertToDrawing(backgroundImage);
            foreach(var shape in shapes)
            {
                shape.GetPoints();
                imageDrawing=shape.Draw(imageDrawing);

            }
            if (shapes.Count > 0)
                backgroundImage.Source = ToWpfImage(imageDrawing);
            else
            {
                backgroundImage.Source = new BitmapImage(new Uri("../../../transparent.png", UriKind.RelativeOrAbsolute));
            }

        }
        private void ChangePosition()
        {

            
            System.Windows.Point difference = new System.Windows.Point((int)(b.X - a.X), (int)(b.Y - a.Y));
            foreach (var shape in shapes)
            {

                if (shape.points.Contains(new System.Windows.Point((int)a.X,(int)a.Y)))
                {
                    //MessageBox.Show("XD");
                    shape.TransformPoints(difference);
                  //  shape.
                    //for (int i=0;i<shape.points.Count;i++)
                    //{
                    //    shape.points[i] = new System.Windows.Point(shape.points[i].X + difference.X, shape.points[i].Y+ difference.Y);
                    //}
                }

            }
            DrawShapes();

        }

        private void backgroundImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(backgroundImage);
            
            // MessageBox.Show(p.X + ", " + p.Y);
            double pixelWidth = backgroundImage.Source.Width;
            double pixelHeight = backgroundImage.Source.Height;
            double x = pixelWidth * p.X / backgroundImage.ActualWidth;
            double y = pixelHeight * p.Y / backgroundImage.ActualHeight;
            shapeClicked(new System.Windows.Point((int)x, (int)y));
            if (mode != Mode.None)
            {
                if (!isASet)
                {
                    a = new System.Windows.Point(x, y);
                    isASet = true;
                    //MessageBox.Show("XD");
                }
                else if (!isBSet)
                {
                    b = new System.Windows.Point(x, y);
                    isBSet = true;

                }
              
                if (isASet && isBSet)
                {
                    isASet=false;
                    isBSet=false;
                    if (mode == Mode.Line)
                    {
                        shapes.Add(new SymmetricLine(a, b));
                        //imageDrawing = shapes[shapes.Count-1].Draw( imageDrawing);
                        //backgroundImage.Source = ToWpfImage(imageDrawing);
                        mode = Mode.None;

                    }
                    else if (mode == Mode.Circle)
                    {
                        shapes.Add(new MidpointCircle(a, b));
                        //imageDrawing = shapes[shapes.Count - 1].Draw(imageDrawing);
                        //backgroundImage.Source = ToWpfImage(imageDrawing);
                        mode = Mode.None;

                    }
                    else if (mode == Mode.Position)
                    {
                        ChangePosition();
                    }
                    DrawShapes();
                }

            }
            
            //MessageBox.Show(x + ", " + y);
        }

        private void DrawALineButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Line;
        }

        private void DrawAThickLineButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.ThickLine;
        }

        private void DrawACircleButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Circle;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Deletion;
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            shapes.Clear();
            DrawShapes();
        }

        private void PositionButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Position;
        }
    }
}
