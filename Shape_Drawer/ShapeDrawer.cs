using System;
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
        abstract public Image Draw(Point a, Point b, Image imgSource);

    }

    internal class SymmetricLine : ShapeDrawer
    {
        public override Image Draw(Point a, Point b, Image imgSource)
        {

            int width = imgSource.Width;
            int height = imgSource.Height;
            Bitmap newBitmap = (Bitmap)imgSource.Clone();


            BitmapData srcData = newBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            //hold the amount of bytes needed to represent the image's pixels
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];

            //copy image data to the buffer
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            newBitmap.UnlockBits(srcData);

            int current = 0;

            int dx = (int)(a.X - b.X);
            int dy = (int)(a.Y - b.Y);

            int d = 2 * dx - dy;
            int dE = 2 * dy;
            int dNE = 2 * (dy - dx);

            int xf = (int)a.X, yf = (int)a.Y;
            int xb = (int)b.X, yb = (int)b.Y;

            buffer[yf * srcData.Stride + xf * 4] = (byte)0;
            buffer[yf * srcData.Stride + xf * 4+1] = (byte)0;
            buffer[yf * srcData.Stride + xf * 4+2] = (byte)0;


            buffer[yb * srcData.Stride + xb * 4] = (byte)0;
            buffer[yb * srcData.Stride + xb * 4 + 1] = (byte)0;
            buffer[yb * srcData.Stride + xb * 4 + 2] = (byte)0;


            while (xf < xb)
            {
                ++xf; --xb;
                if (d < 0)
                    d += dE;
                else
                {
                    d += dNE;
                    ++yf;
                    --yb;
                }
                buffer[yf * srcData.Stride + xf * 4] = (byte)0;
                buffer[yf * srcData.Stride + xf * 4 + 1] = (byte)0;
                buffer[yf * srcData.Stride + xf * 4 + 2] = (byte)0;


                buffer[yb * srcData.Stride + xb * 4] = (byte)0;
                buffer[yb * srcData.Stride + xb * 4 + 1] = (byte)0;
                buffer[yb * srcData.Stride + xb * 4 + 2] = (byte)0;
            }


            //create a new bitmap with changed pixel rgb values
            Bitmap resImg = new Bitmap(width, height);
            BitmapData resData = resImg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, resData.Scan0, bytes);
            resImg.UnlockBits(resData);

            return resImg;



            
           // putPixel(xf, yf);
           // putPixel(xb, yb);
            
        }
    }
}
