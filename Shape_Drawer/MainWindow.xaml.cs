using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        Select,
        Polygon,
        Rectangle,
        EditShape,
        ClipRect,
        FillColor,
    }
    public partial class MainWindow : Window
    {
        Point a;
        Point b;
        byte R = 0;
        byte G = 0;
        byte B = 0;
        int Radius = 0;
        int howThicc = 1;
        System.Windows.Point changePositionA;
        System.Windows.Point changePositionB;
        bool isASet;
        bool isBSet;
        List<ShapeDrawer> shapes = new List<ShapeDrawer>();
        Mode mode = Mode.None;
        System.Drawing.Image? imageDrawing;
        List<Point> polygonPoints = new List<Point>();
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
            if (img == null)
                return;
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
        private bool shapeClicked(Point a)
        {
            foreach (var shape in shapes)
            {
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
            if (backgroundImage == null)
                return;
            foreach (var shape in shapes)
            {
                if (!shape.gotPoints)
                    shape.GetPoints();
                //shape.Thicc(howThicc);
                if (null != imageDrawing)
                    imageDrawing = shape.Draw(imageDrawing);
            }
            if (shapes.Count > 0)
            {
                if (null != imageDrawing)
                    backgroundImage.Source = ToWpfImage(imageDrawing);
            }
            else
            {
                backgroundImage.Source = new BitmapImage(new Uri("../../../transparent.png", UriKind.RelativeOrAbsolute));
            }
        }
        private void ChangePosition()
        {
            Point difference = new Point((int)(b.X - a.X), (int)(b.Y - a.Y), R, G,B);
            foreach (var shape in shapes)
            {
                shape.gotPoints = false;
                if (shape.HitDetection(new Point((int)a.X, (int)a.Y,R,G,B)))
                {
                    shape.TransformPoints(difference);
                }
            }
            DrawShapes();
        }
        private void ClipRect(Point a, Point b)
        {
            int indexOfPolygon = -1;
            int indexOfRectangle = -1;

            for(int i=0;i<shapes.Count;i++)
            {
                if (shapes[i].HitDetection(a))
                {
                    indexOfPolygon = i;
                    break;
                }
            }
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i].HitDetection(b))
                {
                    indexOfRectangle = i;
                    break;
                }
            }
            if(indexOfPolygon!=-1&&indexOfRectangle!=-1)
            shapes[indexOfPolygon].ClipRect((Rectangle)shapes[indexOfRectangle]);



        }
        private void backgroundImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(backgroundImage);
            double pixelWidth = backgroundImage.Source.Width;
            double pixelHeight = backgroundImage.Source.Height;
            double x = pixelWidth * p.X / backgroundImage.ActualWidth;
            double y = pixelHeight * p.Y / backgroundImage.ActualHeight;
            shapeClicked(new Point((int)x, (int)y,R,G,B));
            if (mode != Mode.None)
            {

                

                if (mode == Mode.Polygon)
                {
                    if (polygonPoints.Count > 1)
                    {
                        if (Math.Abs(x - polygonPoints[0].X) < 10 && Math.Abs(y - polygonPoints[0].Y) < 10)
                        {
                            shapes.Add(new Polygon(polygonPoints[0], polygonPoints[1], R, G, B, polygonPoints));
                            DrawShapes();
                            polygonPoints.Clear();
                            isASet = false;
                            isBSet = false;
                            return;
                        }
                    }
                    polygonPoints.Add(new Point((int)x, (int)y,R,G,B));
                }
                
                if (mode == Mode.Select)
                {
                    foreach (var shape in shapes)
                    {
                        foreach(var point in shape.points) 
                        {
                            if (point.X == (int)x && point.Y == (int)y)
                            {
                                shape.Thicc(howThicc);
                                shape.ChangeColor(R, G, B);
                                shape.ChangeRadius(Radius);
                                break;
                            }

                        }
                       
                    }
                    DrawShapes();
                    return;
                }
                if (mode == Mode.FillColor)
                {
                    foreach(var shape in shapes)
                    {
                        if (shape.HitDetection(new Point((int)x, (int)y,R,G,B)))
                        {
                            shape.FillPolygon(R, G, B);
                            break;
                        }

                    }
                    DrawShapes();
                }
                if (!isASet)
                {
                    a = new Point((int)x, (int)y,R,G,B);
                    isASet = true;
                }
                else if (!isBSet)
                {
                    b = new Point((int)x, (int)y,R,G,B);
                    isBSet = true;
                }
                if (isASet && isBSet)
                {
                    isASet = false;
                    isBSet = false;

                    if (mode == Mode.ClipRect)
                    {
                        ClipRect(a, b);
                        DrawShapes();


                    }

                    if (mode == Mode.EditShape)
                    {
                        foreach (var shape in shapes)
                        {
                            if (shape.HitDetection(a))
                            {
                                shape.ChangeSize(a,b);
                                DrawShapes();
                            }
                        }
                        return;
                    }
                    if (mode == Mode.Rectangle)
                    {
                        shapes.Add(new Rectangle(a, b, R, G, B, polygonPoints));
                        mode = Mode.None;


                    }
                    if (mode == Mode.Line)
                    {
                        shapes.Add(new SymmetricLine(a, b, R, G, B));
                        mode = Mode.None;
                    }
                    else if (mode == Mode.Circle)
                    {
                        shapes.Add(new MidpointCircle(a, b, R, G, B));
                        mode = Mode.None;
                    }
                    else if (mode == Mode.Position)
                    {
                        ChangePosition();
                    }
                    else if (mode == Mode.Deletion)
                    {
                        foreach (var shape in shapes)
                        {
                            if (shape.HitDetection(a))
                            {
                                shapes.Remove(shape);
                                DrawShapes();
                                return;
                            }
                        }
                    }
                    DrawShapes();
                }
            }
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
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int32.TryParse(ThiccTextBox.Text, out howThicc))
                howThicc = 1;
        }
        private void ThicknessButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Select;
        }
        private void R_TextChanged(object sender, TextChangedEventArgs e)
        {
            int result;
            if (!Int32.TryParse(RTextBox.Text, out result))
                R = 0;
            else
                R = (byte)result;
        }
        private void G_TextChanged(object sender, TextChangedEventArgs e)
        {
            int result;
            if (!Int32.TryParse(GTextBox.Text, out result))
                G = 0;
            else
                G = (byte)result;
        }
        private void B_TextChanged(object sender, TextChangedEventArgs e)
        {
            int result;
            if (!Int32.TryParse(BTextBox.Text, out result))
                B = 0;
            else
                B = (byte)result;
        }
        private void Radius_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int32.TryParse(RadiusTextBox.Text, out Radius))
                Radius = 0;
        }
        private void PolygonButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Polygon;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.Rectangle;
        }

        private void EditShapeButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.EditShape)
            {
                foreach (var shape in shapes)
                {
                    shape.gotPoints = false;
                }
                DrawShapes();
                mode = Mode.None;

            }
            else
            {
                mode = Mode.EditShape;
                foreach (var shape in shapes)
                {
                    shape.ThickenVertices();
                }
                DrawShapes();
            }
        }

        private void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            mode = Mode.ClipRect;
        }

        private void FillColorButton_Click(object sender, RoutedEventArgs e)
        {
            mode=Mode.FillColor;
        }
    }
}
