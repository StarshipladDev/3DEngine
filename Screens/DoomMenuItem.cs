using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2.Screens
{


    
    class DoomMenuItem
    {
        public int x;
        public int y;
        public int length;
        public int height;
        public Image drawImage;
        public delegate void actionfunction();
        actionfunction ac;

        /// <summary>
        /// MenuItem is the constructor for DoomScroll's 'MenuItem' class.
        /// It takes positional data and a function to run on click.
        /// </summary>
        /// <param name="x"> The 'length' point in px to draw top left corner to</param>
        /// <param name="y">The 'height' point in px to draw top left corner to</param>
        /// <param name="image">A JPG or PNG image to display as the menu item</param>
        /// <param name="ac">A function returning void that will be run when <see cref="OnClick"/> is run</param>
        public DoomMenuItem(int x, int y,Image image, actionfunction ac)
        {
            this.x = x;
            this.y = y;
            this.length = image.Width;
            this.height = image.Height;
            this.drawImage = image;
            this.ac = ac;
        }
        public void  Draw(Graphics g)
        {
            g.DrawImage(drawImage, new Point(x, y));
        }
        public void OnClick()
        {
            this.ac();
        }

    }
}
