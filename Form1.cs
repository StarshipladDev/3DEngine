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
using System.Runtime.CompilerServices;
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
        int playerID = 0;
        Cell[,] cellList = new Cell[Globals.cellSize, Globals.cellSize];
        Player thisPlayer = new Player(4,4,1,0);
        CursorObject cursor = null;
        List<Player> players = new List<Player>();
        Directions switcher;
        System.Timers.Timer aTimer;
        int shotsFired=0;
        bool server = false;
        bool cursorUp = false;
        Client thisClient = null;
        List<Unit> units = new List<Unit>();
        List<Unit> playerUnits = new List<Unit>();
        String commandString = String.Empty;
        String commandStringsNew = String.Empty;
        Color ColorFloor;
        Color ColorRoof;
        /// <summary>
        /// This constructor creates a Form. Parameters should be specified in <see cref="Globals"/>
        /// </summary>
        public Form1()
        {
            ReadConfig();
            playerUnits.Add(null);
            players.Add(thisPlayer);
            thisPlayer = players[0];
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
            ColorFloor = Color.FromArgb(150, 255,200,0);
            ColorRoof = Color.FromArgb(50, rand.Next(50), 255, rand.Next(50));
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
                        units.Add(cellList[xForBlock, yForBLock].createUnit(xForBlock, yForBLock, new Bitmap("Resources/Images/Enemy/Devil/Devil_Idle.png"),new Bitmap("Resources/Images/Enemy/Devil/Devil_Death.png")));
                    }
                }
            }
            cellList[3, 2].setMat(true, Color.Gray);
            //Print Cell
            PrintMap();
            InitializeComponent();
        }
        private void AddToCommandString(String s)
        {
            this.commandString += s + "^";
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
                Debug.WriteLine("Address is :" + configXml.SelectSingleNode("Config/ClientName").InnerText);
                Globals.port = configXml.SelectSingleNode("Config/Port").InnerText;
                Globals.Address = configXml.SelectSingleNode("Config/ServerAddress").InnerText;
                Globals.clientName= configXml.SelectSingleNode("Config/ClientName").InnerText;


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
            Globals.ticks++;
            if (Globals.ticks == 10000)
            {
                Globals.ticks = 0;
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
                this.FormUpdate(true,e.Graphics);
                Globals.readyToDraw = true;
            }
        }
        /// <summary>
        /// FormUpdate is used to perform the graphical calcualtiosn for where various objects will appear on the screen, and call their
        /// respective drawing functions in order.
        /// Commands must be added as a list from inside
        /// </summary>
        /// <param name="unitsa">Whether to draw non-cell specfic items (Such as units or player views)</param>
        /// 
        ///LIST OF COMMNAD TYPES
        ///
        /// ----
        ///MV -Move
        ///MVP - Move Player - MVP[00PlayerID][ch(1) Direction]{'F'-Foward,'B'-Back,'L'-Left,'R'-Right}
        ///
        /// ----
        ///DI - Direction
        ///DIP - Change Player Direction - DIP[00PlayerID][ch(1)]{'L'-Left,'R'-Right}
        ///
        /// 
        ///------
        ///SH - Deal Damage
        ///SHE - Shoot Enemy SHE[000 EnemyIndex][00 Palyer ID][0000 Damage]
        ///SHP - Deal Damage To Player -SHP[000 PlayerIndex][000 Damage]
        ///
        /// ------
        ///SE - Set GameWorld
        ///SEP - Set Player-SEP[00 PlayerID][000 Player X Position][000 Player Y Position][char(1) Player direction] - Creates Player of set ID at set co-ords
        ///SEW - Set World Block - SEW([000 BlockType][000 X Pos][000 Y Pos] x GLobals.MaxCell)- Creates Block of type 't' at location 'x','y'
        ///SEE - Set Enemy - SEE[000 Enemy Type][000 X Pos][000 Y Pos][0000 Deal Damage] - Places enemy and deals damage
        ///
        ///-------
        ///PR- Print
        ///PRT - Print Text - PRT[MessageString]
        ///
        ///--------
        ///DEL-Delete all Client Players
        ///DAL- Let CLient Draw AGain
        ///
        ///--------
        /// CR- Create
        /// CRP - Create Projectile- CRE[000 Projectile Type][000 000EnemyID][000 000Target X,Y]
        /// 
        ///
        ///---------
        ///RP= Run Projectiles
        /// RPR - Run Projectile -RPR[000]EnemyIdPrjecticleCount][000 EnemyId][000 Projectile ID]- Runs stated projectile's 'RunProjectile' function.
        ///
        ///
        private void RunCommands()
        {


            if (server || Globals.SinglePlayer)
            {
                if (Globals.ticks % (Globals.MAXFRAMES) == 0)
                {

                    for (int i = 0; i < units.Count; i++)
                    {
                        if (units[i].projs.Count > 0)
                        {
                            String commander = "RPR" + String.Format("{0:000}", units[i].projs.Count);
                            for (int f = 0; f < units[i].projs.Count; f++)
                            {

                                commander += String.Format("{0:000}{1:000}", i, f);


                            }

                            if (Globals.SinglePlayer)
                            {
                                AddToCommandString(commander);
                            }
                            else
                            {
                                this.thisClient.Write(commander);
                            }
                        }
                        
                    }
                }
                /*
             * Projectile implementation
             * */
                if (Globals.ticks % (Globals.MAXFRAMES * 4) == 0)
                {

                    Random rand = new Random();
                    int playerNum = rand.Next(players.Count);
                    int unitNum = rand.Next(units.Count);
                    if (rand.Next(10) > 1 && units[unitNum].alive)
                    {
                        if (!Globals.SinglePlayer)
                        {
                            this.thisClient.Write("CRP" + String.Format("{0:000}{1:000}{2:000}", unitNum, players[playerNum].GetX(), players[playerNum].Gety()));

                        }
                        else
                        {
                            AddToCommandString("CRP" + String.Format("{0:000}{1:000}{2:000}", unitNum, players[playerNum].GetX(), players[playerNum].Gety()));
                        }
                    }

                }
                
            }
            
            String h = commandString;
            if (!Globals.SinglePlayer && thisClient != null)
            {
                //h = String.Empty;
                lock (thisClient)
                {
                    h = thisClient.GetCommands();
                }
            }
            h += this.commandStringsNew;
            this.commandStringsNew = String.Empty;
            if (h != String.Empty)
            {
                Debug.WriteLine("Running commands :" + h);
                String[] commands = h.Split('^');
                for (int i = 0; i < commands.Length; i++)
                {

                    Debug.Write(" Command:'" + commands[i] + "'");
                    if (commands[i].Length>2)
                    {
                       // Debug.WriteLine("Reading Command '"+ commands[i]+"'");

                        Debug.WriteLine("After trim command is :'" + commands[i].Trim() + "'");
                        commands[i] = commands[i].Trim();
                        switch (commands[i].Substring(0, 3))
                        {
                            //
                            //MOVE PLAYER
                            //
                            case "MVP":
                                int x = Int32.Parse(commands[i].Substring(3, 2));
                                int oldx = players[x].GetX();
                                int oldy = players[x].Gety();
                                Debug.WriteLine(" Moving Player " + x +String.Format(" From {0},{0} ", oldx,oldy ));
                                if (x > -1 && x<players.Count)
                                {
                                    char directionChar= Char.Parse(commands[i].Substring(5, 1));
                                    switch(directionChar)
                                    {
                                        case 'F':
                                            Move("Forward", players[x]);
                                            break;
                                        case 'B':
                                            Move("Back", players[x]);
                                            break;
                                        case 'L':
                                            Move("Left", players[x]);
                                            break;
                                        case 'R':
                                            Move("Right", players[x]);
                                            break;
                                    }
                                    Debug.WriteLine(String.Format("To  {0},{1}.", players[x].GetX(), players[x].Gety()));
                                    if (x != playerID)
                                    {
                                        Debug.WriteLine("Removing player on " + oldx + "," + oldy + ", setting new player on" + players[x].GetX() + "," + players[x].Gety()) ;
                                        cellList[oldx, oldy].RemoveUnit();
                                        playerUnits[x] = (cellList[players[x].GetX(), players[x].Gety()].createUnit(players[x].GetX(), players[x].Gety(),new Bitmap("Resources/Images/Friendly/Player1/Player1_Idle.png"), new Bitmap("Resources/Images/Friendly/Player1/Player1_Death.png")));
                                        if (cellList[oldx, oldy].GetisUnitPresent())
                                        {
                                            Debug.WriteLine("There IS a unit on the old coords "+oldx+","+oldy);
                                        }
                                        if (cellList[players[x].GetX(), players[x].Gety()].GetisUnitPresent())
                                        {
                                            Debug.WriteLine("There IS a unit on the new coords " + oldx + "," + oldy);
                                        }
                                    }
                                }
                                
                                break;
                            case "RPR":
                                
                                int projCount = Int32.Parse(commands[i].Substring(3, 3));
                                Debug.WriteLine(" Firing "+ projCount+" Projectile's ");

                                for (int projCounter = 0; projCounter < projCount; projCounter++)
                                {
                                    Debug.WriteLine("------FIRING PROJECTILE--------");

                                    int unitNumber = Int32.Parse(commands[i].Substring((projCounter * 6) + 6, 3));
                                    if (units[unitNumber].projs.Count <= projCounter)
                                    {
                                        Debug.WriteLine("Unit had less projectiles than counter, ending");
                                        break;
                                    }
                                    int damage = units[unitNumber].projs[projCounter].GetDamage();

                                    int returnCode = units[unitNumber].projs[projCounter].RunProjecticle(this.cellList);
                                    Debug.WriteLine("Form1: Fireball returncode is :" + returnCode);

                                    if (returnCode > -2)

                                    {
                                        Debug.WriteLine("Fireball hit something with returnCode" + returnCode);
                                        units[unitNumber].projs.RemoveAt(projCounter);
                                        projCounter--;

                                    }
                                    if (returnCode > -1)
                                    {
                                        Debug.WriteLine("Fireball hit player "+returnCode);
                                        String ProjectileHitString = "SHP" + String.Format("{0:000}{1:000}", returnCode, damage);
                                        if (!Globals.SinglePlayer && server)
                                        {
                                            Debug.WriteLine("Server sending " + ProjectileHitString);
                                            this.thisClient.Write(ProjectileHitString);

                                        }
                                        if (Globals.SinglePlayer)
                                        {
                                            Debug.WriteLine("SinglePlayer sending " + ProjectileHitString);
                                            commandStringsNew += ProjectileHitString;
                                            Debug.WriteLine("Comand last idnex now "+commandString);
                                        }
                                    }
                                    Debug.WriteLine("------END FIRING PROJECTILE--------");
                                }
                                
                                break;
                            case "DIP":
                                int id = Int32.Parse(commands[i].Substring(3, 2));
                                if (id > -1 && id < players.Count)
                                {
                                    char directionChar = Char.Parse(commands[i].Substring(5, 1));
                                    Debug.WriteLine("Rotating Player " + id + " " + directionChar);
                                    switch (directionChar)
                                    {
                                        case 'L':
                                            players[id].ChangeDirection("Left");
                                            break;
                                        case 'R':
                                            players[id].ChangeDirection("Right");
                                            break;
                                    }
                                }
                                break;
                            case "SHP":
                                Debug.WriteLine("------DEALING DAMAGE TO PLAYER--------");
                                int playerHit = Int32.Parse(commands[i].Substring(3, 3));
                                int damageToPlayer = Int32.Parse(commands[i].Substring(6, 3));
                                Debug.WriteLine("Dealing damage to player" + playerHit);
                                players[playerHit].doDamage(damageToPlayer);
                                if (players[playerHit].IsDead())
                                {
                                    if (playerHit != playerID)
                                    {
                                        Debug.WriteLine("Player " + playerHit + " Died");
                                        playerUnits[playerHit].Kill();
                                    }
                                }
                                Debug.WriteLine("Player " + playerHit + " health is " + players[playerHit].health);
                                Debug.WriteLine("------DEALING DAMAGE TO PLAYER--------");

                                break;
                            //
                            //PRINT VALUE
                            //
                            case "PRT":
                                Debug.Write("Printing ");
                                if (commands[i].Length > 1)
                                {
                                    Globals.flags[5] = true;
                                    Globals.Message = commands[i].Substring(1, commands[i].Length - 1);
                                    Debug.Write(commands[i].Substring(1, commands[i].Length - 1));
                                }
                                break;
                                //Connect
                            case "COC":
                                //
                                //NEW PLAYER ADDED - SERVER HANDLING
                                //
                                if (server)
                                {
                                    Globals.Pause=true;
                                    Thread.Sleep(2000);
                                    Debug.WriteLine("Performing ServerClient Connection Request by making new player");
                                    players.Add(new Player(5 + players.Count, 5 + players.Count, 1,players.Count-1));
                                    playerUnits.Add(cellList[5 + players.Count - 1, 5 + players.Count - 1].createUnit(5 + players.Count - 1, 5 + players.Count - 1, new Bitmap("Resources/Images/Friendly/Player1/Player1_Idle.png"), new Bitmap("Resources/Images/Friendly/Player1/Player1_Death.png")));
                                    Debug.WriteLine("ServerClient: Messaging out New Players. There are " + players.Count + " players");
                                    thisClient.Write("DEL");
                                    ///SEP - Set Player-SEP[00 PlayerID][000 Player X Position][000 Player Y Position] - Creates Player of set ID at set co-ords
                                    ///SEW - Set World Block - SEW[000 BlockType][000 X Pos][000 Y Pos] - Creates Block of type 't' at location 'x','y'
                                    ///SEE - Set Enemy - SEE[000 Enemy Type][000 X Pos][000 Y Pos][0000 Deal Damage] - Places enemy and deals damage

                                    ///Delete All Projectiles before sending data
                                    for (int r = 0; r < units.Count; r++)
                                    {
                                        if (units[r].projs.Count > 0)
                                        {
                                            for (int t = 0; t < units[r].projs.Count; t++)
                                            {
                                                cellList[units[r].projs[t].x, units[r].projs[t].y].RemoveProjecticle();
                                                units[r].projs.RemoveAt(t);
                                                t--;

                                            }
                                        }

                                    }
                                    ///End deleting all data before firing projectiles
                                    for (int f = 0; f < Globals.cellSize; f++)
                                    {
                                        String builder = "SEW";
                                        for (int z = 0; z < Globals.cellSize; z++)
                                        {
                                            builder += String.Format("{0:000}", cellList[f, z].GetCellType()) + String.Format("{0:000}", f) + String.Format("{0:000}", z);
                                        }
                                        thisClient.Write(builder);
                                    }
                                    for (int f = 0; f < units.Count; f++)
                                    {
                                        String builder = "SEE";
                                        builder += "001";
                                        builder += String.Format("{0:000}", units[f].x);
                                        builder += String.Format("{0:000}", units[f].y);
                                        if (units[f].alive)
                                        {
                                            builder += String.Format("{0:0000}",0);
                                        }
                                        else
                                        {

                                            builder += String.Format("{0:0000}",20);
                                        }
                                        thisClient.Write(builder);
                                    }
                                    for (int f = 0; f < players.Count; f++)
                                    {
                                        char directionOfPlayer = 'U';
                                        int playerID = f;
                                        int playerx = players[playerID].GetX();
                                        int playery = players[playerID].Gety();
                                        Directions d = players[playerID].GetDirection();
                                        switch (d)
                                        {
                                            case Directions.DOWN:
                                                directionOfPlayer = 'D';
                                                break;
                                            case Directions.UP:
                                                directionOfPlayer = 'U';
                                                break;
                                            case Directions.LEFT:
                                                directionOfPlayer = 'L';
                                                break;
                                            case Directions.RIGHT:
                                                directionOfPlayer = 'R';
                                                break;
                                        }
                                        Debug.WriteLine(this.thisClient.GetName() + ": Created new player @ " + playerx + "," + playery + " via serverClient");
                                        thisClient.Write("SEP" + String.Format("{0:00}", playerID) + String.Format("{0:000}", playerx) + String.Format("{0:000}", playery)+ directionOfPlayer);

                                    }
                                    thisClient.Write("DAL");
                                    Globals.Pause = false;
                                }
                                break;
                            case "SEW":
                                if (!server)
                                {
                                    for (int z = 0; z < Globals.cellSize; z++)
                                    {
                                        // Debug.WriteLine("Command length is "+commands[i].Length);
                                        // Debug.WriteLine("accessing "+ ((z * 9) + 3)+" that is "+ commands[i].Substring(((z * 9) + 3), 3));
                                        int cellType = Int32.Parse(commands[i].Substring(((z * 9) + 3), 3));
                                        //  Debug.WriteLine("accessing " + ((z * 9) + 6) + " that is " + commands[i].Substring(((z * 9) + 6), 3));
                                        int cellx = Int32.Parse(commands[i].Substring(((z * 9) + 6), 3));
                                        //  Debug.WriteLine("accessing " + ((z * 9) + 9) + " that is " + commands[i].Substring(((z * 9) + 9), 3));
                                        int celly = Int32.Parse(commands[i].Substring(((z * 9) + 9), 3));
                                        cellList[cellx, celly] = new Cell(false, Color.FromArgb(0, 0, 0, 0), ColorFloor, ColorRoof);
                                        if (cellType == 1)
                                        {
                                            cellList[cellx, celly].setMat(true, Color.Black);
                                        }
                                    }
                                }
                            break;
                            case "CRP":
                                int unitID = Int32.Parse(commands[i].Substring(3, 3));
                                int targetX = Int32.Parse(commands[i].Substring(6, 3));
                                int targetY = Int32.Parse(commands[i].Substring(9, 3));
                                units[unitID].CreateProjectile("Fireball", targetX, targetY);
                                break;
                            case "SEE":
                                if (!server)
                                {
                                    int enemyType = Int32.Parse(commands[i].Substring(3, 3));
                                    int enemyX = Int32.Parse(commands[i].Substring(6, 3));
                                    int enemyY = Int32.Parse(commands[i].Substring(9, 3));
                                    int enemyDam = Int32.Parse(commands[i].Substring(12, 4));
                                    String enemyAlive = "None";
                                    String enemyDead = "None";
                                    switch (enemyType)
                                    {
                                        case (1):
                                            enemyAlive = "Resources/Images/Enemy/Devil/Devil_Idle.png";
                                            enemyDead = "Resources/Images/Enemy/Devil/Devil_Death.png";
                                            break;
                                    }
                                    units.Add(cellList[enemyX, enemyY].createUnit(enemyX, enemyY, new Bitmap(enemyAlive), new Bitmap(enemyDead)));
                                    units[units.Count - 1].DealDamage(enemyDam);
                                    Debug.WriteLine("");
                                }
                                
                                break;
                            case "COP":
                                Debug.WriteLine(thisClient.GetName() + ":ClientID is " + thisClient.GetID());
                                Debug.WriteLine(thisClient.GetName() + ":Setting Client ID to  " + commands[i].Substring(3, 2));
                                if (thisClient.GetID() == -1)
                                {
                                    thisClient.SetID(Int32.Parse(commands[i].Substring(3, 2)));
                                    this.playerID = Int32.Parse(commands[i].Substring(3, 2));
                                }
                                else
                                {
                                    Globals.flags[6] = true;
                                    Globals.ServerMessage = "ID already set";
                                    Debug.WriteLine(thisClient.GetName() + ":ID already set to" + thisClient.GetID());
                                }
                                break;
                                //DealDamageTOEnemy
                            case "SHE":
                                int shootingPlayer = Int32.Parse(commands[i].Substring(6, 2));
                                int enemyIndex = Int32.Parse(commands[i].Substring(3, 3));
                                int damageDone = Int32.Parse(commands[i].Substring(8, 4));
                                this.units[enemyIndex].DealDamage(damageDone);
                                break;
                            //
                            //SET UP CLIENT WORLD DETAILS
                            //
                            case "SEP":

                                if (!Globals.SinglePlayer && !server)
                                {
                                    int playerID1 = Int32.Parse(commands[i].Substring(3,2));
                                    int playerx1 = Int32.Parse(commands[i].Substring(5, 3));
                                    int playery1 = Int32.Parse(commands[i].Substring(8, 3));
                                    char d = Char.Parse(commands[i].Substring(11,1));
                                Globals.Message = "Creating new Player " + playerID1 + " at " + playerx1 + "," + playery1 + " count is " + players.Count;
                                    Debug.WriteLine("Creating new Player " + playerID1 + " at " + playerx1 + "," + playery1 + " count is " + players.Count);
                                    if (this.players.Count == playerID1)
                                    {
                                        this.players.Add(new Player(playerx1, playery1, 1,playerID1));
                                        Directions dir=Directions.NULL;
                                        switch (d)
                                        {
                                            case 'D':
                                                dir = Directions.DOWN;
                                                break;
                                            case 'U':
                                                dir = Directions.UP;
                                                break;
                                            case 'L':
                                                dir = Directions.LEFT;
                                                break;
                                            case 'R':
                                                dir = Directions.RIGHT;
                                                break;
                                        }
                                        players[playerID1].SetDirection(dir);
                                        if (this.playerID!=-1 && this.playerID != playerID1)
                                        {
                                            Globals.flags[5] = true;
                                            playerUnits.Add(cellList[playerx1, playery1].createUnit(playerx1, playery1,new Bitmap("Resources/Images/Friendly/Player1/Player1_Idle.png"), new Bitmap("Resources/Images/Friendly/Player1/Player1_Death.png")));
                                            cellList[playerx1, playery1].SetPlayer(players[playerID1].GetPlayerID());
                                            Debug.WriteLine(this.thisClient.GetName() + ": Created new player @ " + playerx1 + "," + playery1);
                                        }
                                    }
                                    if (playerID != -1 && this.playerID == playerID1)
                                    {
                                        playerUnits.Add(null);
                                        this.thisPlayer = this.players[this.playerID];
                                        this.players[playerID1].SetID(this.playerID);
                                        cellList[playerx1, playery1].SetPlayer(players[playerID1].GetPlayerID());
                                    }
                                }
                                break;
                            case "DEL":
                                if (!server)
                                {
                                    //h = String.Empty;
                                    Globals.pauseForInfo = true;
                                    this.players.Clear();
                                    this.units.Clear();
                                    this.playerUnits.Clear();
                                    this.cellList = new Cell[Globals.cellSize, Globals.cellSize];
                                }
                                break;
                            case "DAL":
                                if (!server)
                                {
                                    Globals.pauseForInfo = false;
                                }
                                break;
                            default:
                                Debug.WriteLine("Invalid command: '" + commands[i].Substring(0, 3) + "'");
                                break;
                        }
                    }
                    
                }
            }
            commandString = String.Empty;
        }
            /// <summary>
            /// FormUpdate is used to perform the graphical calcualtiosn for where various objects will appear on the screen, and call their
            /// respective drawing functions in order
            /// </summary>
            /// <param name="unitsa">Whether to draw non-cell specfic items (Such as units or player views)</param>
            private void FormUpdate(bool unitsa = false,Graphics e = null)
            {
                if (this.thisClient != null)
                {
                    this.playerID = thisClient.GetID();
                }
                RunCommands();

                Bitmap n = new Bitmap(this.Width, this.Height);
                Graphics g = Graphics.FromImage(n);

                
            if (!Globals.pauseForInfo)
                {
                        if (cursorUp)
                    {
                        //this.cursor.DebugPrint();
                        this.cursor.MoveCursor();
                    }
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
                    if (Globals.drawText)
                        {
                            if (Globals.flags[0])
                            {

                                g.DrawString("DrawingMap! ", new Font("Arial", 16), new SolidBrush(Color.Red),0, 0);
                                g.DrawString("Your direction is : " + thisPlayer.dir, new Font("Arial", 16), new SolidBrush(Color.Red), 0,20);
                                g.DrawString("Your MaxDepth is : " + maxDepth, new Font("Arial", 16), new SolidBrush(Color.Red), 0,40);
                                g.DrawString("PlayerID : " + this.thisPlayer.GetPlayerID(), new Font("Arial", 16), new SolidBrush(Color.Yellow), 0,60);


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
                                g.DrawString("Messaged Received At Client: " + Globals.Message, new Font("Arial", 16), new SolidBrush(Color.Red), 0, (this.Height / 2) + 60);

                            }
                            if (Globals.flags[6])
                            {
                                g.DrawString("Messaged Received At Server: " + Globals.ServerMessage, new Font("Arial", 10), new SolidBrush(Color.Blue), 0, (this.Height / 2) + 80);

                            }
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

                        if (cursorUp)
                        {
                            this.cursor.Draw(g);
                        }
                }
                else{
                        g.DrawString("Connected, Building new World", new Font("Arial", 16), new SolidBrush(Color.Black), this.Width / 2, (this.Height / 2) + 40);

                }
            if (thisPlayer.IsDead())
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(125, 255, 0, 0)), new RectangleF(0, 0, this.Width, this.Height));
                g.DrawString("You Died", new Font("Comic Sans", 60), new SolidBrush(Color.Red), 0, (this.Height / 2) + 40);

            }
            g.Dispose();
            e.DrawImage(n, 0, 0, n.Width, n.Height);
                    //e.Dispose();

                    //n.Save("Image"+Globals.animations[1]+".png");
                    //Globals.animations[1]++;

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

        public void Move(string direction,Player p)
        {
            Debug.WriteLine("Move command issued");
            if (direction == "Forward")
            {
                switch (p.dir)
                {
                    case Directions.UP:
                        ChangeY(true,p);
                        break;
                    case Directions.DOWN:
                        ChangeY(false, p);
                        break;
                    case Directions.LEFT:
                        ChangeX(false, p);
                        break;
                    case Directions.RIGHT:
                        ChangeX(true, p);
                        break;
                }
            }
            if (direction.Equals("Left"))
            {
                switch (p.dir)
                {
                    case Directions.UP:
                        ChangeX(false, p);
                        break;
                    case Directions.DOWN:
                        ChangeX(true, p);
                        break;
                    case Directions.LEFT:
                        ChangeY(false, p);
                        break;
                    case Directions.RIGHT:
                        ChangeY(true, p);
                        break;
                }
            }
            if (direction.Equals("Right"))
            {
                switch (p.dir)
                {
                    case Directions.UP:
                        ChangeX(true, p);
                        break;
                    case Directions.DOWN:
                        ChangeX(false, p);
                        break;
                    case Directions.LEFT:
                        ChangeY(true, p);
                        break;
                    case Directions.RIGHT:
                        ChangeY(false, p);
                        break;
                }
            }
            if (direction.Equals("Back"))
            {
                switch (p.dir)
                {
                    case Directions.UP:
                        ChangeY(false, p);
                        break;
                    case Directions.DOWN:
                        ChangeY(true, p);
                        break;
                    case Directions.LEFT:
                        ChangeX(true, p);
                        break;
                    case Directions.RIGHT:
                        ChangeX(false, p);
                        break;
                }
            }

        }
        /// <summary>
        /// ChangeX trys to change the player's x co-ordinate. This method deals with invalid movement
        /// </summary>
        /// <param name="right">States whether to move the palyer right. If false, palyer will mvoe left</param>
        private void ChangeX(bool right,Player p)
        {
            int x = p.GetX();
            int y = p.Gety();
            if (right)
            {
                if (x < cellList.GetLength(1) - 1)
                {
                    if (!cellList[x + 1, y].GetMat() && !cellList[x + 1, y].GetisUnitPresent())
                    {
                        cellList[x, y].SetPlayer(-1);
                        cellList[x + 1, y].SetPlayer(p.GetPlayerID());
                        p.SetX(p.GetX() + 1);
                    }
                }
            }
            else
            {
                if (x > 1)
                {
                    if (!cellList[x - 1, y].GetMat() && !cellList[x - 1, y].GetisUnitPresent())
                    {
                        cellList[x, y].SetPlayer(-1);
                        cellList[x - 1, y].SetPlayer(p.GetPlayerID());
                        p.SetX(p.GetX() - 1);
                    }
                }
            }
        }
        /// <summary>
        /// ChangeY trys to change the player's y co-ordinate. This method deals with invalid movement
        /// </summary>
        /// <param name="up">States whether to move the palyer up. If false, player will move down</param>
        private void ChangeY(bool up,Player p)
        {
            int x = p.GetX();
            int y = p.Gety();
            if (up)
            {
                if (y > 1)
                {
                    if (!cellList[x, y - 1].GetMat() && !cellList[x, y - 1].GetisUnitPresent())
                    {
                        cellList[x, y].SetPlayer(-1);
                        cellList[x, y-1].SetPlayer(p.GetPlayerID());
                        p.SetY(p.Gety() - 1);
                    }
                }

            }
            else
            {
                if (y < cellList.GetLength(1) - 1)
                {
                    if (!cellList[x, y + 1].GetMat() && !cellList[x, y + 1].GetisUnitPresent())
                    {
                        cellList[x, y].SetPlayer(-1);
                        cellList[x, y + 1].SetPlayer(p.GetPlayerID());
                        p.SetY(p.Gety() + 1);
                    }
                }
            }
        }
        private void MoveCommand(char direction)
        {
            if (Globals.SinglePlayer)
            {
                AddToCommandString("MVP"+String.Format("{0:00}", 0) +direction);
            }
            else
            {
                this.thisClient.Write("MVP" + String.Format("{0:00}", thisClient.GetID()) + direction);
            }
        }
        private void DirectionCommand(char direction)
        {
            if (Globals.SinglePlayer)
            {
                AddToCommandString("DIP" + String.Format("{0:00}", 0) + direction);
            }
            else
            {
                this.thisClient.Write("DIP" + String.Format("{0:00}", this.thisClient.GetID()) + direction);
            }
        }
        /// <summary>
        /// OnKeyUp handles key presses done while a <see cref="Form1"/> isntance is the active window
        /// </summary>
        /// <param name="sender">The object that sent this call </param>
        /// <param name="e">The arguments of this particular key press</param>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            bool servermade = false;
            Directions playersdir = thisPlayer.dir;
            Thread f = null;
            switch (e.KeyCode)
            {
                case Keys.Space :

                    this.Text = "DOOMCLoneV2Lines";
                    Globals.drawLines = !Globals.drawLines;
                    break;
                case Keys.D1:
                    this.Text = "DOOMCLoneV2Lines";
                    Globals.drawLines = !Globals.drawLines;
                    break;
                case Keys.D2:
                    this.Text = "DOOMCLoneV2Color";
                    Globals.drawFill = !Globals.drawFill;
                    break;
                case Keys.D3:
                    this.Text = "DOOMCLoneV2Gun";
                    Globals.drawGun = !Globals.drawGun;
                    break;
                case Keys.D4:
                    this.Text = "DoomClone ";
                    Globals.drawText = !Globals.drawText;
                    break;
                case Keys.S:

                    MoveCommand('B');
                    break;
                case Keys.W:

                    MoveCommand('F');
                    break;
                case Keys.D:

                    MoveCommand('R');
                    break;
                case Keys.A:
                    MoveCommand('L');
                    break;
                case Keys.Q:
                    this.thisPlayer.RefreshGun();
                    RefreshPlayerView(this.thisPlayer.playerView);
                    break;
                case Keys.U:
                    Globals.flags[0] = !Globals.flags[0];
                    for (int i = 3; i < Globals.flags.Length; i++)
                    {
                        Globals.flags[i] = false;
                    }
                    PrintMap();
                    break;
                //Server
                case Keys.O:
                    if (this.server == false)
                    {
                        this.server = true;
                        Globals.SinglePlayer = false;
                        f = new Thread(ThreadFunctions.ThreadRunnerServer);
                        f.Start();
                        Globals.flags[0] = false;
                        for (int i = 3; i < Globals.flags.Length; i++)
                        {
                            Globals.flags[i] = false;
                        }
                        Globals.flags[3] = true;
                        servermade = true;
                    }
                    break;
                case Keys.P:
                    Globals.SinglePlayer = false;
                    if (thisClient == null)
                    {
                        Globals.flags[0] = false;
                        for (int i = 3; i < Globals.flags.Length; i++)
                        {
                            Globals.flags[i] = false;
                        }
                        Globals.flags[4] = true;
                        f = new Thread(ThreadFunctions.ClientThread);
                        object args1;
                        this.thisClient = new Client(Globals.port, Globals.Address, "The Client");
                        args1 = this.thisClient;
                        f.Start(args1);
                    }
                    break;
                case Keys.L:
                    Globals.SinglePlayer = false;
                    
                        f = new Thread(ThreadFunctions.ClientThread);
                        object args;
                        Client cl = new Client(Globals.port, Globals.Address, "The Client2",1);
                        args = cl;
                        f.Start(args);
                    break;
                case Keys.K:
                    if (Globals.SinglePlayer)
                    {
                        AddToCommandString("P" + "Hey There From This Client at " + DateTime.Now);
                    }
                    else
                    {
                        this.thisClient.Write("P" + "Hey There From This Client at " + DateTime.Now);
                    }
                    break;
                case Keys.V:
                    if (!Globals.SinglePlayer)
                    {
                        this.thisClient.Write("ML1");
                    }
                    break;
            }
            if (servermade == true)
            {
                if (thisClient == null)
                {
                    Globals.flags[0] = false;
                    for (int i = 3; i < Globals.flags.Length; i++)
                    {
                        Globals.flags[i] = false;
                    }
                    Globals.flags[4] = true;
                    f = new Thread(ThreadFunctions.ClientThread);
                    object args;
                    this.thisClient = new Client(Globals.port, Globals.Address, "The Client");
                    args = this.thisClient;
                    f.Start(args);
                }
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
                Debug.Write("\n\n");
                //X
                for (int i = 0; i < cellList.GetLength(1); i++)
                {
                    Debug.Write("[");
                    if (cellList[i,f].GetMat())
                    {
                        Debug.Write(" * ");
                    }
                    else if (thisPlayer.GetX() == i && thisPlayer.Gety() == f)
                    {
                        Debug.Write("<3 ");
                    }

                    else if (cellList[i, f].GetPlayer() > -1)
                    {
                        Debug.Write(" " + cellList[i, f].GetPlayer() + " ");
                    }
                    else if (cellList[i, f].GetisUnitPresent())
                    {
                        Debug.Write("^-^");
                    }
                    else
                    {
                        Debug.Write("   ");
                    }
                    Debug.Write("]");

                }
            }
        }
        /// <summary>
        /// CursorHandler is what happens if a click occurs while the cursor is up
        /// </summary>
        private void CursorHandler()
        {

            //Set gun as true and redraw
            Globals.flags[1] = true;
            thisPlayer.GetPlayerGunSound().Play();
            int x = cursor.GetCoords()[0];
            int y = cursor.GetCoords()[1];
            for (int i = 0; i < units.Count; i++)
            {
                Unit unitClicked = units[i];
                if (x > unitClicked.topLeft.X && x < unitClicked.bottomRight.X)
                {
                    if (y < unitClicked.bottomRight.Y && y > unitClicked.topLeft.Y)
                    {
                        if (unitClicked.alive == true)
                        {
                            Debug.WriteLine("Unit is alive, dealing damage");
                            Debug.WriteLine("Player is "+ String.Format("{0:00}",this.playerID));
                            Debug.WriteLine("Gun Damage is " + String.Format("{0:0000}", thisPlayer.GetGunDamage()));
                            Debug.WriteLine("Enemy index i is " + String.Format("{0:000}",i));
                            Debug.WriteLine("Those 3 things together are " + String.Format("{0:000}", i)+"-" + String.Format("{0:00}", this.playerID) +"-"+ String.Format("{0:0000}", thisPlayer.GetGunDamage()));
                            if (Globals.SinglePlayer)
                            {
                                AddToCommandString("SHE"+String.Format("{0:000}00{1:0000}",i,thisPlayer.GetGunDamage()));
                            }
                            else
                            {
                                this.thisClient.Write("SHE" + String.Format("{0:000}", i) + String.Format("{0:00}", this.playerID) + String.Format("{0:0000}", thisPlayer.GetGunDamage()));
                            }
                           // unitClicked.DealDamage(thisPlayer.GetGunDamage());
                        }
                    }
                }
            }
            if (shotsFired == 3)
            {

                cursorUp = false;
                shotsFired = 0;
                this.cursor = null;
            }
            else
            {
                shotsFired++;
            }
        }
                
        /// <summary>
        /// Handles the logic of a unit being clicked
        /// </summary>
        /// <param name="unitClicked"></param>
        private void ClickHandler(Unit unitClicked,int x,int y)
        {
            cursorUp = true;
            this.cursor = new CursorObject(new Bitmap("Resources/Images/Utility/Crosshair.png"),x,y, this.Height/5,50);
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
                    ClickHandler(unitClicked,x,y);

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
            if (cursorUp)
            {
                if (e.Button == MouseButtons.Right)
                {
                    cursorUp = false;
                }
                else
                {

                    CursorHandler();
                }
            }
            else
            {
                for (int i = 0; i < units.Count; i++)
                {
                    if (ClickedOn(units[i], e.X, e.Y) == 1)
                    {
                        i = units.Count;
                    }
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

                    DirectionCommand('R');
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

                    DirectionCommand('L');
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
