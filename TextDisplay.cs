using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2.Overlays
{
    class TextDisplay
    {
        private string textToDisplay;
        private int x, y;

        public TextDisplay(string textToDisplay,int x, int y)
        {
            this.textToDisplay = textToDisplay;
            this.x = x/2;
            this.y = y/2;
        }

        /// <summary>
        /// GetImage draw the stated text to a background on a given Graphics object
        /// </summary>
        /// <param name="g">The graphics object that will be drawn to</param>
        /// <param name="x">The x value of the mid point of the image being drawn to</param>
        /// <param name="y">The y value of the mid point of the image being drawn to</param>
        /// <param name="pfc">The Font Collection where the first family is the font that will be used to draw</param>
        public void GetImage(Graphics g,PrivateFontCollection pfc)
        {
            Globals.WriteDebug("TextDsiplay->GetImage","Printing "+this.textToDisplay,true);
            g.DrawImage(Image.FromFile("Resources/Images/Utility/TextBackground.png"), new PointF(x - 200, y - 200));
            g.DrawString(this.textToDisplay, new Font(pfc.Families[0], 16), new SolidBrush(Color.Black),x - 200, y - 100);
        }

    }
}
