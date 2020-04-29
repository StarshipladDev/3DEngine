using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;

namespace DoomCloneV2
{
    /// <summary>
    /// Form1 is the Primary form component that runs when openeing the .exe
    /// </summary>
    public partial class Form1 : Form
    {
        Cell[,] cellList = new Cell[Globals.cellSize, Globals.cellSize];
        Player thisPlayer = new Player(4,4,1);
        Directions switcher;
        System.Timers.Timer aTimer;
        Server server = null;
        Client thisClient = null;
        List<Unit> units = new List<Unit>();
        /// <summary>
        /// This constructor creates a Form. Parameters should be specified in <see cref="Globals"/>
        /// </summary>
        public Form1()
        {
            ReadConfig();
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer,true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            for (int i=0; i < Globals.animations.Length; i++)
            {
                Globals.animations[i] = 0;
            }
            for (int i = 0; i < Globals.flags.Length; i++)
            {
                Globals.flags[i] = false;
            }
            thisPlayer.playerView = Globals.ResizeImage(thisPlayer.playerView, this.Width, this.Height);
            Random rand = new Random();
            Color ColorRoof = Color.FromArgb(150, 255,200,0);
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

                        Color ColorFloor = Color.FromArgb(50, rand.Next(50), 255, rand.Next(50));
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
                        units.Add(cellList[xForBlock, yForBLock].createUnit(new Bitmap("Resources/Images/Enemy/Devil/Devil_Idle.png"),new Bitmap("Resources/Images/Enemy/Devil/Devil_Death.png")));
                    }
                }
            }
            cellList[3, 2].setMat(true, Color.Gray);
            //Print Cell
            PrintMap();
            InitializeComponent();
        }
        public static void ReadConfig()
        {
            try
            {

                FileStream configFile = File.OpenRead("config.xml");
                long configFileContentsLength = configFile.Length;
                Byte[] configFileContentsByte = new Byte[configFileContentsLength];
                configFile.Read(configFileContentsByte, 0, (int)configFileContentsLength);
                String configFileContents = Encoding.ASCII.GetString(configFileContentsByte);
                Debug.WriteLine(configFileContents);
                configFile.Close();
                XmlDocument configXml = new XmlDocument();
                configXml.InnerXml = configFileContents;
                Debug.WriteLine("Port is :"+configXml.SelectSingleNode("Config/Port").InnerText);
                Debug.WriteLine("Address is :" + configXml.SelectSingleNode("Config/ServerAddress").InnerText);
                Globals.port = configXml.SelectSingleNode("Config/Port").InnerText;
                Globals.Address = configXml.SelectSingleNode("Config/ServerAddress").InnerText;

            }
            catch(Exception e)
            {
                Debug.WriteLine("Error getting Config");
            }
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
                base.OnPaint(e);
                this.FormUpdate(true);
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
            //this.Update();
            Bitmap n = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(n);
            //Graphics g = this.CreateGraphics();
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
                            pickY =( thisPlayer.Gety() + 1 )- (maxDepth - loopFromBack);
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
                            pickX = (thisPlayer.GetX() - 1 )+ (maxDepth - loopFromBack);
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
                    
                    g.DrawString("DrawingMap! ", new Font("Arial", 16), new SolidBrush(Color.Red), this.Width / 2, this.Height / 2);
                    g.DrawString("Your direction is : " + thisPlayer.dir, new Font("Arial", 16), new SolidBrush(Color.Red), this.Width / 2, (this.Height / 2) + 20);
                    g.DrawString("Your MaxDepth is : " + maxDepth, new Font("Arial", 16), new SolidBrush(Color.Red), this.Width / 2, (this.Height / 2) + 40);


                }
                if (Globals.flags[3])
                {
                    g.DrawString("Server Started!", new Font("Arial", 16), new SolidBrush(Color.Red), this.Width / 2, (this.Height / 2) + 40);

                }
                if (Globals.flags[4])
                {
                    g.DrawString("Client Started!", new Font("Arial", 16), new SolidBrush(Color.Red), this.Width / 2, (this.Height / 2) + 40);

                }
                if (Globals.flags[5])
                {
                    g.DrawString("Messaged Received From Client: "+Globals.Message, new Font("Arial", 16), new SolidBrush(Color.Red),0, (this.Height / 2) + 60);

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
                Graphics z = this.CreateGraphics();
                z.DrawImage(n, 0, 0, n.Width, n.Height);
                z.Dispose();

                //n.Save("Image"+Globals.animations[1]+".png");
                //Globals.animations[1]++;

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
            else
            {
                picker = thisPlayer.GetX() + 1;
            }
            if (picker >= Globals.MaxPossibleDepth)
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
                if (x > 1)
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
                if (y > 1)
                {
                    if (!cellList[x, y - 1].GetMat())
                    {

                        this.thisPlayer.SetY(this.thisPlayer.Gety() - 1);
                    }
                }
                
            }
            else
            {
                if (y < cellList.GetLength(1) - 1)
                {
                    if (!cellList[x, y + 1].GetMat())
                    {

                        this.thisPlayer.SetY(this.thisPlayer.Gety() + 1);
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
                        ChangeY(false);
                        break;
                    case Directions.DOWN:
                        ChangeY(true);
                        break;
                    case Directions.LEFT:
                        ChangeX(true);
                        break;
                    case Directions.RIGHT:
                        ChangeX(false);
                        break;
                }


            }
            if (e.KeyCode == Keys.W)
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
                        ChangeX(false);
                        break;
                    case Directions.RIGHT:
                        ChangeX(true);
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
                        ChangeY(true);
                        break;
                    case Directions.RIGHT:
                        ChangeY(false);
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
                        ChangeY(false);
                        break;
                    case Directions.RIGHT:
                        ChangeY(true);
                        break;
                }

            }
            if (e.KeyCode == Keys.Q)
            {
                this.thisPlayer.RefreshGun();
                RefreshPlayerView(this.thisPlayer.playerView);

            }
            if (e.KeyCode == Keys.U)
            {
                Globals.flags[0] = !Globals.flags[0];
                Globals.flags[3] = false;
                Globals.flags[4] = false;
                PrintMap();

            }
            if (e.KeyCode == Keys.O)
            {
                Thread f = new Thread(ThreadFunctions.ThreadRunnerServer);
                f.Start();
                Globals.flags[0] = false;
                Globals.flags[3] = true;
                Globals.flags[4] = false;

            }
            if (e.KeyCode == Keys.P)
            {
                Globals.flags[0] = false;
                Globals.flags[3] = false;
                Globals.flags[4] = true;
                Thread f = new Thread(ThreadFunctions.ClientThread);
                object args;
                this.thisClient = new Client(Globals.port, Globals.Address,"The Client");
                args = this.thisClient;
                f.Start(args);
            }
            if (e.KeyCode == Keys.L)
            {
                Client c = new Client(Globals.port, "localhost", "New Client Thread");
                Thread f = new Thread(ThreadFunctions.Write);
                f.Start(c);
            }
            if (e.KeyCode == Keys.K)
            {
                thisClient.Write("Hey There From This Client at "+DateTime.Now);
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
            //Set gun as true and redraw
            Globals.flags[1] = true;
            thisPlayer.GetPlayerGunSound().Play();
            for (int i=0; i < units.Count; i++)
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
    }
   
    public enum Directions
    {
        UP,DOWN,LEFT,RIGHT,NULL
    }
    
    
}
