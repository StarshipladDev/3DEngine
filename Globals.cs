using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{

    /**
     * Globals is a static/constant class used to convey information between listeners and the main form while the application runs.
     * 
     */
    public static class Globals
    {

        
        public static bool SinglePlayer = true;
        public static bool drawGun = true;
        public static bool drawLines = false;
        public static bool drawFill = true;
        public static bool drawText = true;
        public static bool drawn = true;
        public static bool Pause = false;
        public static bool pauseForInfo = false;
        public const int maxView = 20;
        public const int cellSize = 20;
        //Draw directin /Drew gun //Turn gun back //draw client confirm //draw server confirm //Draw message //Draw Server receive
        public static bool[] flags = new bool[7];
        //Base animaions
        public static int[] animations = new int[5];
        public static bool readyToDraw = true;
        public static string Message = String.Empty;
        public static string ServerMessage = String.Empty;
        public static string port = "8004";
        public static string Address = "localhost";
        public static string clientName = "This Client";
        public const int MAXFRAMES = 8;
        public const int INTERVALTIMEMILISECONDS = 1000 / 5;
        public const int MaxPossibleDepth = 20;
        public static int ticks = 0;

        /// <summary>
        /// Stolen from https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp 04/03/2020
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Crops a sub-rectangle of a given image
        /// </summary>
        /// <param name="source">The imagee file to draw a sub-image from</param>
        /// <param name="section">The specified size and position of the rectangle subimage</param>
        /// <returns></returns>
        public static Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }
    }
}
