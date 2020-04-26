using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    /// <summary>
    /// Unit is the class responsible for holding the information required to draw an object when viewed by the palyer,
    ///and also as an area to stroe unit-instance-specific information such as a damage and health
    /// </summary>
    public class Unit
    {
        Bitmap unitImage;
        Bitmap deadUnitImage;
        Bitmap returnImage;
        private int health = 20;
        public Point topLeft = new Point(-1, -1);
        public Point bottomRight = new Point(-1, -1);
        public bool alive = true;
        public bool dying = false;
        int offset = 0;
        /// <summary>
        /// setUpUnit is a constructor but like later
        /// </summary>
        /// <param name="unitImage">Provides a bitmap image to be read whenever the unit is 'alive'</param>
        /// <param name="deadUnitImage">Provides a bitmap image to be read/drawn wheenever the unit is 'dead'</param>
        public void setUpUnit(Bitmap unitImage, Bitmap deadUnitImage)
        {
            this.unitImage = unitImage;
            this.deadUnitImage = deadUnitImage;
            this.returnImage = this.unitImage;
            offset = new Random().Next(8);
        }
        /// <summary>
        /// GetUnitImage in an accessor method to get the current image set as the unit's viewable image
        /// </summary>
        /// <returns>Returns the current image set as the unit's viewable image</returns>
        public Bitmap getUnitImage()
        {
            return this.returnImage;
        }
        /// <summary>
        /// setUnitImage in an accessor method to set the current image set as the unit's viewable image
        /// </summary>
        public void setUnitImage(Bitmap img)
        {
            this.unitImage = img;
        }
        /// <summary>
        /// DealDamage in an accessor method used to edit the unit's current health, and perform required changes to the unit's state
        /// such as making it's viewable image 'dead'
        /// </summary>
        public void DealDamage(int damage)
        {
            this.health -= damage;
            if (this.health <= 0)
            {
                this.returnImage = deadUnitImage;
                this.dying = true;
                this.alive = false;
                this.offset = Globals.MAXFRAMES - Globals.animations[0];
            }
        }
        public void Draw(Graphics g, int drawLength, int drawHeight, int[] topLefta)
        {
            if (alive || dying)
            {
                Bitmap drawImage = getUnitImage();
                int layer = 0;
                if ((Globals.animations[0] + offset) % Globals.MAXFRAMES >= Globals.MAXFRAMES / 2)
                {
                    layer = 1;
                }
                Rectangle section = new Rectangle(new Point(400 * (((Globals.animations[0] + offset) % Globals.MAXFRAMES) % 4), 400 * layer), new Size(400, 400));
                drawImage = (Globals.CropImage(drawImage, section));
                //drawImage=ResizeImage(drawImage,drawImage.Width / (((maxDepth - loopFromBack) * 2) -3 ), drawImage.Height / (((maxDepth - loopFromBack) * 2) -3));
                drawImage = Globals.ResizeImage(drawImage, drawLength, drawHeight);
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
                this.returnImage = Globals.ResizeImage(this.getUnitImage(), drawLength, drawHeight);
                g.DrawImage(getUnitImage(), new Point(topLefta[0], topLefta[1]));
                this.topLeft.X = topLefta[0];
                this.topLeft.Y = topLefta[1];
                this.bottomRight.X = topLefta[0] + getUnitImage().Width;
                this.bottomRight.Y = topLefta[0] + getUnitImage().Height;
            }

        }
    }
}
