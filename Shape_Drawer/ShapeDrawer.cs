using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
namespace Shape_Drawer
{
    public struct Point
    {
        public int X;
        public int Y;
        public byte R;
        public byte G;
        public byte B;
        public Point(int a, int b, byte _R, byte _G, byte _B)
        {
            X = a; Y = b;
            R = _R;
            G=_G; B= _B; 
        }
    }
    public struct EdgeInBucket
    {
        public int ymax;
        public float xmin;
        public int ymin;
        public float dxdy;
        public EdgeInBucket(int a, float b, int c, float _dxdy)
        {
            ymax = a;
            ymin = c;
            xmin = b;
            dxdy = _dxdy;
        }

    }
    internal abstract class ShapeDrawer
    {
        public List<Point> points = new List<Point>();
        public bool gotPoints = false;
        protected byte rColor = 0;
        protected byte gColor = 0;
        protected byte bColor = 0;
        public Point a;
        public Point b;
        protected ShapeDrawer(Point a, Point b, byte R, byte G, byte B)
        {
            rColor = R;
            gColor = G;
            bColor = B;
            this.a = new Point(a.X, a.Y, (byte)R, (byte)G, (byte)B);
            this.b = new Point(b.X, b.Y, (byte)R, (byte)G, (byte)B);
        }
        abstract public Image Draw(Image imgSource);
        abstract public void GetPoints();
        abstract public void Thicc(int howThicc);
        abstract public void Antialias(int x1, int y1, int x2, int y2, float thickness, Image imgSource);
        abstract public void TransformPoints(Point a);
        abstract public void ChangeColor(byte R, byte G, byte B);
        abstract public void ChangeRadius(int Radius);
        abstract public bool HitDetection(Point a);
        abstract public void ThickenVertices();
        abstract public void ChangeSize(Point a, Point t);
        abstract public void ClipRect(Rectangle rect);

        abstract public void FillPolygon(byte R, byte G, byte B);
       abstract public Image FillPolygon(Image imgSource);
    }
    internal class ShapeDrawerConcrete : ShapeDrawer
    {
        public ShapeDrawerConcrete(Point a, Point b, byte R, byte G, byte B) : base(a, b, R, G, B)
        {
        }
        public override void Antialias(int x1, int y1, int x2, int y2, float thickness, Image imgSource)
        {
        }
        public override void ChangeColor(byte R, byte G, byte B)
        {
            rColor = R;
            gColor = G;
            bColor = B;
        }
        public override void ChangeRadius(int Radius)
        {
        }

        public override void ChangeSize(Point a, Point t)
        {

        }

        public override void ClipRect(Rectangle rect)
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

            foreach (var point in points)
            {
                buffer[point.Y * srcData.Stride + point.X * 4] = (byte)bColor;
                buffer[point.Y * srcData.Stride + point.X * 4 + 1] = (byte)gColor;
                buffer[point.Y * srcData.Stride + point.X * 4 + 2] = (byte)rColor;

            }

