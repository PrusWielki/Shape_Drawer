using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shape_Drawer
{
    internal abstract class ShapeDrawer
    {
        abstract public void Draw();

    }

    internal class SymmetricLine : ShapeDrawer
    {
        public override void Draw()
        {
           /* int dx = x2 ‐ x1;
            int dy = y2 ‐ y1;
            int d = 2 * dy ‐ dx;
            int dE = 2 * dy;
            int dNE = 2 * (dy ‐ dx);
            int xf = x1, yf = y1;
            int xb = x2, yb = y2;
            putPixel(xf, yf);
            putPixel(xb, yb);
            while (xf < xb)
            {
                ++xf; ‐‐xb;
                if (d < 0)
                    d += dE;
                else
                {
                    d += dNE;
                    ++yf;
                    ‐‐yb;
                }
                putPixel(xf, yf);
                putPixel(xb, yb);
            }*/
        }
    }
}
