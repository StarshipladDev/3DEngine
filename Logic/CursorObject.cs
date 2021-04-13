using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoomCloneV2
{

    //Handels the position and 'swaying' movement of the firing cursor.
    //Master co-ords are the 'centerpoint' of the sway being taken
    //Goal co-ords are the points the cursor will mvoe towards each 'step'
    //co-ords are the acctual position of the center of the cursor
    class CursorObject
    {
        int masterx = 0;
        int mastery = 0;
        int maxShift = 0;
        int x = 0;
        int y = 0;
        int goalx = 0;
        int goaly = 0;
        int stepSize;
        int imageHeight = 0;
        int imageWidth = 0;
        Bitmap cursorImage = null;
        public CursorObject(Bitmap cursorImage,int x,int y,int maxShift,int stepSize)
        {
            this.cursorImage = cursorImage;
            this.imageWidth = cursorImage.Width;
            this.imageHeight = cursorImage.Height;
            this.masterx = x;
            this.mastery = y;
            this.maxShift = maxShift;
            this.stepSize = stepSize;
            this.x = masterx;
            this.y = mastery;
            NewGoal();
            x = goalx;
            y = goaly;
            
        }
        public void DebugPrint()
        {
            Debug.WriteLine("Master x,y are "+masterx+" "+mastery);
            Debug.WriteLine("x,y are " + x + " " + y);
            Debug.WriteLine("Goal x,y are " + goalx + " " + goaly);
            Debug.WriteLine("Stepsize is  " + stepSize);
        }
        public void Draw(Graphics g)
        {
            g.DrawImage(this.cursorImage, x-(this.cursorImage.Width/2), y-(this.cursorImage.Height / 2));
            if (Globals.drawOutline)
            {
                g.DrawRectangle(new Pen(Color.Red), new Rectangle(x - (this.cursorImage.Width / 2), y - (this.cursorImage.Height / 2), this.cursorImage.Width, this.cursorImage.Height));
            }
        }
        public int[] GetCoords()
        {
            int[] returna = new int[2];
            returna[0] = x;
            returna[1] = y;
            return returna;
        }
        public void NewGoal()
        {

            Random rand = new Random();
            //Do x
            if (x > masterx)
            {
                goalx = x-rand.Next(maxShift);
            }
            else
            {
                goalx = x+rand.Next(maxShift);
            }
            //Do y
            if (y < mastery)
            {
                goaly = x + rand.Next(maxShift);
            }
            else
            {
                goaly = y - rand.Next(maxShift);
            }
        }
        public void MoveCursor()
        {
            if(goalx==x && goaly == y)
            {
                NewGoal();
            }
            x=MoveTowards(x, goalx);
            y=MoveTowards(y, goaly);
            

        }
        public int MoveTowards(int value, int goal)
        {
            if (value > goal)
            {
                if (value - goal <= stepSize)
                {
                    value = goal;
                }
                else
                {
                    value -= stepSize;
                }
            }
            else if (goal > value)
            {
                if (goal - value <= stepSize)
                {
                    value = goal;
                }
                else
                {
                    value += stepSize;
                }
            }
            return value;
            
        }
        public Bitmap GetCursor()
        {
            return this.cursorImage;
        }
    }
}