            //create a new bitmap with changed pixel rgb values
            Bitmap resImg = new Bitmap(width, height);
            BitmapData resData = resImg.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, resData.Scan0, bytes);
            resImg.UnlockBits(resData);
            return resImg;
        }

        public override void FillPolygon(byte R, byte G, byte B)
        {
        }
        public override Image FillPolygon(Image image)
        {
            return image;
        }
        public override void GetPoints()
        {
        }

        public override bool HitDetection(Point a)
        {
            foreach(var point in points)
            {
                if (point.X == a.X && point.Y == a.Y)
                    return true;

            }
            return false;
           // return points.Contains(new Point(a.X,a.Y,rColor,gColor,bColor));
        }

        public override void Thicc(int howThicc)
        {
        }

        public override void ThickenVertices()
        {
            for (int i = 0; i < 2; i++)
            {
                points.Add(new Point(a.X + i, a.Y + i,rColor,gColor,bColor));
                points.Add(new Point(a.X - i, a.Y - i, rColor, gColor, bColor));

                points.Add(new Point(a.X, a.Y + i, rColor, gColor, bColor));
                points.Add(new Point(a.X, a.Y - i, rColor, gColor, bColor));

                points.Add(new Point(a.X + i, a.Y, rColor, gColor, bColor));
                points.Add(new Point(a.X - i, a.Y, rColor, gColor, bColor));



                points.Add(new Point(b.X + i, b.Y + i, rColor, gColor, bColor));
                points.Add(new Point(b.X - i, b.Y - i, rColor, gColor, bColor));



                points.Add(new Point(b.X, b.Y + i, rColor, gColor, bColor));
                points.Add(new Point(b.X, b.Y - i, rColor, gColor, bColor));


                points.Add(new Point(b.X + i, b.Y, rColor, gColor, bColor));
                points.Add(new Point(b.X - i, b.Y, rColor, gColor, bColor));

            }
        }

        public override void TransformPoints(Point p)
        {
            points.Clear();
            gotPoints = false;
            a = new Point(a.X + p.X, a.Y + p.Y,rColor,gColor,bColor);
            b = new Point(b.X + p.X, b.Y + p.Y, rColor, gColor, bColor);
        }

    }
    internal class SymmetricLine : ShapeDrawerConcrete
    {
        public SymmetricLine(Point a, Point b, byte R, byte G, byte B) : base(a, b, R, G, B) { }
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
                        if (!points.Contains(new Point(points[i].X + j, points[i].Y,rColor,gColor,bColor)))
                        {
                            points.Add(new Point(points[i].X + j, points[i].Y, rColor, gColor, bColor));
                            points.Add(new Point(points[i].X - j, points[i].Y, rColor, gColor, bColor));
                        }
                    }
                    else
                    {
                        if (!points.Contains(new Point(points[i].X, points[i].Y + j, rColor, gColor, bColor)))
                        {
                            points.Add(new Point(points[i].X, points[i].Y + j, rColor, gColor, bColor));
                            points.Add(new Point(points[i].X, points[i].Y - j, rColor, gColor, bColor));
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
            BitmapData resData = resImg.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
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
            //bool swapped = false;
            if (a.X > b.X)
            {
                (b, a) = (a, b);
                //swapped = true;
            }
            int dx = Math.Abs((int)(b.X - a.X));
            int dy = Math.Abs((int)(b.Y - a.Y));
            int d = dy - (dx / 2);
            int x = (int)a.X, y = (int)a.Y;
            points.Add(new Point(x, y, rColor, gColor, bColor));

            bool vertical = false;
            if (Math.Abs(b.Y - a.Y) > Math.Abs(b.X - a.X))
                vertical = true;
            if (vertical)
            {
                if (a.Y < b.Y)
                {
                    while (y < (int)b.Y && y >= (int)a.Y)
                    {
                        y++;
                        if (a.X < b.X)
                        {
                            if (d < 0)
                                d = d + dx;
                            else if (dx != 0)
                            {
                                d += (-dy + dx);
                                x++;
                            }
                        }
                        points.Add(new Point(x, y,rColor,gColor,bColor));
                    }

                }
                else
                {
                    while (y > (int)b.Y && y <= (int)a.Y)
                    {
                        y--;
                        if (a.X < b.X)
                        {
                            if (d < 0)
                                d = d + dx;
                            else if (dx != 0)
                            {
                                d += (-dy + dx);
                                x++;
                            }
                        }
                        points.Add(new Point(x, y,rColor,gColor,bColor));
                    }
                }
            }


            while (x < (int)b.X && x >= (int)a.X)
            {


                if (a.X < b.X)
                {
                    x++;
                    if (a.Y < b.Y)
                    {
                        if (d < 0)
                            d = d + dy;
                        else if (dy != 0)
                        {
                            d += (dy - dx);
                            y++;
                        }
                    }
                    else if (a.Y > b.Y)
                    {
                        if (d < 0)
                            d = d + dy;
                        else if (dy != 0)
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
                        else if (dy != 0)
                        {
                            d += (dy + dx);
                            y++;
                        }
                    }
                    else if (a.Y > b.Y)
                    {
                        if (d > 0)
                            d = d + dy;
                        else if (dy != 0)
                        {
                            d += (dy + dx);
                            y--;
                        }
                    }
                }

                points.Add(new Point(x, y,rColor,gColor,bColor));
            }
            
        }
    }
    internal class MidpointCircle : ShapeDrawerConcrete
    {
        public MidpointCircle(Point a, Point b, byte R, byte G, byte B) : base(a, b, R, G, B) { }
        public override void ChangeRadius(int Radius)
        {
            b = new Point(a.X, a.Y + Radius, rColor, gColor, bColor);
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
            points.Add(new Point((int)(x + a.X), (int)(a.Y + y),rColor,gColor,bColor));
            points.Add(new Point((int)(-x + a.X), (int)(a.Y + y), rColor, gColor, bColor));
            points.Add(new Point((int)(x + a.X), (int)(a.Y - y), rColor, gColor, bColor));
            points.Add(new Point((int)(y + a.X), (int)(a.Y + x), rColor, gColor, bColor));
            points.Add(new Point((int)(-y + a.X), (int)(a.Y + x), rColor, gColor, bColor));
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
                points.Add(new Point((int)(x + a.X), (int)(a.Y + y), rColor, gColor, bColor));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y + y), rColor, gColor, bColor));
                points.Add(new Point((int)(x + a.X), (int)(a.Y - y), rColor, gColor, bColor));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y - y), rColor, gColor, bColor));
            }
            x = 0;
            y = Radius;
            points.Add(new Point((int)(x + a.X), (int)(a.Y - y), rColor, gColor, bColor));
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
                points.Add(new Point((int)(x + a.X), (int)(a.Y + y), rColor, gColor, bColor));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y + y), rColor, gColor, bColor));
                points.Add(new Point((int)(x + a.X), (int)(a.Y - y), rColor, gColor, bColor));
                points.Add(new Point((int)(-x + a.X), (int)(a.Y - y), rColor, gColor, bColor));
            }
        }
    }
    internal class Polygon : ShapeDrawerConcrete
    {
        protected List<Point> polygonPoints;
        protected List<SymmetricLine> polygonLines;

        protected List<List<EdgeInBucket>> edgeTable = new List<List<EdgeInBucket>>();
        protected List<EdgeInBucket> activeEdgeTable = new List<EdgeInBucket>();

        bool filled = false;
        byte RFill;
        byte GFill;
        byte BFill;
        public Polygon(Point a, Point b, byte R, byte G, byte B, List<Point> polygonPoints) : base(a, b, R, G, B)
        {
            this.polygonPoints = new List<Point>(polygonPoints);
            polygonLines = new List<SymmetricLine>();
        }
        public override void GetPoints()
        {
            points.Clear();
            gotPoints = true;
            polygonLines.Clear();
            for (int i = 0; i < polygonPoints.Count - 1; i++)
            {
                polygonLines.Add(new SymmetricLine(polygonPoints[i], polygonPoints[i + 1], rColor, gColor, bColor));
            }
            polygonLines.Add(new SymmetricLine(polygonPoints[polygonPoints.Count - 1], polygonPoints[0], rColor, gColor, bColor));
            foreach (var line in polygonLines)
            {
                line.GetPoints();
            }
            FillEdgeTable();
            if(filled)
            FillPolygon(RFill, GFill, BFill);
        }
        public override bool HitDetection(Point a)
        {
            foreach (var line in polygonLines)
            {
                foreach(var point in line.points)
                {
                    if (point.X == a.X && point.Y == a.Y)
                        return true;
                }
               // if (line.points.Contains(new Point(a.X, a.Y, rColor, gColor, bColor)))
                 //   return true;

            }
            return false;
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
        public override void TransformPoints(Point p)
        {
            for (int i = 0; i < polygonPoints.Count; i++)
            {
                polygonPoints[i] = new Point(polygonPoints[i].X + p.X, polygonPoints[i].Y + p.Y, rColor, gColor, bColor);
            }
            polygonLines.Clear();
            gotPoints = false;
        }
        public override void ClipRect(Rectangle rect)
        {
            var lineCount = polygonLines.Count;
            List<SymmetricLine> tempLines = new List<SymmetricLine>(polygonLines);

            //polygonLines.Clear();

            for (int i = 0; i < lineCount; i++)
            {
                clipLine(new Point(tempLines[i].a.X, tempLines[i].a.Y, rColor, gColor, bColor), new Point(tempLines[i].b.X, tempLines[i].b.Y, rColor, gColor, bColor), rect);
                //LiangBarsky(new Point(polygonLines[i].a.X, polygonLines[i].a.Y), new Point(polygonLines[i].b.X, polygonLines[i].b.Y), rect);
            }
        }
        private void LiangBarsky(Point p1, Point p2, Rectangle clip)
        {
            float dx = p2.X - p1.X, dy = p2.Y - p1.Y, tE = 0, tL = 1;
            if (Clip(-dx, p1.X - clip.left, ref tE, ref tL))
                if (Clip(dx, clip.right - p1.X, ref tE, ref tL))
                    if (Clip(-dy, p1.Y - clip.bottom, ref tE, ref tL))
                        if (Clip(dy, clip.top - p1.Y, ref tE, ref tL))
                        {
                            if (tL < 1)
                            {
                                p2.X = (int)(p1.X + dx * tL);
                                p2.Y = (int)(p1.Y + dy * tL);
                            }
                            if (tE > 0)
                            {
                                p1.X += (int)(dx * tE);
                                p1.Y += (int)(dy * tE);
                            }
                            polygonLines.Add(new SymmetricLine(p1, p2, 200, gColor, bColor));

                            //drawLine(p1, p2);
                        }



        }
        private bool Clip(float denom, float numer, ref float tE, ref float tL)
        {
            if (denom == 0)
            { //Paralel line
                if (numer > 0)
                    return false; // outside ‐ discard
                return true; //skip to next edge
            }
            float t = numer / denom;
            if (denom > 0)
            { //PE
                if (t > tL) //tE > tL ‐ discard
                    return false;
                if (t > tE)
                    tE = t;
            }
            else
            { //denom < 0 ‐ PL
                if (t < tE) //tL < tE ‐ discard
                    return false;
                if (t < tL)
                    tL = t;
            }
            return true;
        }

        int clipTest(float p, float q, ref float tl, ref float t2)
        {
            float r;
            int retVal = 1;

            //line entry point
            if (p < 0.0)
            {

                r = q / p;

                // line portion is outside the clipping edge
                if (r > t2)
                    retVal = 0;

                else
                if (r > tl)
                    tl = r;
            }

            else

            //line leaving point
            if (p > 0.0)
            {
                r = q / p;

                // line portion is outside     
                if (r < tl)
                    retVal = 0;

                else if (r < t2)
                    t2 = r;
            }

            // p = 0, so line is parallel to this clipping edge 
            else

            // Line is outside clipping edge 
            if (q < 0.0)
                retVal = 0;

            return (retVal);
        }

        void clipLine(Point p1, Point p2, Rectangle rect)
        {
            float t1 = 0, t2 = 1, dx = p2.X - p1.X, dy;

            // inside test wrt left edge
            if (clipTest(-dx, p1.X - rect.a.X, ref t1, ref t2) == 1)

                // inside test wrt right edge 
                if (clipTest(dx, rect.b.X - p1.X, ref t1, ref t2) == 1)

                {
                    dy = p2.Y - p1.Y;

                    // inside test wrt bottom edge 
                    if (clipTest(-dy, p1.Y - rect.a.Y, ref t1, ref t2) == 1)

                        // inside test wrt top edge 
                        if (clipTest(dy, rect.b.Y - p1.Y, ref t1, ref t2) == 1)
                        {

                            if (t2 < 1.0)
                            {
                                p2.X = (int)(p1.X + t2 * dx);
                                p2.Y = (int)(p1.Y + t2 * dy);
                            }

                            if (t1 > 0.0)
                            {
                                p1.X += (int)(t1 * dx);
                                p1.Y += (int)(t1 * dy);
                            }

                            // lineDDA(ROUND(p1.x), ROUND(p1.y), ROUND(p2.x), ROUND(p2.y));
                            polygonLines.Add(new SymmetricLine(p1, p2, 200, gColor, bColor));
                        }
                }
            //points.Clear();
            foreach (var line in polygonLines)
            {
                line.GetPoints();
            }
            //gotPoints = false;

        }

        private int CheckIfBucketExists(int Y)
        {
            foreach (var bucket in edgeTable)
            {
                foreach (var edgeInBucket in bucket)
                {
                    if (edgeInBucket.ymin == Y)
                        return edgeTable.IndexOf(bucket);
                }

            }
            return -1;


        }
        private void FillEdgeTable()
        {
            int yMax = 0;
            int xMin = 0;
            int yMin = 0;
            float dxdy = 0;

            foreach (var line in polygonLines)
            {
                dxdy = (float)(line.a.X - line.b.X) / (float)(line.a.Y - line.b.Y);
                if (line.a.Y > line.b.Y)
                {
                    yMax = line.a.Y;
                    yMin = line.b.Y;
                    xMin = line.b.X;

                }
                else
                {
                    yMax = line.b.Y;
                    yMin = line.a.Y;
                    xMin = line.a.X;
                }
                int indexOfBucket = CheckIfBucketExists(yMin);
                if (indexOfBucket != -1)
                {
                    edgeTable[indexOfBucket].Add(new EdgeInBucket(yMax, xMin,yMin, dxdy));
                }
                else
                {
                    edgeTable.Add(new List<EdgeInBucket>());
                    edgeTable[edgeTable.Count - 1].Add(new EdgeInBucket(yMax, xMin,yMin, dxdy));

                }

            }


        }

        private (int, int) FindSmallestYInEdgeTable()
        {
            int smallesty = edgeTable[0][0].ymin;
            int indexOfSmallestY = 0;

            foreach (var bucket in edgeTable)
            {
                foreach (var edgeInBucket in bucket)
                {
                    if (edgeInBucket.ymin < smallesty)
                    {
                        smallesty = edgeInBucket.ymin;
                        indexOfSmallestY = edgeTable.IndexOf(bucket);
                    }

                }
            }
            return (smallesty, indexOfSmallestY);
        }

        private int FindIndexOfYInEdgeTable(int Y)
        {
            foreach (var bucket in edgeTable)
            {
                foreach (var edgeInBucket in bucket)
                {
                    if (edgeInBucket.ymin == Y)
                    {
                        return edgeTable.IndexOf(bucket);
                    }

                }
            }
            return -1;


        }
        private void DrawLinesBetweenPoints(int Y, byte _R,byte _G, byte _B)
        {

            for (int i = 0; i < activeEdgeTable.Count-1; i += 2)
            {
                if ((int)activeEdgeTable[i].xmin == (int)activeEdgeTable[i + 1].xmin)
                    continue;
                polygonLines.Add(new SymmetricLine(new Point((int)activeEdgeTable[i].xmin+1, Y, _R, _G, _B), new Point((int)activeEdgeTable[i + 1].xmin+1,Y, _R, _G, _B), _R, _G, _B));
                polygonLines[polygonLines.Count - 1].GetPoints();

            }


        }

        private void DrawLinesBetweenPoints(int Y, BitmapData srcData, byte[] buffer)
        {

            for (int i = 0; i < activeEdgeTable.Count - 1; i += 2)
            {
                if ((int)activeEdgeTable[i].xmin == (int)activeEdgeTable[i + 1].xmin)
                    continue;
                // polygonLines.Add(new SymmetricLine(new Point((int)activeEdgeTable[i].xmin + 1, Y), new Point((int)activeEdgeTable[i + 1].xmin + 1, Y), _R, _G, _B));
                // polygonLines[polygonLines.Count - 1].GetPoints();
                for (int j = (int)activeEdgeTable[i].xmin + 1; j < (int)activeEdgeTable[i + 1].xmin + 1; j++)
                {
                    buffer[Y * srcData.Stride + j * 4] = (byte)bColor;
                    buffer[Y * srcData.Stride + j * 4 + 1] = (byte)gColor;
                    buffer[Y * srcData.Stride + j * 4 + 2] = (byte)rColor;
                }

            }


        }
        private void RemoveEdgesFromAET(int Y)
        {
            for (int i = 0; i < activeEdgeTable.Count;)
            {
                if (activeEdgeTable[i].ymax == Y)
                {
                    activeEdgeTable.RemoveAt(i);
                }
                else
                {
                    i++;
                }


            }


        }
        public override void FillPolygon(byte _R, byte _G, byte _B)
        {
            filled = false;
            
            //if(!gotPoints)
            GetPoints();
            filled = true;
            RFill = _R;
            GFill = _G;
            BFill = _B;
            (int smallestY, int indexOfY) = FindSmallestYInEdgeTable();
            activeEdgeTable.Clear();

            while (activeEdgeTable.Count != 0 || edgeTable.Count != 0)
            {
                indexOfY = FindIndexOfYInEdgeTable(smallestY);
                if (indexOfY != -1)
                {
                    if (activeEdgeTable.Count != 0)
                    {
                         activeEdgeTable.AddRange(edgeTable[indexOfY]);
                    }
                    else
                    activeEdgeTable = new List<EdgeInBucket>(edgeTable[indexOfY]);
                    edgeTable.RemoveAt(indexOfY);
                }

                activeEdgeTable.Sort((edge1, edge2) => edge1.xmin.CompareTo(edge2.xmin));

                DrawLinesBetweenPoints(smallestY,_R,_G,_B);


                smallestY++;

                RemoveEdgesFromAET(smallestY);
                for (int i = 0; i < activeEdgeTable.Count; i++)
                {
                    activeEdgeTable[i] = new EdgeInBucket(activeEdgeTable[i].ymax, activeEdgeTable[i].xmin + activeEdgeTable[i].dxdy,  activeEdgeTable[i].ymin, activeEdgeTable[i].dxdy);
                }

           
            }
           // gotPoints = false;
        }

        public override Image FillPolygon(Image imgSource)
        {
            filled = false;

            //if(!gotPoints)
            GetPoints();
            filled = true;



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

            
           

            

            




            (int smallestY, int indexOfY) = FindSmallestYInEdgeTable();
            activeEdgeTable.Clear();

            while (activeEdgeTable.Count != 0 || edgeTable.Count != 0)
            {
                indexOfY = FindIndexOfYInEdgeTable(smallestY);
                if (indexOfY != -1)
                {
                    if (activeEdgeTable.Count != 0)
                    {
                        activeEdgeTable.AddRange(edgeTable[indexOfY]);
                    }
                    else
                        activeEdgeTable = new List<EdgeInBucket>(edgeTable[indexOfY]);
                    edgeTable.RemoveAt(indexOfY);
                }

                activeEdgeTable.Sort((edge1, edge2) => edge1.xmin.CompareTo(edge2.xmin));

                DrawLinesBetweenPoints(smallestY, srcData,buffer);


                smallestY++;

                RemoveEdgesFromAET(smallestY);
                for (int i = 0; i < activeEdgeTable.Count; i++)
                {
                    activeEdgeTable[i] = new EdgeInBucket(activeEdgeTable[i].ymax, activeEdgeTable[i].xmin + activeEdgeTable[i].dxdy, activeEdgeTable[i].ymin, activeEdgeTable[i].dxdy);
                }


            }
            //create a new bitmap with changed pixel rgb values
            Bitmap resImg = new Bitmap(width, height);
            BitmapData resData = resImg.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, resData.Scan0, bytes);
            resImg.UnlockBits(resData);
            return resImg;
            // gotPoints = false;
        }
    }
        internal class Rectangle : Polygon
        {
            public int top, bottom, left, right;
            public Rectangle(Point a, Point b, byte R, byte G, byte B, List<Point> polygonPoints) : base(a, b, R, G, B, polygonPoints)
            {
                this.polygonPoints.Clear();
                this.polygonPoints.Add(a);
                this.polygonPoints.Add(new Point(b.X, a.Y, rColor, gColor, bColor));
                this.polygonPoints.Add(b);
                this.polygonPoints.Add(new Point(a.X, b.Y, rColor, gColor, bColor));
                if (a.X < b.X)
                {
                    top = a.Y; bottom = b.Y; left = a.X; right = b.X;
                }
                else
                {
                    top = b.Y; bottom = a.Y; left = b.X; right = a.X;
                }

                //foreach(var point in this.polygonPoints)
                // {
                //     if (point.Y < top)
                //         top = point.Y;
                //     if (point.Y > bottom)
                //         bottom = point.Y;
                //     if (point.X < left)
                //         left = point.X;
                //     if (point.X > right)
                //         right = point.X;
                // }
            }

            public override void ChangeSize(Point p, Point t)
            {
                bool vertexHit = false;
                bool lineHit = false;
                int index = 0;
                gotPoints = false;
                Point temp;

                foreach (var point in polygonPoints)
                {
                    if (Math.Abs(point.X - p.X) < 3 && Math.Abs(point.Y - p.Y) < 3)
                    {
                        vertexHit = true;
                        break;
                    }
                    index++;
                }
                if (vertexHit)
                {
                    temp = new Point(polygonPoints[(index + 2) % 4].X, polygonPoints[(index + 2) % 4].Y, rColor, gColor, bColor);
                    polygonPoints.Clear();
                    polygonLines.Clear();
                    a = temp;
                    b = t;
                    this.polygonPoints.Add(temp);
                    this.polygonPoints.Add(new Point(temp.X, t.Y, rColor, gColor, bColor));
                    this.polygonPoints.Add(t);
                    this.polygonPoints.Add(new Point(t.X, temp.Y, rColor, gColor, bColor));

                }
                else
                {
                    index = 0;
                    foreach (var line in polygonLines)
                    {
                        if (line.HitDetection(p))
                        {
                            lineHit = true;
                            break;
                        }

                        index++;
                    }
                    if (lineHit)
                    {
                        if (Math.Abs(polygonLines[index].points[0].X - (polygonLines[index].points[polygonLines[index].points.Count - 1].X)) < 2)
                        {
                            for (int i = 0; i < polygonPoints.Count; i++)
                            {
                                if (polygonLines[index].points.Contains(polygonPoints[i]))
                                {
                                    polygonPoints[i] = new Point(t.X, polygonPoints[i].Y, rColor, gColor, bColor);
                                }
                            }

                        }
                        else if (Math.Abs(polygonLines[index].points[0].Y - (polygonLines[index].points[polygonLines[index].points.Count - 1].Y)) < 2)
                        {
                            for (int i = 0; i < polygonPoints.Count; i++)
                            {
                                if (polygonLines[index].points.Contains(polygonPoints[i]))
                                {
                                    polygonPoints[i] = new Point(polygonPoints[i].X, t.Y, rColor, gColor, bColor);
                                }
                            }

                        }
                        polygonLines.Clear();

                    }

                }

            }
            public override void GetPoints()
            {
                points.Clear();
                gotPoints = true;
                for (int i = 0; i < polygonPoints.Count - 1; i++)
                {
                    polygonLines.Add(new SymmetricLine(polygonPoints[i], polygonPoints[i + 1], rColor, gColor, bColor));
                }
                polygonLines.Add(new SymmetricLine(polygonPoints[polygonPoints.Count - 1], polygonPoints[0], rColor, gColor, bColor));
                foreach (var line in polygonLines)
                {
                    line.GetPoints();
                }
            }
            public override void ThickenVertices()
            {
                //foreach (var line in polygonLines)
                //{


                //    for (int i = 0; i < 2; i++)
                //    {

                //        line.points.Add(new Point(a.X + i, a.Y + i));
                //        line.points.Add(new Point(a.X - i, a.Y - i));

                //        line.points.Add(new Point(a.X, a.Y + i));
                //        line.points.Add(new Point(a.X, a.Y - i));

                //        line.points.Add(new Point(a.X + i, a.Y));
                //        line.points.Add(new Point(a.X - i, a.Y));



                //        line.points.Add(new Point(b.X + i, b.Y + i));
                //        line.points.Add(new Point(b.X - i, b.Y - i));



                //        line.points.Add(new Point(b.X, b.Y + i));
                //        line.points.Add(new Point(b.X, b.Y - i));


                //        line.points.Add(new Point(b.X + i, b.Y));
                //        line.points.Add(new Point(b.X - i, b.Y));

                //    }
                //}
            }



        }

    }

