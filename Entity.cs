using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    public class Entity
    {

        //The overall Iamge Frame
        public Bitmap unitImage;
        //The subsection of the unit image
        public Bitmap returnImage;
        public int x = 0;
        public int y = 0;
        public int offset = 0;
        public int sectionSize = 400;
        public int drawRatio = 1;
        public bool animated = true;
        public Point topLeft = new Point(-1, -1);
        public Point bottomRight = new Point(-1, -1);
        public bool alive = true;
        public bool dying = false;
        public void Draw(Graphics g, int drawLength, int drawHeight, int[] topLefta)
        {
            if (alive || dying)
            {
                Bitmap drawImage = unitImage;
                int layer = 0;
                int animNumber = (Globals.animations[0] + offset) % Globals.MAXFRAMES;
                if (animNumber >= Globals.MAXFRAMES / 2)
                {
                    layer = 1;
                }
                Rectangle section = new Rectangle(new Point(sectionSize * (animNumber % 4) + ((animNumber % (Globals.MAXFRAMES / 2) + 1) * (sectionSize/20)), (sectionSize * layer) + ((sectionSize/20) * (layer + 1))), new Size(sectionSize, sectionSize));
                drawImage = (Globals.CropImage(drawImage, section));
                //drawImage=ResizeImage(drawImage,drawImage.Width / (((maxDepth - loopFromBack) * 2) -3 ), drawImage.Height / (((maxDepth - loopFromBack) * 2) -3));
                drawImage = Globals.ResizeImage(drawImage, drawLength/drawRatio, drawHeight/drawRatio);
                g.DrawImage(drawImage, new Point(topLefta[0], topLefta[1]));
                this.topLeft.X = topLefta[0];
                this.topLeft.Y = topLefta[1];
                this.bottomRight.X = topLefta[0] + drawImage.Width;
                this.bottomRight.Y = topLefta[0] + drawImage.Height;
                if (alive)
                {
                    Random rand = new Random();
                    if (rand.Next(5) == 3 && alive)
                    {
                        if (rand.Next(2) == 0)
                        {
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer("Resources/Sound/Boof.wav");
                            player.Play();
                        }
                        else
                        {
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer("Resources/Sound/Boof2.wav");
                            player.Play();
                        }
                    }
                }
                else
                {
                    if ((Globals.animations[0] + offset) % Globals.MAXFRAMES == Globals.MAXFRAMES - 1)
                    {
                        this.returnImage = drawImage;
                        dying = false;
                    }
                }
            }
            else
            {
                if (!animated)
                {
                    this.returnImage = Globals.ResizeImage(unitImage, drawLength/drawRatio, drawHeight/drawRatio);
                }
                else
                {
                    this.returnImage = Globals.ResizeImage(this.returnImage, drawLength / drawRatio, drawHeight / drawRatio);
                }
                int excess = 0;
                if (drawRatio > 1)
                {
                    excess = drawLength / 2;
                }
                g.DrawImage(this.returnImage, new Point(topLefta[0]+ excess, topLefta[1]+ excess));
                this.topLeft.X = topLefta[0] + excess;
                this.topLeft.Y = topLefta[1] + excess;
                this.bottomRight.X = topLefta[0] + unitImage.Width + excess;
                this.bottomRight.Y = topLefta[0] + unitImage.Height + excess;
            }

        }
    }

}
