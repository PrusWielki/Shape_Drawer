﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Windows.Point;

namespace Shape_Drawer
{
    internal abstract class ShapeDrawer
    {
        public List<Point> points = new List<Point>();

        public int rColor = 0;
        public int gColor = 0;
        public int bColor = 0;

        public Point a;
        public Point b;

        protected ShapeDrawer(Point a, Point b)
        {
            this.a = a;
            this.b = b;
        }

        abstract public Image Draw(Image imgSource);
        abstract public void GetPoints();

        abstract public void TransformPoints(Point a);


    }

    internal class ShapeDrawerConcrete : ShapeDrawer
    {
        public ShapeDrawerConcrete(Point a, Point b) : base(a, b)
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
            throw new NotImplementedException();
        }

        public override void TransformPoints(Point p)
        {
            points.Clear();
            a = new Point(a.X + p.X, a.Y + p.Y);
            b = new Point(b.X + p.X, b.Y + p.Y);
            //  for(int i=0;i<points.Count; i++)
            //{
            //  points[i] = new Point(points[i].X + a.X, points[i].Y + a.Y);

            //            }



        }
    }

    internal class SymmetricLine : ShapeDrawerConcrete
    {

        //public new List<Point> points = new List<Point>();

        public SymmetricLine(Point a, Point b) : base(a, b) { }

        
        public override void GetPoints()
        {



            int dx = (int)(b.X - a.X);
            int dy = (int)(b.Y - a.Y);


            int d = dy - (dx / 2);
            int x = (int)a.X, y = (int)a.Y;


            points.Add(new Point(x, y));


            while (x < (int)b.X)
            {
                x++;

                if (d < 0)
                    d = d + dy;
                else
                {
                    d += (dy - dx);
                    y++;
                }

                points.Add(new Point(x, y));

            }

            //int d = 2 * dy - dx;
            //int dE = 2 * dy;
            //int dNE = 2 * (dy - dx);

            //int xf = (int)a.X, yf = (int)a.Y;
            //int xb = (int)b.X, yb = (int)b.Y;

            //buffer[yf * srcData.Stride + xf * 4] = (byte)0;
            //buffer[yf * srcData.Stride + xf * 4+1] = (byte)0;
            //buffer[yf * srcData.Stride + xf * 4+2] = (byte)0;


            //buffer[yb * srcData.Stride + xb * 4] = (byte)0;
            //buffer[yb * srcData.Stride + xb * 4 + 1] = (byte)0;
            //buffer[yb * srcData.Stride + xb * 4 + 2] = (byte)0;


            //while (xf < xb)
            //{
            //    ++xf; --xb;
            //    if (d < 0)
            //        d += dE;
            //    else
            //    {
            //        d += dNE;
            //        ++yf;
            //        --yb;
            //    }
            //    buffer[yf * srcData.Stride + xf * 4] = (byte)0;
            //    buffer[yf * srcData.Stride + xf * 4 + 1] = (byte)0;
            //    buffer[yf * srcData.Stride + xf * 4 + 2] = (byte)0;


            //    buffer[yb * srcData.Stride + xb * 4] = (byte)0;
            //    buffer[yb * srcData.Stride + xb * 4 + 1] = (byte)0;
            //    buffer[yb * srcData.Stride + xb * 4 + 2] = (byte)0;
            //}


            //create a new bitmap with changed pixel rgb values




            // putPixel(xf, yf);
            // putPixel(xb, yb);

        }

       
       
    }
    internal class MidpointCircle : ShapeDrawerConcrete
    {
        //public new List<Point> points=new List<Point>();

        public MidpointCircle(Point a, Point b) : base(a, b) { }
        

        public override void GetPoints()
        {




            int R = (int)Math.Sqrt((Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)));

            int x = R;
            int y = 0;
            int P = 1 - R;


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
            y = R;


            points.Add(new Point((int)(x + a.X), (int)(a.Y - y)));

            int dE = 3;
            int dSE = 5 - 2 * R;
            int d = 1 - R;
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


            //create a new bitmap with changed pixel rgb values





            // putPixel(xf, yf);
            // putPixel(xb, yb);

        }
       
    }
}
