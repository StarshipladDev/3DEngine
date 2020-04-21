using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace DoomCloneV2
{
 /**
  * Globals is a static/constant class used to convey information between listeners and the main form while the application runs.
  * 
  */
    public static class Globals
    {
        public static bool drawGun = true;
        public static bool drawLines = true;
        public static bool drawFill = true;
        public const int maxView = 20;
        public const int cellSize = 20;
        //Draw directin /Drew gun //Turn gun back
        public static bool[] flags = new bool[5];
        public static int[] animations = new int[5];
        public static bool readyToDraw = true;
        public const int MAXFRAMES=8;
        public const int INTERVALTIMEMILISECONDS = 1000;
        public const int MaxPossibleDepth =20;

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
    }
    /// <summary>
    /// Form1 is the Primary form component that runs when openeing the .exe
    /// </summary>
    public partial class Form1 : Form
    {
        Cell[,] cellList = new Cell[Globals.cellSize, Globals.cellSize];
        Player thisPlayer = new Player(4,4,1);
        Directions switcher;
        System.Timers.Timer aTimer;
        
        List<Unit> units = new List<Unit>();
        /// <summary>
        /// This constructor creates a Form. Parameters should be specified in <see cref="Globals"/>
        /// </summary>
        public Form1()
        {
            for(int i=0; i < Globals.animations.Length; i++)
            {
                Globals.animations[i] = 0;
            }
            thisPlayer.playerView = Globals.ResizeImage(thisPlayer.playerView, this.Width, this.Height);
            Random rand = new Random();
            Color ColorRoof = Color.FromArgb(150, rand.Next(255), rand.Next(255), rand.Next(255));
            //Y
            for (int i = 0; i < cellList.GetLength(0); i++)
            {

                //X
                for (int f = 0; f < cellList.GetLength(1); f++)
                {
                    if(f==0 || f == cellList.GetLength(0) - 1 || i==0 || i== cellList.GetLength(0) - 1)
                    {
                        cellList[i, f] = new Cell(true,Color.Gray, Color.Gray, Color.Gray);
                    }
                    else
                    {

                        Color ColorFloor = Color.FromArgb(150, rand.Next(255), rand.Next(155)+100-rand.Next(100), rand.Next(255));
                        cellList[i, f] = new Cell(false, Color.FromArgb(0, 0, 0, 0),ColorFloor,ColorRoof);
                    }
                    
                }
            }
            //Set Random Cells
            for(int i = 0;i < 9; i++){
                int xForBlock = rand.Next(Globals.cellSize-1)+1;
                int yForBLock= rand.Next(Globals.cellSize-1)+1;
                if(i<4 && xForBlock!= thisPlayer.GetX() && yForBLock != thisPlayer.Gety())
                {
                    cellList[xForBlock,yForBLock].setMat(true, Color.Black);
                }
                else
                {
                    if (! cellList[xForBlock, yForBLock].GetisUnitPresent())
                    {
                        units.Add(cellList[xForBlock, yForBLock].createUnit(new Bitmap("Resources/Images/DevilSuit.png"),new Bitmap("Resources/Images/DevilSuit_Dead.png")));
                    }
                }
            }
            cellList[3, 2].setMat(true, Color.Gray);
            //Print Cell
            PrintMap();
            InitializeComponent();
        }
        /// <summary>
        /// TimerCall is the eventlistener that runs every 'tick', that occurs every 'INTERVALTIMEMILISECONDS', specified in <see cref="Globals"/>
        /// </summary>
        /// <param name="sender">Sender is the object that calls this timer listener (Should always be Form1.aTimer)</param>
        /// <param name="e">e is the event arguments of the particular call</param>
        private void TimerCall(object sender, System.Timers.ElapsedEventArgs e)
        {
            Globals.animations[0]++;
            if (Globals.animations[0] == Globals.MAXFRAMES){
                Globals.animations[0] = 0;
            }
            this.Invalidate();
        }
        /// <summary>
        /// Form1_Load is called after <see cref="InitializeComponent"/> is called
        /// </summary>
        /// <param name="sender">Sender is the object that called this method</param>
        /// <param name="e">e is the event arguments of the particular call</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            this.aTimer = new System.Timers.Timer();
            this.aTimer.Elapsed += TimerCall;
            this.aTimer.Interval = Globals.INTERVALTIMEMILISECONDS;
            this.aTimer.Enabled = true;
        }
        /// <summary>
        /// OnPaint is called after every time <see cref="Invalidate()"/> is called.
        /// This method should call any applciation specific drawings by calling <see cref="FormUpdate(bool)"/>
        /// </summary>
        /// <param name="e">e is the arguments of each paint event</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Globals.readyToDraw)
            {
                Globals.readyToDraw = false;
                this.FormUpdate(true);
                base.OnPaint(e);
                Globals.readyToDraw = true;
            }
        }
        /// <summary>
        /// FormUpdate is used to perform the graphical calcualtiosn for where various objects will appear on the screen, and call their
        /// respective drawing functions in order
        /// </summary>
        /// <param name="unitsa">Whether to draw non-cell specfic items (Such as units or player views)</param>
        private void FormUpdate(bool unitsa = false)
        {
            this.Update();
            //Graphics g = Graphics.FromImage(new Bitmap(this.Width,this.Height));
            Graphics g = this.CreateGraphics();
            for (int i = 0; i < units.Count; i++)
            {
                units[i].bottomRight = new Point(-1, -1);
                units[i].topLeft = new Point(-1, -1);
            }
            //GetTargetBlockViaDirection
            bool playerVerticalDirection = false;
            if (thisPlayer.dir == Directions.UP || thisPlayer.dir == Directions.DOWN)
            {
                playerVerticalDirection = true;
            }
            int offset = 0;
            if (playerVerticalDirection)
            {
                offset = thisPlayer.GetX();
            }
            else
            {
                offset = thisPlayer.Gety();
            }
            int maxDepth = GetMaxDepth();
            int loopFromBack = 0;
            while (loopFromBack < maxDepth)
            {
               
                int loopFromLeft = /*targetbase*/0;
                int scanAcrossMax = ((maxDepth - loopFromBack)*2) + 1;
                int halfScan = maxDepth - loopFromBack;
                while (loopFromLeft <scanAcrossMax /*baselineTargetSquare*/)
                {
                    {
                        int screenHeight = this.Height;
                        int screenWidth = this.Width;
                        //Create centreLine)

                        int centreLine=1;
                        //left
                        if (loopFromLeft< halfScan)
                        {
                            centreLine = 0;
                        }
                        //right
                        else if (loopFromLeft > halfScan)
                        {
                            centreLine = 2;
                        }
                        int pickX = 0;
                        int pickY = 0;
                        bool matToLeft = false;
                        if (thisPlayer.dir == Directions.UP)
                        {
                            pickX = (offset - halfScan) + loopFromLeft;
                            pickY = thisPlayer.Gety() + 1 - (maxDepth - loopFromBack);
                        }
                        else if (thisPlayer.dir == Directions.DOWN)
                        {
                            pickX = (offset + halfScan) - loopFromLeft;
                            pickY = (thisPlayer.Gety() - 1) + (maxDepth - loopFromBack);
                        }
                        else if (thisPlayer.dir == Directions.LEFT)
                        {
                            pickY = (offset + halfScan) - loopFromLeft;
                            pickX = (thisPlayer.GetX() + 1) - (maxDepth - loopFromBack);
                        }
                        else if (thisPlayer.dir == Directions.RIGHT)
                        {
                            pickY = (offset - halfScan) + loopFromLeft;
                            pickX = thisPlayer.GetX() - 1 - (maxDepth - loopFromBack);
                        }
                        if(pickX>=0 && pickX<cellList.GetLength(0) && pickY>=0 && pickY < cellList.GetLength(1))
                        {
                            //if (cellList[pickX,pickY].GetisUnitPresent()== unitsa)
                            {
                                bool MatFront = false;
                                if (loopFromBack == maxDepth - 1 && loopFromLeft == 1 && cellList[pickX, pickY].GetMat())
                                {
                                    MatFront = true;
                                }
                                if (thisPlayer.dir == Directions.UP && pickX > 0 && centreLine == 2)
                                {
                                    if /*(cellList[pickX - 1, pickY].GetisUnitPresent() || */(cellList[pickX - 1, pickY].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                else if (thisPlayer.dir == Directions.DOWN && pickX < cellList.GetLength(0) - 1 && centreLine == 2)
                                {
                                    if /*(cellList[pickX + 1, pickY].GetisUnitPresent() ||*/(cellList[pickX + 1, pickY].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                else if (thisPlayer.dir == Directions.LEFT && pickY > 0 && centreLine == 2)
                                {
                                    if /*(cellList[pickX, pickY + 1].GetisUnitPresent() || */(cellList[pickX, pickY + 1].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                else if (thisPlayer.dir == Directions.RIGHT && pickY < cellList.GetLength(0) - 1 && centreLine == 2)
                                {
                                    if /*(cellList[pickX, pickY - 1].GetisUnitPresent() || */(cellList[pickX, pickY - 1].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                cellList[pickX, pickY].Draw(g, centreLine, loopFromBack, loopFromLeft, maxDepth, screenWidth, screenHeight, Globals.drawLines, matToLeft, MatFront);
                            }
                           
                        }

                    }
                    loopFromLeft++;
                    
                }
                loopFromBack++;
            }
            if (unitsa == false)
            {
                g.Dispose();
                FormUpdate(true);
            }
            else
            {
                if (Globals.flags[0])
                {
                    Globals.flags[0] = false;
                    g.DrawString("DrawingMap! ", new Font("Arial", 16), new SolidBrush(Color.Red), this.Width / 2, this.Height / 2);
                    g.DrawString("Your direction is : " + thisPlayer.dir, new Font("Arial", 16), new SolidBrush(Color.Red), this.Width / 2, (this.Height / 2) + 20);


                }
                //DrawGunShooting
                if (Globals.flags[1] == true)
                {
                    RefreshPlayerView(this.thisPlayer.GetPlayerGunShoot());
                    Globals.flags[1] = false;
                    Globals.flags[2] = true;
                }
                //Set gun back to normal
                else if (Globals.flags[2] == true)
                {
                    RefreshPlayerView(this.thisPlayer.GetPlayerGun());
                    Globals.flags[2] = false;
                }
                if (Globals.drawGun)
                {
                    g.DrawImage(thisPlayer.playerView, new Point(this.Width - thisPlayer.playerView.Width - 10, this.Height - thisPlayer.playerView.Height - 40));
                }
                g.Dispose();

            }
           
        }
        /// <summary>
        /// RefreshPlayerView takes a given image and sets the form's playerview as tat image resized to the application size
        /// </summary>
        /// <param name="img">The bitmap iamge to be set as playerview</param>
        public void RefreshPlayerView(Bitmap img)
        {
            this.thisPlayer.playerView = Globals.ResizeImage(img, this.Width / 2, this.Height / 2);

        }
        /// <summary>
        /// GetMaxDepth is used to callculate how many 'cells' away from the player, based on their current view,
        /// they can see in any direction. This is the furtherest away the program will draw from.
        /// </summary>
        /// <returns>GetMaxDepth returns the distance the palyer can see away from themselves, in # of cells</returns>
        public int GetMaxDepth()
        {
            int picker = 1;
            if(thisPlayer.dir==Directions.UP )
            {
                picker= thisPlayer.Gety() + 1;
            }
            else if (thisPlayer.dir == Directions.DOWN)
            {
                picker = cellList.GetLength(1)- thisPlayer.Gety() ;
            }
            else if (thisPlayer.dir == Directions.RIGHT)
            {
                picker = cellList.GetLength(0)-thisPlayer.GetX();
            }
            else if (thisPlayer.dir == Directions.LEFT)
            {
                picker = thisPlayer.GetX() + 1;
            }
            if (picker > Globals.MaxPossibleDepth)
                {
                    return Globals.MaxPossibleDepth;
                }
                else
                {
                    return picker;
                }
        }
        /// <summary>
        /// ChangeX trys to change the player's x co-ordinate. This method deals with invalid movement
        /// </summary>
        /// <param name="right">States whether to move the palyer right. If false, palyer will mvoe left</param>
        private void ChangeX(bool right)
        {
            int x = thisPlayer.GetX();
            int y = thisPlayer.Gety();
            if (right)
            {
                if (x < cellList.GetLength(1) - 1)
                {
                    if (!cellList[x + 1, y].GetMat())
                    {
                        this.thisPlayer.SetX(this.thisPlayer.GetX() + 1);
                    }
                }
            }
            else
            {
                if (x > 0)
                {
                    if (!cellList[x - 1, y].GetMat())
                    {


                        this.thisPlayer.SetX(this.thisPlayer.GetX() - 1);
                    }
                }
            }
        }
        /// <summary>
        /// ChangeY trys to change the player's y co-ordinate. This method deals with invalid movement
        /// </summary>
        /// <param name="up">States whether to move the palyer up. If false, player will move down</param>
        private void ChangeY (bool up)
        {
            int x = thisPlayer.GetX();
            int y = thisPlayer.Gety();
            if (up)
            {
                if (y < cellList.GetLength(1) - 1)
                {
                    if (!cellList[x, y + 1].GetMat())
                    {

                        this.thisPlayer.SetY(this.thisPlayer.Gety() + 1);
                    }
                }
            }
            else
            {
                if (y > 0)
                {
                    if (!cellList[x, y - 1].GetMat())
                    {

                        this.thisPlayer.SetY(this.thisPlayer.Gety() - 1);
                    }
                }
            }
        }
        /// <summary>
        /// OnKeyUp handles key presses done while a <see cref="Form1"/> isntance is the active window
        /// </summary>
        /// <param name="sender">The object that sent this call </param>
        /// <param name="e">The arguments of this particular key press</param>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            Directions playersdir = thisPlayer.dir;
            if (e.KeyCode == Keys.Space || e.KeyCode==Keys.D1)
            {
                this.Text = "DOOMCLoneV2";
                Globals.drawLines = !Globals.drawLines;
                
                
            }
            if (e.KeyCode == Keys.D2)
            {
                this.Text = "DOOMCLoneV2";
                Globals.drawFill = !Globals.drawFill;


            }
            if (e.KeyCode == Keys.D3)
            {
                this.Text = "DOOMCLoneV2";
                Globals.drawGun = !Globals.drawGun;


            }
            if (e.KeyCode == Keys.S)
            {
                switch (playersdir)
                {
                    case Directions.UP:
                        ChangeY(true);
                        break;
                    case Directions.DOWN:
                        ChangeY(false);
                        break;
                    case Directions.LEFT:
                        ChangeX(true);
                        break;
                    case Directions.RIGHT:
                        ChangeX(true);
                        break;
                }


            }
            if (e.KeyCode == Keys.W)
            {
                switch (playersdir)
                {
                    case Directions.UP:
                        ChangeY(false);
                        break;
                    case Directions.DOWN:
                        ChangeY(true);
                        break;
                    case Directions.LEFT:
                        ChangeX(false);
                        break;
                    case Directions.RIGHT:
                        ChangeX(false);
                        break;
                }


            }
            if (e.KeyCode == Keys.D)
            {
                switch (playersdir)
                {
                    case Directions.UP:
                        ChangeX(true);
                        break;
                    case Directions.DOWN:
                        ChangeX(false);
                        break;
                    case Directions.LEFT:
                        ChangeY(false);
                        break;
                    case Directions.RIGHT:
                        ChangeY(true);
                        break;
                }

            }
            if (e.KeyCode == Keys.A)
            {
                switch (playersdir)
                {
                    case Directions.UP:
                        ChangeX(false);
                        break;
                    case Directions.DOWN:
                        ChangeX(true);
                        break;
                    case Directions.LEFT:
                        ChangeY(true);
                        break;
                    case Directions.RIGHT:
                        ChangeY(false);
                        break;
                }

            }
            if (e.KeyCode == Keys.P)
            {
                this.thisPlayer.RefreshGun();
                RefreshPlayerView(this.thisPlayer.playerView);

            }
            if (e.KeyCode == Keys.U)
            {
                PrintMap();
                Globals.flags[0] = true;

            }
            this.Update();
            this.Invalidate();
        }
        /// <summary>
        /// A debug function used to print the state of the Form1 <see cref="Form1.cellList"/> at call
        /// </summary>
        private void PrintMap()
        {
            //Y
            for (int f = 0; f < cellList.GetLength(0); f++)
            {
                Debug.Write("\n");
                //X
                for (int i = 0; i < cellList.GetLength(1); i++)
                {
                    if (cellList[i,f].GetMat())
                    {
                        Debug.Write("[*]");
                    }
                    else if (thisPlayer.GetX() == i && thisPlayer.Gety() == f)
                    {
                        Debug.Write(" ^^");
                    }
                    else if (cellList[i, f].GetisUnitPresent())
                    {
                        Debug.Write(" <3");
                    }
                    else
                    {
                        Debug.Write(" []");
                    }

                }
            }
        }
        /// <summary>
        /// Handles the logic of a unit being clicked
        /// </summary>
        /// <param name="unitClicked"></param>
        private void ClickHandler(Unit unitClicked)
        {
            //Set gun as true and redraw
            Globals.flags[1] = true;
            thisPlayer.GetPlayerGunSound().Play();
            if (unitClicked.alive == true)
            {
                unitClicked.DealDamage(thisPlayer.GetGunDamage());
            }
            this.Invalidate();
        }
        /// <summary>
        /// ClickedOn is a function used to check if a unit has been clicked on. It then passes the unit to <see cref="ClickHandler(Unit)"/>
        /// </summary>
        /// <param name="unitClicked">unitClicked is the unit beign tested for a click interaction</param>
        /// <param name="x">x is the x co-ordinate of the mouse click as passed by <see cref="MouseClicker(object, MouseEventArgs)"/></param>
        /// <param name="y">y is the y co-ordinate of the mouse click as passed by <see cref="MouseClicker(object, MouseEventArgs)"/></param>
        /// <returns></returns>
        private int ClickedOn(Unit unitClicked,int x ,int y)
        {
            if (x > unitClicked.topLeft.X && x < unitClicked.bottomRight.X)
            {
                if (y < unitClicked.bottomRight.Y && y > unitClicked.topLeft.Y)
                {
                    ClickHandler(unitClicked);

                    return 1;
                }
            }
            return 0;
        }
        /// <summary>
        /// MouseClicker is an eventlistener class used to create event on mouse click occur while <see cref="Form1"/> is the active window
        /// </summary>
        /// <param name="sender">The object that sends the MouseEvent call</param>
        /// <param name="e">The arguments of that particular MouseClick</param>
        private void MouseClicker(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            for(int i=0; i < units.Count; i++)
            {
                if (ClickedOn(units[i], e.X, e.Y) == 1)
                {
                    i = units.Count;
                }
            }
        }
        /// <summary>
        /// MouseMover is an eventlistener class used to create event on mouse move occur while <see cref="Form1"/> is the active window
        /// </summary>
        /// <param name="sender">The object that sends the MouseEvent call</param>
        /// <param name="e">The arguments of that particular MouseClick</param>
        private void MouseMover(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.X > this.Width - 30)
            {
                if (switcher != Directions.RIGHT)
                {
                    switcher = Directions.RIGHT;
                    Thread.Sleep(500);
                }
                else if (switcher == Directions.RIGHT)
                {

                    thisPlayer.ChangeDirection("Right");
                    Debug.WriteLine("Changing Direction Right");
                    switcher = Directions.NULL;
                    this.Invalidate();
                }
            }
            else if (e.X <30)
            {
                if (switcher!= Directions.LEFT)
                {
                    switcher = Directions.LEFT;
                    Thread.Sleep(500);
                }
                else if (switcher == Directions.LEFT)
                {

                    thisPlayer.ChangeDirection("Left");
                    Debug.WriteLine("Changing Direction Left");
                    switcher = Directions.NULL;
                    this.Invalidate();
                }
            }
            
        }
    }/// <summary>
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
        /// <summary>
        /// setUpUnit is a constructor but like later
        /// </summary>
        /// <param name="unitImage">Provides a bitmap image to be read whenever the unit is 'alive'</param>
        /// <param name="deadUnitImage">Provides a bitmap image to be read/drawn wheenever the unit is 'dead'</param>
        public void setUpUnit(Bitmap unitImage,Bitmap deadUnitImage)
        {
            this.unitImage = unitImage;
            this.deadUnitImage = deadUnitImage;
            this.returnImage = this.unitImage;
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
            this.unitImage=img;
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
            }
        }
    }
    public class Gun
    {
        /*
         * muzzle_Flare -> Flare_[bullet Type]_[DamageRate]_[Name]
receiver -> Receiver_[Camo Code]_[DamageRate]_[Name]
ejection-> Ejection_[bullet Type]_[DamageRate]_[Name]
front_Body-> Front_Body_[Camo Code]_[bullet Type]_[Name]
barrel-> Barrel_[Camo Code]_[DamageRate]_[Name]
magazine-> Magazine_[bullet Type]_[Ammo_Cap]_[Name]
grip-> Grip_[Camo Code]_[Grip_Type]_[Name]
arm-> Arm_[Uniform Code]_[Armour_Code]_[Name]
hand-> Hand_[Race Code]_[Style Code]_[Name]
rail-> Rail_[Bullet Type]_[DamageRate]_[Name]
sight-> Receiver_[Camo Code]_[Sight_Code]_[Name]
         * */
        int damage;
        String camo_Code; // 000-Universal // 001- Tiger Strike // 002-Pinky
        String bullet_Type; // 000-Universal // 001 - Bullets //002 Rail
        String damage_Rate; // 000-Universal // 001 -Medium //002-Heavy
        String ammo_Cap; // 000 20-30 Rounds // 001 - 60-100 // 002 -1-5
        String uniform_Code; // 000 - Universal // 001 - Green Camo // 002 - Navy 
        String armour_Code; // 000-No Armour
        String race_Code;// 000-Human
        String style_Code;// 000- Ungloved White
        String sight_Code;// 000 - Irons/No scope // 001 -Red dot
        String grip_Type; // 000 - Medium Grip // 001 -Bipod // 002- L        Bitmap muzzle_Flare;
        Bitmap receiver;
        Bitmap ejection;
        Bitmap front_Body;
        Bitmap barrel;
        Bitmap magazine;
        Bitmap grip;
        Bitmap arm;
        Bitmap hand;
        Bitmap token;
        Bitmap rail;
        Bitmap sight;
        Bitmap muzzle_Flare;
        Bitmap picture;
        Bitmap pictureShoot;
        System.Media.SoundPlayer player;
        public Gun(String camo_Code, String bullet_Type, String damage_Rate, String ammo_Cap, String uniform_Code, String armour_Code, String race_Code, String style_Code, String sight_Code,String grip_Type,int damage)
        {
            this.damage=damage;
            this.camo_Code=camo_Code; // 000-Universal // 001- Tiger Strike // 002-Pinky
            this.bullet_Type=bullet_Type; // 000-Universal // 001 - Bullets //002 Rail
            this.damage_Rate=damage_Rate; // 000-Universal // 001 -Medium //002-Heavy
            this.ammo_Cap=ammo_Cap; // 000 20-30 Rounds // 001 - 60-100 // 002 -1-5
            this.uniform_Code=uniform_Code; // 000 - Universal // 001 - Green Camo // 002 - Navy 
            this.armour_Code=armour_Code; // 000-No Armour
            this.race_Code=race_Code;// 000-Human
            this.style_Code=style_Code;// 000- Ungloved White
            this.sight_Code=sight_Code;// 000 - Irons/No scope // 001 -Red dot
            this.grip_Type=grip_Type; 
            this.buildGun();
        }
        private void buildGun()
        {
            setComponent("Barrel");
            setComponent("Muzzle_Flare");
            setComponent("Hand");
            setComponent("Grip");
            setComponent("Ejection");
            setComponent("Rail");
            setComponent("Sight");
            setComponent("Front_Body");
            setComponent("Receiver");

            setComponent("Token");
            setComponent("Arm");
            setComponent("Magazine");
            picture = new Bitmap(arm.Width,arm.Height, PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(picture);
            g.DrawImage(this.barrel, new Point(0,0));
            g.DrawImage(this.hand, new Point(0, 0));
            g.DrawImage(this.grip, new Point(0, 0));
            g.DrawImage(this.rail, new Point(0, 0));
            g.DrawImage(this.sight, new Point(0, 0));
            g.DrawImage(this.front_Body, new Point(0, 0));
            g.DrawImage(this.receiver, new Point(0, 0));

            g.DrawImage(this.ejection, new Point(0, 0));
            g.DrawImage(this.token, new Point(0, 0));
            g.DrawImage(this.arm, new Point(0, 0));
            g.DrawImage(this.magazine, new Point(0, 0));
            g.Dispose();
            pictureShoot = new Bitmap(arm.Width, arm.Height, PixelFormat.Format32bppPArgb);
            g = Graphics.FromImage(pictureShoot);
            g.DrawImage(this.barrel, new Point(0, 0));
            g.DrawImage(this.muzzle_Flare, new Point(0, 0));
            g.DrawImage(this.hand, new Point(0, 0));
            g.DrawImage(this.grip, new Point(0, 0));
            g.DrawImage(this.rail, new Point(0, 0));
            g.DrawImage(this.sight, new Point(0, 0));
            g.DrawImage(this.front_Body, new Point(0, 0));
            g.DrawImage(this.receiver, new Point(0, 0));

            g.DrawImage(this.ejection, new Point(0, 0));
            g.DrawImage(this.token, new Point(0, 0));
            g.DrawImage(this.arm, new Point(0, 0));
            g.DrawImage(this.magazine, new Point(0, 0));
            g.RotateTransform(20);
            g.Dispose();
            player = new System.Media.SoundPlayer("Resources/Sound/Bang_"+this.bullet_Type+"_"+this.damage_Rate+".wav");
            Debug.WriteLine("Resources/Sound/Bang_" + this.bullet_Type + "_" + this.damage_Rate + ".wav");
        }

        public System.Media.SoundPlayer GetSound()
        {
            return this.player;
        }
        public Bitmap GetImage()
        {
            return this.picture;
        }
        public int GetDamage()
        {
            return this.damage;
        }
        public Bitmap GetImageShoot()
        {
            return this.pictureShoot;
        }
        private void setComponent(String gunPart)
        {
            Random rand = new Random();
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;

            String imgPath = RunningPath + "Resources\\Images\\Gun_Parts\\" + gunPart + "\\";
            String[] possibleImgs = System.IO.Directory.GetFiles(imgPath, "*.png");
            List<String> relevantImgs = new List<String>();
            for(int i=0; i < possibleImgs.Count(); i++)
            {
                String fileName = possibleImgs[i].Substring(imgPath.Length, possibleImgs[i].Length - imgPath.Length);
                int firstVarStart = gunPart.Length+1;
                int secondVarStart = gunPart.Length + 5;
                switch (gunPart)
                {
                    case "Barrel":
                        if (fileName.Substring(firstVarStart, 3).Equals("000")|| fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Muzzle_Flare":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Receiver":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Ejection":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Front_Body":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.bullet_Type))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Magazine":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.ammo_Cap))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Grip":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.grip_Type))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Arm":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.uniform_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.armour_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Hand":
                        if (fileName.Substring(firstVarStart, 3).Equals(this.race_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.style_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Rail":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Sight":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.sight_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Token":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.grip_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.style_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            int numberPicked = rand.Next(relevantImgs.Count());
            switch (gunPart)
            {
                case "Barrel":
                    this.barrel = new Bitmap(relevantImgs[numberPicked]);
                    
                    break;
                case "Muzzle_Flare":
                    this.muzzle_Flare = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Receiver":
                    this.receiver = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Ejection":
                    this.ejection = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Front_Body":
                    this.front_Body = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Magazine":
                    this.magazine = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Grip":
                    this.grip = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Arm":
                    this.arm = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Hand":
                    this.hand = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Rail":
                    this.rail = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Sight":
                    this.sight = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Token":
                    this.token = new Bitmap(relevantImgs[numberPicked]);

                    break;
                default:
                    Debug.WriteLine("Unkown Switch "+gunPart);
                    break;
            }


        }
    }
    public class Cell
    {

        Color drawColor;
        Color floorColor;
        Color roofColor;
        bool mat = false;
        bool isUnitOnCell = false;
        Unit unitOnCell = null;
        public Cell(bool mat, Color drawColor,Color floorColor, Color roofColor)
        {
            this.mat = mat;
            this.drawColor = drawColor;
            this.floorColor = floorColor;
            this.roofColor = roofColor;
        }
        public Unit createUnit(Bitmap unitPic, Bitmap unitPicDead)
        {
            this.isUnitOnCell = true;
            this.unitOnCell = new Unit();
            this.unitOnCell.setUpUnit(unitPic,unitPicDead);
            return this.unitOnCell;
        }
        public bool GetisUnitPresent()
        {
            return this.isUnitOnCell;
        }
        public Unit GetUnitOnCell()
        {
            if (GetisUnitPresent())
            {
                return this.unitOnCell;
            }
            else 
            { 
                return null; 
            }
        }
        public bool GetMat()
        {
            return this.mat;
        }
        public void setMat(bool mat,Color color)
        {
            this.mat = mat;
            this.drawColor = color;
        }
        
        public void Draw(Graphics g, int centreLine, int loopFromBack, int loopFromLeft, int maxDepth, int screenLength, int screenHeight, bool drawLines, bool drawLeftMat,bool playerOn=false)
        {

            Point[] points;
            int drawLength = screenLength/(((maxDepth-loopFromBack)*2)-1);
            int drawHeight = screenHeight / (((maxDepth - loopFromBack) * 2) - 1);
            int drawLengthPrior = screenLength / (((maxDepth - loopFromBack+1) * 2) - 1);
            int drawHeightPrior = screenHeight / (((maxDepth - loopFromBack+1) * 2) - 1);
            int[] topLeft = new int[] { (drawLength * (loopFromLeft-1)), screenHeight / 2 - (drawHeight / 2) };
            int[] topRight = new int[] { (drawLength * (loopFromLeft-1)) +drawLength, screenHeight / 2 - (drawHeight / 2) };
            int[] bottomLeft = new int[] { drawLength * (loopFromLeft-1), screenHeight / 2 - (drawHeight / 2) + drawHeight}; 
            int[] bottomRight = new int[] { drawLength * (loopFromLeft-1) + drawLength, screenHeight / 2 - (drawHeight / 2) + drawHeight };
            int PriorLoop = loopFromLeft;
            int[] topLeftPrior = new int[] { drawLengthPrior * (PriorLoop), screenHeight / 2 - (drawHeightPrior / 2) };
            int[] bottomRightPrior = new int[] { drawLengthPrior * (PriorLoop) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2) + drawHeightPrior };
            int[] topRightPrior = new int[] {(drawLengthPrior * (PriorLoop)) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2) }; 
            int[] bottomLeftPrior = new int[] {drawLengthPrior * (PriorLoop), screenHeight / 2 - (drawHeightPrior / 2) + drawHeightPrior };

            if (Globals.drawFill)
            {
                //IfBackRow
                if (loopFromBack == 0)
                {
                    Color drawing = this.drawColor;
                    if (this.mat == false)
                    {
                        drawing = Color.Gray;
                    }
                    g.FillRectangle(new SolidBrush(drawing), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2), drawLength, drawHeight);
                }
                if (loopFromBack == 1 && maxDepth == Globals.maxView)
                {
                    Color drawing = this.drawColor;
                    if (this.mat == false)
                    {
                        drawing = Color.FromArgb(75, 0, 0, 0);
                    }
                    g.FillRectangle(new SolidBrush(drawing), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2), drawLength, drawHeight);
                }
                if (loopFromBack > 0)
                {

                    if (drawLeftMat == false)
                    {
                        //DrawFloor
                        points = new Point[] { new Point(bottomLeft[0], bottomLeft[1]), new Point(bottomLeftPrior[0], bottomLeftPrior[1]), new Point(bottomRightPrior[0], bottomRightPrior[1]), new Point(bottomRight[0], bottomRight[1]) };
                        g.FillPolygon(new SolidBrush(this.floorColor), points);
                        //DrawRoof
                        points = new Point[] { new Point(topLeft[0], topLeft[1]), new Point(topRight[0], topRight[1]), new Point(topRightPrior[0], topRightPrior[1]), new Point(topLeftPrior[0], topLeftPrior[1]) };
                        g.FillPolygon(new SolidBrush(this.roofColor), points);
                    }
                    else
                    {
                        //DrawFloor
                        points = new Point[] { new Point(bottomLeft[0], bottomLeft[1]), new Point(bottomLeft[0], bottomLeftPrior[1]), new Point(bottomRightPrior[0], bottomRightPrior[1]), new Point(bottomRight[0], bottomRight[1]) };
                        g.FillPolygon(new SolidBrush(this.floorColor), points);
                        //DrawRoof
                        points = new Point[] { new Point(topLeft[0], topLeft[1]), new Point(topRight[0], topRight[1]), new Point(topRightPrior[0], topRightPrior[1]), new Point(topLeft[0], topLeftPrior[1]) };
                        g.FillPolygon(new SolidBrush(this.roofColor), points);
                    }
                    if (this.mat)
                    {
                        //DrawToRight
                        if (centreLine == 0)
                        {
                            points = new Point[] { new Point((drawLength * (loopFromLeft - 1)) + drawLength, screenHeight / 2 - (drawHeight / 2)), new Point((drawLengthPrior * (loopFromLeft)) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2)), new Point(drawLengthPrior * (loopFromLeft) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2) + drawHeightPrior), new Point(drawLength * (loopFromLeft - 1) + drawLength, screenHeight / 2 - (drawHeight / 2) + drawHeight) };
                            g.FillPolygon(new SolidBrush(Color.Red), points);
                        }
                        //DrawToLeft
                        else if (centreLine == 2 && !drawLeftMat)
                        {
                            points = new Point[] { new Point(topLeft[0], topLeft[1]), new Point(bottomLeft[0], bottomLeft[1]), new Point(bottomLeftPrior[0], bottomLeftPrior[1]), new Point(topLeftPrior[0], topLeftPrior[1]) };
                            g.FillPolygon(new SolidBrush(Color.Blue), points);
                        }
                        if (!playerOn)
                        {
                            g.FillRectangle(new SolidBrush(this.drawColor), drawLength * (loopFromLeft - 1), screenHeight / 2 - (drawHeight / 2), drawLength, drawHeight);

                        }


                    }
                    else
                    {
                        if (isUnitOnCell)
                        {
                            Bitmap drawImage = unitOnCell.getUnitImage();
                            //drawImage=ResizeImage(drawImage,drawImage.Width / (((maxDepth - loopFromBack) * 2) -3 ), drawImage.Height / (((maxDepth - loopFromBack) * 2) -3));
                            drawImage = Globals.ResizeImage(drawImage, drawLength, drawHeight);
                            g.DrawImage(drawImage, new Point(topLeft[0], topLeft[1]));
                            unitOnCell.topLeft = new Point(topLeft[0], topLeft[1]);
                            unitOnCell.bottomRight = new Point(topLeft[0] + drawImage.Width, topLeft[1] + drawImage.Height);
                            Random rand = new Random();
                            if (rand.Next(5) == 3 && unitOnCell.alive)
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
                    }


                }
            }
            
            if (drawLines )
            {
                //Draw outline
                g.DrawRectangle(new Pen(Color.Black), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2), drawLength, drawHeight);
                if(loopFromBack > 0)
                {
                    //Draw connectors
                    //top left
                    g.DrawLine(new Pen(Color.Black), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2),/*draw to */ drawLengthPrior * (loopFromLeft + 1), screenHeight / 2 - (drawHeightPrior / 2));
                    //top right
                    g.DrawLine(new Pen(Color.Black), (drawLength * (loopFromLeft)) + drawLength, screenHeight / 2 - (drawHeight / 2),/*draw to */ (drawLengthPrior * (loopFromLeft + 1)) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2));
                    //bottom left
                    g.DrawLine(new Pen(Color.Black), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2) + drawHeight,/*draw to */ drawLengthPrior * (loopFromLeft + 1), screenHeight / 2 - (drawHeightPrior / 2) + drawHeightPrior);
                    //bottom right
                    g.DrawLine(new Pen(Color.Black),bottomRight[0],bottomRight[1],bottomLeft[0],bottomLeft[1]);

                }




            } 
        }
    }
    public enum Directions
    {
        UP,DOWN,LEFT,RIGHT,NULL
    }
    
    public class Player
    {
        private int xPos { get; set; }
        private int yPos { get; set; }
        private Gun playerGun;
        public Directions dir = Directions.UP;
        public Bitmap playerView;
      
        public Player(int x, int y,int gunType)
        {
            CreateGun();
            this.yPos = y;
            this.xPos = x;
            
        }
        private void CreateGun()
        {
            Random rand = new Random();
            String Camo = "00" + (rand.Next(3) + 2);
            String Damage = "00" + (rand.Next(2) + 1);
            String Sight = "00" + (rand.Next(2));
            String Grip = "00" + (rand.Next(3) );
            String Ammo = "00" + (rand.Next(2));
            int damage = rand.Next(21);
            Debug.WriteLine("New Gun with Camo/Damage: " + Camo + " \\ " + Damage);
            this.playerGun = new Gun(Camo, "001",Damage,Ammo, "001", "000", "000", "000",Sight,Grip,damage);
            this.playerView = GetPlayerGun();
        }
        public int GetGunDamage()
        {
            return this.playerGun.GetDamage();
        }
        public System.Media.SoundPlayer GetPlayerGunSOund()
        {
            return this.playerGun.GetSound();
        }
        public void RefreshGun()
        {
            this.CreateGun();
        }
        public Bitmap GetPlayerGun()
        {
            return playerGun.GetImage();
        }
        public Bitmap GetPlayerGunShoot()
        {
            return playerGun.GetImageShoot();
        }
        public System.Media.SoundPlayer GetPlayerGunSound()
        {
            return playerGun.GetSound();
        }
        public int GetX()
        {
            return xPos;
        }
        public int Gety()
        {
            return yPos;
        }
        public int SetY(int i)
        {
            this.yPos = i;
            return yPos;
        }
        public int SetX(int i)
        { 
            this.xPos = i;
            return xPos;
        }
        public void ChangeDirection(String direction)
        {
            if(direction.Equals( "Right"))
            {
                switch (this.dir)
                {
                    case Directions.DOWN:
                        this.dir = Directions.LEFT;
                        break;
                    case Directions.RIGHT:
                        this.dir = Directions.DOWN;
                        break;
                    case Directions.UP:
                        this.dir = Directions.RIGHT;
                        break;
                    case Directions.LEFT:
                        this.dir = Directions.UP;
                        break;
                }
            }
            else
            {
                switch (this.dir)
                {
                    case Directions.DOWN:
                        this.dir = Directions.RIGHT;
                        break;
                    case Directions.RIGHT:
                        this.dir = Directions.UP;
                        break;
                    case Directions.UP:
                        this.dir = Directions.LEFT;
                        break;
                    case Directions.LEFT:
                        this.dir = Directions.DOWN;
                        break;
                }

            }
        }
    }
}
