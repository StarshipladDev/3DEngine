using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    public class Powerup
    {

        Bitmap baseImage;
        System.Media.SoundPlayer player;
        public Bitmap[] animationImages;
        public Player.PowerUpTypes powerUpType;

        public Powerup(Bitmap baseImage,Player.PowerUpTypes p)
        {

            this.baseImage = baseImage;
            this.animationImages = BuildPowerup();
            this.powerUpType = p;
        }
        /// <summary>
        /// BuildPowerup is a Method that is used to return a -frame animation array that can be itterated through
        /// to aniamte the powerup in use.
        /// </summary>
        /// <note>
        /// RECTANGLE DEFINITIONS ARE CURRENTLY HARDCODED
        /// </note>
        /// <returns> A 8 Frame Bitmap array</returns>
        private Bitmap[] BuildPowerup()
        {
            //Frame is every 2 * (i+1), 40 along
            Bitmap[] powerupFrames = new Bitmap[8];
            Graphics g = null;
            int layer = 0;
            for (int i = 0; i < powerupFrames.Length; i++)
            {

                if (i - 4 >= 0)
                {
                    layer = 1;
                }
                Bitmap tempImage = new Bitmap(baseImage.Width/4, baseImage.Height/2, PixelFormat.Format32bppPArgb);
                Rectangle rec = new Rectangle((i % 4) * 400+(((i%4)+1) *20), layer * 400 +/*offset*/((layer+1)*20),400,400);
                Debug.WriteLine(String.Format("Rectangle made with x,y {0},{1} and width/height {2}/{3}", rec.X, rec.Y, rec.Width, rec.Height));
                g = Graphics.FromImage(tempImage);
                tempImage = baseImage.Clone(rec, tempImage.PixelFormat);
                powerupFrames[i] = tempImage;
            }
            player = new System.Media.SoundPlayer("Resources/Sound/Bang_001_001.wav");

            g.Dispose();
            return powerupFrames;

        }

        public System.Media.SoundPlayer GetPowerupSound()
        {
            return player;
        }

    }
}
