using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
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
        public  enum UnitType { Devil, SoldierGun , Player };
        public static Cell[,] cellListGlobal;
        public static bool SinglePlayer = true;
        public static bool drawGun = true;
        public static bool drawLines = false;
        public static bool drawFill = true;
        public static bool drawText = false;
        public static bool drawn = true;
        public static bool Pause = false;
        public static bool pauseForInfo = false;
        public static bool playingMusic = false;
        public static bool drawHUD = true;
        public static bool drawOutline = false;
        public static bool playMusic = false;

        public static bool writeDebug = true;

        public static int drawingPowerup = 9;
        public const int maxView = 20;
        public const int cellSize = 20;
        //Draw directin /Drew gun //Turn gun back //draw client confirm //draw server confirm //Draw message //Draw Server receive
        public static bool[] flags = new bool[7];
        //Base animations
        public static int[] animations = new int[5];
        public static bool readyToDraw = true;
        public static string Message = String.Empty;
        public static string ServerMessage = String.Empty;
        public static string port = "8004";
        public static string Address = "localhost";
        public static string clientName = "This Client";
        public static string playerFileName = "Player01";
        public const int MAXFRAMES = 8;
        public const int INTERVALTIMEMILISECONDS = 1000 / 5;
        public const int MaxPossibleDepth = 20;
        public static int ticks = 0;
        public static int debugTimes = 10;
        public static  Color floorColor;
        public static Color drawColor;
        public static Color roofColor;
        public static Color katanaColor;
        public static System.Media.SoundPlayer music = new System.Media.SoundPlayer();
        public static DateTime lastPlayerMusic = DateTime.Now;

        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);



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
            //image.Save("pre-enlarge"+width+"_"+height+".png");
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
                    //wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            //destImage.Save("post-enlarge" + width + "_" + height + ".png");
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

        public static Bitmap ReColorImage(Bitmap inputImage,Color colorToChange, Color colorToChangeTo,String location="Test.png")
        {
            DateTime then = DateTime.Now;
            Globals.WriteDebug("Globals->ReColorImage", "Recolouring", true);
            for (int i = 0; i < inputImage.Width; i++)
            {
                for(int f = 0; f < inputImage.Height; f++)
                {
                    if (inputImage.GetPixel(i, f).Equals(colorToChange))
                    {
                        inputImage.SetPixel(i, f, colorToChangeTo);
                    }
                }
            }
            //inputImage.Save(location);
            TimeSpan span = TimeSpan.FromMilliseconds(DateTime.Now.Millisecond - then.Millisecond);
            Debug.WriteLine("TIME FOR IMAGE WaS "+span.Milliseconds);
            return inputImage;

        }
        
        public static Point GetFreeCell(Random rand)
        {

            int xForBlock = rand.Next(Globals.cellSize - 2) + 1;
            int yForBLock = rand.Next(Globals.cellSize - 2) + 1;
            while (Globals.cellListGlobal[xForBlock, yForBLock].GetisUnitPresent() || Globals.cellListGlobal[xForBlock, yForBLock].GetMat() || Globals.cellListGlobal[xForBlock, yForBLock].GetPlayer() != -1)
            {

                Debug.WriteLine("Clinet:Trying co-ords" + xForBlock + "," + yForBLock);
                xForBlock = rand.Next(Globals.cellSize - 2) + 1;
                yForBLock = rand.Next(Globals.cellSize - 2) + 1;
            }
            Debug.WriteLine("Client: Got co-ords" + xForBlock + "," + yForBLock);
            return new Point(xForBlock, yForBLock);

        }
        public static List<Unit> FillMapWithEnemies()
        {
            List<Unit> units = new List<Unit>();
            Random rand = new Random();
            for (int i = 0; i < 9; i++)
            {
                Point enemyPoint = GetFreeCell(rand);
                int enemyType = rand.Next(2);
                if (enemyType > 0)
                {
                    units.Add(Globals.cellListGlobal[enemyPoint.X, enemyPoint.Y].CreateUnit(enemyPoint.X, enemyPoint.Y, i, Globals.UnitType.SoldierGun));

                }
                else
                {
                    units.Add(Globals.cellListGlobal[enemyPoint.X, enemyPoint.Y].CreateUnit(enemyPoint.X, enemyPoint.Y, i, Globals.UnitType.Devil));
                }
            }
            return units;
        }

        public static void Play(string audioPath)
        {
            if (playMusic)
            {
                mciSendString(@"open " + audioPath + " type waveaudio alias anAliasForYourSound", null, 0, IntPtr.Zero);
                mciSendString(@"seek anAliasForYourSound to start", null, 0, IntPtr.Zero);
                mciSendString(@"play anAliasForYourSound", null, 0, IntPtr.Zero);
            }
                
            
            
        }

        public static void StopMusic()
        {
            mciSendString(@"close anAliasForYourSound", null, 0, IntPtr.Zero);
        }

        public static void WriteDebug(String tag, String message, bool print){

            if (print && writeDebug)
            {
                Debug.WriteLine("\n---------------------");
                Debug.WriteLine("Debug at :" + tag);
                Debug.WriteLine(message);
                Debug.WriteLine("---------------------");

            }
        }

        public static Unit FindFirstUnit(int x, int y, Directions d, Cell[,] cellList)
        {
            Unit returnUnit = null;
            String tag = "FindFirstUnit";
            Globals.WriteDebug(tag, "Running FindFirstUnit", true);

            switch (d)
            {
                case Directions.DOWN:
                    while (y < cellList.GetLength(1))
                    {

                        y++;
                        if (cellList[x, y].GetMat())
                        {
                            return null;
                        }
                        if (cellList[x, y].GetUnitOnCell() != null)
                        {
                            Globals.WriteDebug(tag, "Return Unit is " + cellList[x, y].GetUnitOnCell().GetUnitType(), true);
                            return cellList[x, y].GetUnitOnCell();
                        }
                    }
                    break;
                case Directions.UP:
                    while (y > 0)
                    {
                        y--;
                        if (cellList[x, y].GetMat())
                        {
                            return null;
                        }
                        if (cellList[x, y].GetUnitOnCell() != null)
                        {
                            Globals.WriteDebug(tag, "Return Unit is " + cellList[x, y].GetUnitOnCell().GetUnitType(), true);
                            return cellList[x, y].GetUnitOnCell();
                        }
                    }
                    break;
                case Directions.LEFT:
                    while (x > 0)
                    {
                        x--;
                        if (cellList[x, y].GetMat())
                        {
                            return null;
                        }
                        if (cellList[x, y].GetUnitOnCell() != null)
                        {
                            Globals.WriteDebug(tag, "Return Unit is " + cellList[x, y].GetUnitOnCell().GetUnitType(), true);
                            return cellList[x, y].GetUnitOnCell();
                        }
                    }
                    break;
                case Directions.RIGHT:
                    while (x < cellList.GetLength(0))
                    {

                        x++;
                        if (cellList[x, y].GetMat())
                        {
                            return null;
                        }
                        if (cellList[x, y].GetUnitOnCell() != null)
                        {
                            Globals.WriteDebug(tag, "Return Unit is " + cellList[x, y].GetUnitOnCell().GetUnitType(), true);
                            return cellList[x, y].GetUnitOnCell();
                        }
                    }
                    break;
            }
            Globals.WriteDebug(tag, "Returning Null", true);

            return returnUnit;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="d"></param>
        /// <param name="cellList"></param>
        /// <param name="distance"></param>
        /// <param name="searchType">0= Projectiles only, 1= Units only, 2 =Both</param>
        /// <returns></returns>
        public static Entity FindFirstEntityInDistance(int x, int y, Directions d, Cell[,] cellList,int distance,int searchType)
        {
            Entity returnUnit = null;
            String tag = "FindFirstEntity";
            Globals.WriteDebug(tag, "Running FindFirstEntity", true);
            int searches = 0;
            while(searches< distance)
            {
                switch (d)
                {
                    case Directions.DOWN:
                        if (y < cellList.GetLength(1))
                        {
                            y++;
                        }
                        break;
                    case Directions.UP:
                        if (y > 0)
                        {
                            y--;
                        }
                        break;
                    case Directions.LEFT:
                        if (x > 0)
                        {
                            x--;
                        }
                        break;
                    case Directions.RIGHT:
                        if (x < cellList.GetLength(0))
                        {
                            x++;
                        }
                        break;
                }

                //Globals.WriteDebug(tag, "Checking xy"+x+" "+y , true);
                if (cellList[x, y].GetMat())
                {
                    return null;
                }
                if (searchType == 1 || searchType == 2)
                {
                    if (cellList[x, y].GetUnitOnCell() != null)
                    {
                        Globals.WriteDebug(tag, "Return Unit is " + cellList[x, y].GetUnitOnCell().GetUnitType(), true);
                        return cellList[x, y].GetUnitOnCell();
                    }
                }
                if (searchType == 0 || searchType == 2)
                {
                    if (cellList[x, y].GetProjectile() != null)
                    {
                        Globals.WriteDebug(tag, "Return Projectile is " + cellList[x, y].GetProjectile().ToString(), true);
                        return cellList[x, y].GetProjectile();
                    }
                }
                searches++;

            }
            Globals.WriteDebug(tag, "Returning Null", true);
            return returnUnit;

        }


    }
}
