using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2.Screens
{
    class MapDraw
    {
        public void PaintDrawing(Graphics g)
        {
            for (int i = 0; i < Globals.cellListGlobal.GetLength(0); i++)
            {
                for (int f = 0; f < Globals.cellListGlobal.GetLength(1); f++)
                {
                    Globals.WriteDebug("MapDraw-> PaintDrawing","Drawing "+i+","+f,false);
                    Color c = Globals.drawColor;
                    if (Globals.cellListGlobal[i, f].GetMat() == false)
                    {
                        c = Globals.floorColor;
                    }
                    if (Globals.cellListGlobal[i, f].GetUnitOnCell() != null && !Globals.cellListGlobal[i, f].GetUnitOnCell().GetUnitType().Equals(Globals.UnitType.Player))
                    {
                        c = Color.Red;
                    }
                    int x = (i * 10) + 50;
                    int y = (f * 10) + 50;
                    g.FillRectangle(new SolidBrush(c), new Rectangle(x, y, 10, 10));

                }
            }
            // g.Dispose();
        }

        
    }
}
