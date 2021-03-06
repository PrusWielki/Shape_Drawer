using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace Shape_Drawer
{
    public struct Point
    {
        public int X;
        public int Y;
        public Point(int a, int b)
        {
            X = a; Y = b;
        }
    }
    internal abstract class ShapeDrawer
    {
        public List<Point> points = new List<Point>();
        public bool gotPoints = false;
        protected int rColor = 0;
        protected int gColor = 0;
        protected int bColor = 0;
        public Point a;
        public Point b;
        protected ShapeDrawer(Point a, Point b, int R, int G, int B)
        {
            rColor = R;
            gColor = G;
            bColor = B;
            this.a = a;
            this.b = b;
        }
        abstract public Image Draw(Image imgSource);
        abstract public void GetPoints();
        abstract public void Thicc(int howThicc);
        abstract public void Antialias(int x1, int y1, int x2, int y2, float thickness, Image imgSource);
        abstract public void TransformPoints(Point a);
        abstract public void ChangeColor(int R, int G, int B);
        abstract public void ChangeRadius(int Radius);
    }
    internal class ShapeDrawerConcrete : ShapeDrawer
    {
        public ShapeDrawerConcrete(Point a, Point b, int R, int G, int B) : base(a, b, R, G, B)
        {
        }
        public override void Antialias(int x1, int y1, int x2, int y2, float thickness, Image imgSource)
        {
        }
        public override void ChangeColor(int R, int G, int B)
        {
            rColor = R;
            gColor = G;
            bColor = B;
        }
        public override void ChangeRadius(int Radius)
        {
        }
        public override Image Draw(Image imgSource)
        {
            int width = imgSource.Width;
            int height = imgSource.Height;
            Bitmap newBitmap = (Bitmap)imgSource.Clone();
            BitmapData srcData = newBitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            //hold the amount of bytes needed to represent the image's pixels
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            //copy image data to the buffer
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            newBitmap.UnlockBits(srcData);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (points.Contains(new Point(x, y)))
                    {
                        buffer[y * srcData.Stride + x * 4] = (byte)bColor;
                        buffer[y * srcData.Stride + x * 4 + 1] = (byte)gColor;
                        buffer[y * srcData.Stride + x * 4 + 2] = (byte)rColor;
                    }
                }
            }
            //create a new bitmap with changed pixel rgb values
            Bitmap resImg = new Bitmap(width, height);
            BitmapData resData = resImg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, resData.Scan0, bytes);
            resImg.UnlockBits(resData);
            return resImg;
        }
        public override void GetPoints()
        {
        }
        public override void Thicc(int howThicc)
        {
        }
        public override void TransformPoints(Point p)
        {
            points.Clear();
            gotPoints = false;
            a = new Point(a.X + p.X, a.Y + p.Y);
            b = new Point(b.X + p.X, b.Y + p.Y);
        }
    }
    internal class SymmetricLine : ShapeDrawerConcrete
    {
        public SymmetricLine(Point a, Point b, int R, int G, int B) : base(a, b, R, G, B) { }
        public override void Thicc(int howThicc)
        {
            if (howThicc == 1)
            {
                gotPoints = false;
                return;
            }
            points.Clear();
            GetPoints();
            gotPoints = true;
            int listSize = points.Count;
            for (int i = 0; i < listSize; i++)
            {
                for (int j = 1; j < (howThicc - 1) / 2 + 1; j++)
                {
                    if (Math.Abs((int)a.X - b.X) > Math.Abs((int)a.Y - b.Y))
                    {
                        if (!points.Contains(new Point(points[i].X + j, points[i].Y)))
                        {
                            points.Add(new Point(points[i].X + j, points[i].Y));
                            points.Add(new Point(points[i].X - j, points[i].Y));
                        }
                    }
                    else
                    {
                        if (!points.Contains(new Point(points[i].X, points[i].Y + j)))
                        {
                            points.Add(new Point(points[i].X, points[i].Y + j));
                            points.Add(new Point(points[i].X, points[i].Y - j));
                        }
                    }
                }
            }
        }
        private (int, Image) IntensifyPixel(int x, int y, float thickness, float distance, Image imgSource)
        {
            int width = imgSource.Width;
            int height = imgSource.Height;
            Bitmap newBitmap = (Bitmap)imgSource.Clone();
            BitmapData srcData = newBitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            //hold the amount of bytes needed to represent the image's pixels
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            //copy image data to the buffer
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            newBitmap.UnlockBits(srcData);
            // float r = 0.5f;
            float cov = 0;// coverage(thickness, distance, r);
                          // if (cov > 0)
                          // {
                          // putPixel(x, y, lerp(BKG_COLOR, LINE_COLOR, cov));
            //}
            //create a new bitmap with changed pixel rgb values
            Bitmap resImg = new Bitmap(width, height);
            BitmapData resData = resImg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, resData.Scan0, bytes);
            resImg.UnlockBits(resData);
            return ((int)cov, resImg);
        }
        public override void Antialias(int x1, int y1, int x2, int y2, float thickness, Image imgSource)
        {
            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int dE = 2 * dy, dNE = 2 * (dy - dx);
            int d = 2 * dy - dx;
            int result = 0;
            int i = 1;
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float)(1 / (2 * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2 * dx * invDenom; //precomputed constant
            int x = x1, y = y1;
            IntensifyPixel(x, y, thickness, 0, imgSource);
            while (true)
            {
                i++;
                (result, imgSource) = IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom, imgSource);
                if (result <= 0)
                    break;
            }
            i = 1;
            while (true)
            {
                i++;
                (result, imgSource) = IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom, imgSource);
                if (result <= 0)
                    break;
            }
            while (x < x2)
            {
                ++x;
                if (d < 0) // move to E
                {
                    two_v_dx = d + dx;
                    d += dE;
                }
                else // move to NE
                {
                    two_v_dx = d - dx;
                    d += dNE;
                    ++y;
                }
                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, imgSource);
                i = 1;
                while (true)
                {
                    i++;
                    (result, imgSource) = IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom, imgSource);
                    if (result <= 0)
                        break;
                }
                i = 1;
                while (true)
                {
                    i++;
                    (result, imgSource) = IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom, imgSource);
                    if (result <= 0)
                        break;
                }
            }
        }
        public override void GetPoints()
        {
            points.Clear();
            gotPoints = true;
            if (a.X > b.X)
            {
                Point temp = a;
                a = b;
                b = temp;
            }
            int dx = (int)(b.X - a.X);
            int dy = (int)(b.Y - a.Y);
            int d = dy - (dx / 2);
            int x = (int)a.X, y = (int)a.Y;
            points.Add(new Point(x, y));
            while (x < (int)b.X)
            {
                if (a.X < b.X)
                {
                    x++;
                    if (a.Y < b.Y)
                    {
                        if (d < 0)
                            d = d + dy;
                        else
                        {
                            d += (dy - dx);
                            y++;
                        }
                    }
                    else if (a.Y > b.Y)
                    {
                        if (d > 0)
                            d = d + dy;
                        else
                        {
                            d += (dy - dx);
                            y--;
                        }
                    }
                }
                else
                {
                    x--;
                    if (a.Y < b.Y)
                    {
                        if (d < 0)
                            d = d + dy;
                        else
                        {
                            d += (dy + dx);
                            y++;
                        }
                    }
                    else if (a.Y > b.Y)
                    {
                        if (d > 0)
                            d = d + dy;
                        else
                        {
                            d += (dy + dx);
                            y--;
                        }
                    }
                }
                points.Add(new Point(x, y));
            }
        }
    }
    internal class MidpointCircle : ShapeDrawerConcrete
    {
        public MidpointCircle(Point a, Point b, int R, int G, int B) : base(a, b, R, G, B) { }
        public override void ChangeRadius(int Radius)
        {
            b = new Point(a.X, a.Y + Radius);
            points.Clear();
            gotPoints = false;
        }
        public override void GetPoints()
        {
            points.Clear();
            gotPoints = true;
            int Radius = (int)Math.Sqrt((Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)));
            int x = Radius;
            int y = 0;
            int P = 1 - Radius;
            points.Add(new Point((int)(x + a.X), (int)(a.Y + y)));
            points.Add(new Point((int)(-x + a.X), (int)(a.Y + y)));
            points.Add(new Point((int)(x + a.X), (int)(a.Y - y)));
            points.Add(new Point((int)(y + a.X), (int)(a.Y + x)));
            points.Add(new Point((int)(-y + a.X), (int)(a.Y + x)));
            while (x > y)
            {
                y++;
                if (P <= 0)
                    P = P + 2 * y + 1;
                else
                {
                    x--;
                    P = P + 2 * y - 2 * x + 1;
                }
                if (x < y)
                    break;
                points.Add(new Point((int)(x + a.X), (int)(a.Y + y)));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y + y)));
                points.Add(new Point((int)(x + a.X), (int)(a.Y - y)));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y - y)));
            }
            x = 0;
            y = Radius;
            points.Add(new Point((int)(x + a.X), (int)(a.Y - y)));
            int dE = 3;
            int dSE = 5 - 2 * Radius;
            int d = 1 - Radius;
            while (x < y)
            {
                if (d < 0)
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    y--;
                }
                x++;
                points.Add(new Point((int)(x + a.X), (int)(a.Y + y)));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y + y)));
                points.Add(new Point((int)(x + a.X), (int)(a.Y - y)));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y - y)));
            }
        }
    }
    internal class Polygon : ShapeDrawerConcrete
    {
        private List<Point> polygonPoints;
        private List<SymmetricLine> polygonLines;
        public Polygon(Point a, Point b, int R, int G, int B, List<Point> polygonPoints) : base(a, b, R, G, B)
        {
            this.polygonPoints = polygonPoints;
            polygonLines = new List<SymmetricLine>();
        }
        public override void GetPoints()
        {
            points.Clear();
            gotPoints = true;
            for (int i = 0; i < polygonPoints.Count - 2; i++)
            {
                polygonLines.Add(new SymmetricLine(polygonPoints[i], polygonPoints[i + 1], rColor, gColor, bColor));
            }
            polygonLines.Add(new SymmetricLine(polygonPoints[polygonPoints.Count - 2], polygonPoints[0], rColor, gColor, bColor));
            foreach (var line in polygonLines)
            {
                line.GetPoints();
            }
        }
        public override Image Draw(Image imgSource)
        {
            foreach (var line in polygonLines)
            {
                imgSource = line.Draw(imgSource);
            }
            return imgSource;
            // return base.Draw(imgSource);
        }
    }
}
