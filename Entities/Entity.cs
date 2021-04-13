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
        public enum EntityTypes{
            BorningRegular,
            Projectile,
            Unit
        }
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
        public EntityTypes type = EntityTypes.BorningRegular;
        private Bitmap[] rootImages = new Bitmap[8];

        public Entity(String baseFile_p, int setionSize=400, bool animated= true)
        {
            if (dying)
            {
                offset = -Globals.animations[0];
            }
            Globals.WriteDebug("Entity -> Constructor -> ", "Base File is "+baseFile_p, true);
            Bitmap baseFile = (Bitmap)Image.FromFile(baseFile_p);
            if (animated)
            {
                for (int i = 0; i < Globals.MAXFRAMES; i++)
                {
                    int layer = 0;
                    if (i >= Globals.MAXFRAMES / 2)
                    {
                        layer = 1;
                    }
                    Rectangle section = new Rectangle(new Point(sectionSize * (i % 4) + ((i % (Globals.MAXFRAMES / 2) + 1) * (sectionSize / 20)), (sectionSize * layer) + ((sectionSize / 20) * (layer + 1))), new Size(sectionSize, sectionSize));
                    rootImages[i] = Globals.CropImage(baseFile, section);
                }
            }
            else
            {
                rootImages[0] = baseFile;
            }
            
        }
        public virtual void Draw(Graphics g, int drawLength, int drawHeight, int[] topLefta)
        {
            
            Bitmap drawImage;
            int animNumber;
            if (animated)
            {
                animNumber = (Globals.animations[0] + offset) % Globals.MAXFRAMES;
            }
            else
            {
                animNumber = 0;
            }
            drawImage = Globals.ResizeImage(rootImages[animNumber], drawLength/drawRatio, drawHeight/drawRatio);
            Globals.WriteDebug("Entity->Draw->", "animNumber is " + animNumber + " drawImage Height is" + rootImages[animNumber],false);
            this.topLeft.X = topLefta[0] + ((drawRatio / 2) * drawImage.Width);
            this.topLeft.Y = topLefta[1] + ((drawRatio / 2) * drawImage.Height);
            this.bottomRight.X = topLeft.X + drawImage.Width;
            this.bottomRight.Y = topLeft.Y + drawImage.Height;
            g.DrawImage(drawImage, new Point(topLeft.X, topLeft.Y));
            if (dying && (Globals.animations[0] + offset) % Globals.MAXFRAMES == Globals.MAXFRAMES - 1)
            {
                rootImages[0] = rootImages[Globals.MAXFRAMES - 1];
                dying = false;
                animated = false;
            }
            if (Globals.drawOutline)
            {
                g.DrawRectangle(new Pen(Color.Red), new Rectangle(topLeft.X,topLeft.Y,drawImage.Width,drawImage.Height));
                g.DrawRectangle(new Pen(Color.Cyan), new Rectangle(topLefta[0], topLefta[1], drawLength,drawHeight));
            }

        }
    }

}
