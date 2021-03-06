﻿using DoomCloneV2.Overlays;
using DoomCloneV2.Screens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        Player thisPlayer;
        CursorObject cursor = null;
        List<Player> players = new List<Player>();
        Directions switcher;
        System.Timers.Timer aTimer;
        int shotsFired=0;
        int turnCount = 0;
        int playerDoneCount = 0;
        int debugTimesTaken = 0;
        int loadingProgress = 0;
        bool server = false;
        bool cursorUp = false;
        bool serverTurn = false;
        bool menu = true;
        bool aboutOn = false;
        bool drawMapMakeMenu = false;
        bool drawPrompt = false;
        bool[] playerEndTurnArray = new bool[20];
        bool lockPlayer = false;
        Client thisClient = null;
        List<Unit> units = new List<Unit>();
        List<Unit> playerUnits = new List<Unit>();
        String commandString = String.Empty;
        public  String commandStringsNew = String.Empty;
        TimeSpan averageTimeToRun = TimeSpan.FromSeconds(0);
        TimeSpan averageTimeToRunDrawing = TimeSpan.FromSeconds(0);
        PrivateFontCollection pfc;
        CommandReader cr;
        Color ColorFloor;
        Color ColorRoof;
        Color colorWall;
        DoomMenuItem play;
        DoomMenuItem about;
        DoomMenuItem back;

        DoomMenuItem mapBack;
        DoomMenuItem map;
        TextDisplay textDisplay;
        Hud playerHud;
        /// <summary>
        /// This constructor creates a Form. Parameters should be specified in <see cref="Globals"/>
        /// </summary>
        public Form1()
        {
            Globals.Play(Application.StartupPath + "/Resources/Sound/SpaceWhales.wav");
            ReadConfig();
            //These menu items are handled via their 'OnClick' commands, run from the mosuehandler method
            DoomMenuItem.actionfunction ac1 = BeginSession;
            DoomMenuItem.actionfunction ac2 = ToggleFunc;
            DoomMenuItem.actionfunction ac3 = ToggleMapFunc;
            DoomMenuItem.actionfunction ac4 = BackMenu;
            play = new DoomMenuItem(300, 200, Image.FromFile("Resources/Images/Menu/Play.png"), ac1);
            about = new DoomMenuItem(300, 300, Image.FromFile("Resources/Images/Menu/About.png"), ac2);
            back = new DoomMenuItem(400, 400, Image.FromFile("Resources/Images/Menu/Back.png"), ac4);
            map = new DoomMenuItem(300, 400, Image.FromFile("Resources/Images/Menu/MapMaker.png"), ac3);
            mapBack = new DoomMenuItem(400, 400, Image.FromFile("Resources/Images/Menu/Back.png"), ac4);
            pfc = new PrivateFontCollection();
            pfc.AddFontFile("Resources/Fonts/Doomfont.ttf");
            InitializeComponent();
        }

        /// <summary>
        /// BeginSession is a function that begins initalizing the componenets of the playable gameworld, 
        /// as required for a single palyer game.
        /// It is called to enter the 'play' state of the application
        /// </summary>
        public void BeginSession()
        {

            menu = false;
            Globals.pauseForInfo = true;
            AddLoadValue(10);
            //Start up combat player
            thisPlayer = new Player(4, 4, 1, 0, "Player" + String.Format("{0:00}", Int32.Parse(Globals.playerFileName)));
            thisPlayer.SetPowerUp(Player.PowerUpTypes.KATANA);
            playerUnits.Add(null);
            players.Add(thisPlayer);
            thisPlayer = players[0];
            AddLoadValue(10);
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer,true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            for (int i = 0; i < Globals.animations.Length; i++)
            {
                Globals.animations[i] = 0;
            }
            for (int i = 0; i < Globals.flags.Length; i++)
            {
                Globals.flags[i] = false;
            }
            Debug.WriteLine("Updating GunSize");
            thisPlayer.updateGunSize(this.Width/2, this.Height/2);
            //Put player at first empty area
            Random rand = new Random();
            bool playerDown = false;
            while (!playerDown)
            {

                Point playerPoint = Globals.GetFreeCell(rand);
                if (!Globals.cellListGlobal[playerPoint.X, playerPoint.Y].GetMat())
                {
                    thisPlayer.SetX(playerPoint.X);
                    thisPlayer.SetY(playerPoint.Y);
                    Globals.cellListGlobal[playerPoint.X, playerPoint.Y].SetPlayer(0);
                    playerDown = true;
                }

            }
            AddLoadValue(20);
            //Set Random Cells
            //07/07/2020 -  no blocks created, only enemies. 
            units=Globals.FillMapWithEnemies();
            AddLoadValue(20);
            //20/01/2021 - Add palyer Hud
            playerHud = new Hud(ref thisPlayer);
            AddLoadValue(20);
            //Print Cell
            PrintMap();
            AddLoadValue(20);
            Globals.pauseForInfo = false;
            loadingProgress = 0;

            Globals.StopMusic();
        }
       


        private void AddLoadValue(int value)
        {
            loadingProgress += value;
            this.Refresh();
            if (loadingProgress >= 100)
            {
                loadingProgress = 0;
            }
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
                Globals.playerFileName = configXml.SelectSingleNode("Config/PlayerFileName").InnerText;
                Debug.WriteLine("PlayerFileName is :" + String.Format("{0:00}", Int32.Parse(Globals.playerFileName)));
                try
                {   int kataR = Int32.Parse(configXml.SelectSingleNode("Config/KatanaRed").InnerText);
                    int kataG = Int32.Parse(configXml.SelectSingleNode("Config/KatanaGreen").InnerText);
                    int kataB = Int32.Parse(configXml.SelectSingleNode("Config/KatanaBlue").InnerText);

                    Globals.katanaColor = Color.FromArgb(255, kataR, kataG, kataB);
                }
                catch(Exception e)
                {
                    Globals.WriteDebug("Form1.cs -> ReadConfig ","Error colouring Katana",true);
                    Globals.katanaColor = Color.FromArgb(255, 255, 0, 0);
                }

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
            if (DateTime.Now.Subtract(Globals.lastPlayerMusic) > TimeSpan.FromSeconds(30))
            {
                Globals.StopMusic();
                Globals.Play(Application.StartupPath + "/Resources/Sound/Combat.wav");
                Globals.lastPlayerMusic = DateTime.Now;
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
            if(menu)
            {
                this.DrawMenu(e.Graphics);
            }
            else if (Globals.readyToDraw)
            {

                Globals.readyToDraw = false;
                base.OnPaint(e);
                this.FormUpdate(true,e.Graphics);
                Globals.readyToDraw = true;
            }
        }
        public string GetColourString(Color c)
        {
            String returnString = "";
            returnString += String.Format("{0:000}",c.R);
            returnString += String.Format("{0:000}", c.G);
            returnString += String.Format("{0:000}", c.B);
            return returnString;
        }
        /// <summary>
        /// ServerSend is a utility feature to make actions that should be matched across all palyers occur.
        /// </summary>
        /// <param name="message"></param>
        public void ServerSend(String message)
        {
            if (!Globals.SinglePlayer)
            {
                this.thisClient.Write(message);
            }
            else
            {
                AddToCommandString(message);
            }
        }
        private bool CheckPlayerHasAP(int playerIDToCheck)
        {
            if (players[playerIDToCheck].ChangeActionPoints(0) == 0)
            {
                commandStringsNew += ("ETP" + String.Format("{0:000}", playerIDToCheck));
                Globals.WriteDebug("Form -> CheckPlayerHasAP() ->", "Actioning Player " + playerIDToCheck + " and their AP is " + players[playerIDToCheck].ChangeActionPoints(0) + "Running ETP", true);
                return false;
            }
            else
            {
                return true;
            }
        }


        private bool ClearLineOfSight(int targetx,int targety, Entity shooter)
        {
            bool canHit = false;
            if (targetx == shooter.x)
            {
                bool canhitThisWay = true;
                if (targety > shooter.y)
                {
                    for (int i = 0; i < Math.Abs(targety - shooter.y); i++)
                    {
                        if (Globals.cellListGlobal[shooter.x, shooter.y + i].GetMat())
                        {
                            canhitThisWay=false;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Abs(shooter.y - targety); i++)
                    {
                        if (Globals.cellListGlobal[targetx, targety + i].GetMat())
                        {
                            canhitThisWay = false;
                        }
                    }
                }
                canHit = canhitThisWay;
            }
            if (!canHit && targety == shooter.y)
            {
                bool canhitThisWay = true;
                if (targetx > shooter.x)
                {
                    for (int i = 0; i < Math.Abs(targetx - shooter.x); i++)
                    {
                        if (Globals.cellListGlobal[shooter.x + i, shooter.y].GetMat())
                        {
                            canhitThisWay = false;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Abs(shooter.x - targetx); i++)
                    {
                        if (Globals.cellListGlobal[targetx + i, targety].GetMat())
                        {
                            canhitThisWay = false;
                        }
                    }
                }
                canHit = canhitThisWay;
            }
            if (!canHit && Math.Abs(targetx- shooter.x)== Math.Abs(targety - shooter.y))
            {
                Point point = new Point(targetx, targety);
                bool canhitThisWay = true;
                while (canhitThisWay && point.X != shooter.x && point.Y != shooter.y)
                {
                    if (point.X < shooter.x)
                    {
                        point.X += 1;
                    }
                    if (point.X > shooter.x)
                    {
                        point.X -= 1;
                    }
                    if (point.Y < shooter.y)
                    {
                        point.Y += 1;
                    }
                    if (point.Y > shooter.y)
                    {
                        point.Y -= 1;
                    }
                    if (Globals.cellListGlobal[point.X, point.Y].GetMat())
                    {
                        canhitThisWay = false;
                    }
                }
                canHit = canhitThisWay;
            }
            return canHit;
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
        ///CO - Connect
        ///COC - Tell Server player 'x' Connected - COC[00 - PlayerID][00- Player Skin]
        /// 
        /// 
        /// ------
        ///SE - Set GameWorld / Connect
        ///SEP - Set Player-SEP[00 PlayerID][000 Player X Position][000 Player Y Position][char(1) Player direction][00 PlayerCharacterFile][0 Player AP] - Creates Player of set ID at set co-ords
        ///SEW - Set World Block - SEW([000 BlockType][000 X Pos][000 Y Pos] x GLobals.MaxCell)- Creates Block of type 't' at location 'x','y'
        ///SEE - Set Enemy - SEE[000 Enemy Type][000 X Pos][000 Y Pos][0000 Deal Damage] - Places enemy and deals damage
        ///SEC - Set Colour - SEX[ 3 X [000000000 RGB  values in a '000' format] in order- wall colour, floor colour, roof colour]
        ///-------
        ///PR- Print
        ///PRT - Print Text - PRT[MessageString]
        ///
        ///--------
        /// MISC
        ///DEL-Delete all Client Players
        ///DAL- Let CLient Draw AGain
        ///DPU - Do Power Up -DPU[00 PlayerID ]
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
        ///----------
        ///ET - End Turn
        ///ETP - End Turn Player - ETP[000PlayerID] - Lets server know player has ended turn
        ///ETS - End Turn Server -ETS -Let's players Know Server is finished
        ///
        private void RunCommands()
        {
            CommandReader cR = new CommandReader(ref players,this,ref Globals.cellListGlobal,ref playerUnits,ref units);
            if (server || Globals.SinglePlayer)
            {
                //
                //Do things That happen on the server side
                //
                if (serverTurn)
                {
                    Globals.WriteDebug("Form -> RunCommands() ->Server Is True", "Server actioning ", true);

                    for (int i = 0; i < units.Count; i++)
                    {
                        if (units[i].projs.Count > 0)
                        {
                            String commander = "RPR" + String.Format("{0:000}", units[i].projs.Count);
                            for (int f = 0; f < units[i].projs.Count; f++)
                            {
                                commander += String.Format("{0:000}{1:000}", i, f);
                            }
                            ServerSend(commander);
                        }

                    }

                    /*
                     * Projectile implementation
                    * */
                    if (turnCount % 4 == 0)
                    {

                        Random rand = new Random();
                        int playerNum = rand.Next(players.Count);
                        int unitNum = rand.Next(units.Count);
                        if (rand.Next(10) > 1 && units[unitNum].alive)
                        {
                            //If there is a clearLine of Sight
                            if (ClearLineOfSight(players[playerNum].GetX(),players[playerNum].Gety(), units[unitNum])) ;
                            ServerSend("CRP" + String.Format("{0:000}{1:000}{2:000}", unitNum, players[playerNum].GetX(), players[playerNum].Gety()));
                        }

                    }
                    ServerSend("ETS");
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

                        #region runcommands


                        switch (commands[i].Substring(0, 3))
                        {
                            //
                            //MOVE PLAYER
                            //
                            case "MVP":
                                int movingPlayer = Int32.Parse(commands[i].Substring(3, 2));
                                Globals.WriteDebug("Form -> RunCommands() ->", "Moving Player " + movingPlayer + " and their AP is " + players[movingPlayer].ChangeActionPoints(0), true);
                                cR.MovePlayer(playerID, commands[i],ref thisClient,ref commandStringsNew ,server);
                                players[movingPlayer].ChangeActionPoints(-1);
                                CheckPlayerHasAP(movingPlayer);
                                break;
                            case "RPR":
                                cR.RunProjectile(commands[i], ref thisClient, ref commandStringsNew, server);
                                break;
                            //
                            //END SERVER TURN
                            //
                            case "ETS":
                                if(server || Globals.SinglePlayer)
                                {
                                    Globals.WriteDebug("Form -> RunCommands() ->ETS", "Server started finished actioning  Palyer AP is " + thisPlayer.ChangeActionPoints(0), true);
                                    for (int resetPlayers = 0; resetPlayers < players.Count(); resetPlayers++)
                                    {
                                        players[resetPlayers].ChangeActionPoints(5);
                                    }
                                    for (int playerCounter = 0; playerCounter < playerEndTurnArray.Length; playerCounter++)
                                    {
                                        playerEndTurnArray[playerCounter] = false;
                                    }
                                    playerDoneCount = 0;
                                    serverTurn = false;
                                }
                                if (thisPlayer.IsDead())
                                {
                                    ServerSend("ETP" + String.Format("{0:000}", this.playerID));
                                }
                                else
                                {
                                    lockPlayer = false;
                                    this.players[playerID].ChangeActionPoints(5);
                                }
                                Globals.WriteDebug("Form -> RunCommands() ->ETS", "Server ended finished actioning , Palyer AP is "+thisPlayer.ChangeActionPoints(0), true);
                                break;
                            //
                            //END TURN PLAYER
                            //
                            case "ETP":

                                int playerIndex = Int32.Parse(commands[i].Substring(3, 3));
                                if (playerIndex == playerID)
                                {
                                    lockPlayer = true;
                                }
                                if (server || Globals.SinglePlayer)
                                {
                                    if (!playerEndTurnArray[playerIndex])
                                    {
                                        playerEndTurnArray[playerIndex] = true;
                                        playerDoneCount++;
                                    }
                                    if (playerDoneCount == players.Count())
                                    {
                                        Globals.WriteDebug("Form -> RunCommands() -> EP -> ", "All Players Actioned, Running Server", true);
                                        serverTurn = true;
                                    }
                                }

                                break;
                            //
                            //CHANGE PLAYER DIRECTION
                            //
                            case "DIP":
                                int id = Int32.Parse(commands[i].Substring(3, 2));
                                if (id > -1 && id < players.Count)
                                {
                                    players[id].ChangeActionPoints(-1);
                                    cR.ChangeDirection(id, commands[i]);
                                    CheckPlayerHasAP(id);
                                }
                                break;
                            //
                            //Set Player to be doing Powerup
                            //
                            case "DPU":
                                int playerID2 = Int32.Parse(commands[i].Substring(3, 2));
                                if (playerID2 > -1 && playerID2 < players.Count)
                                {
                                    if (! players[playerID2].usingPowerUpFrame)
                                    {
                                        cR.Powerup(playerID2, commands[i],players[playerID2].GetPowerup().powerUpType);
                                    }
                                }
                                break;
                            //
                            // DAMAGE PLAYER
                            //
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
                            //
                            //PLAYER CONNECTED
                            //
                            case "COC":
                                //
                                //NEW PLAYER ADDED - SERVER HANDLING
                                //
                                if (server)
                                {
                                    Globals.Pause = true;
                                    Thread.Sleep(2000);
                                    Debug.WriteLine("Performing ServerClient Connection Request by making new player");
                                    String colorString = "";
                                    colorString += "SEC";
                                    colorString += GetColourString(Globals.drawColor) + GetColourString(Globals.floorColor) + GetColourString(Globals.roofColor);
                                    thisClient.Write(colorString);
                                    String fileName = commands[i].Substring(5, 2);
                                    int filenameInt = Int32.Parse(fileName);
                                    fileName = "Player" + String.Format("{0:00}", filenameInt);
                                    Debug.WriteLine("Server PlayerName is " + thisPlayer.playerFileName);
                                    Debug.WriteLine("New PlayerName is " + fileName);
                                    Debug.WriteLine("Respective substrings are " + thisPlayer.playerFileName.Substring(thisPlayer.playerFileName.Length - 2, 2) + "," + fileName.Substring(fileName.Length - 2, 2));
                                    Point playerNewPoint = Globals.GetFreeCell(new Random());
                                    players.Add(new Player(playerNewPoint.X, playerNewPoint.Y, 1, players.Count, fileName));
                                    playerUnits.Add(Globals.cellListGlobal[playerNewPoint.X, playerNewPoint.Y].CreateUnit(playerNewPoint.X, playerNewPoint.Y,players.Count-2, Globals.UnitType.Player, "Resources/Images/Friendly/" + fileName + "/" + fileName + "_Idle.png"));                                    Debug.WriteLine("ServerClient: Messaging out New Players. There are " + players.Count + " players");
                                    thisClient.Write("DEL");
                                    ///SEP - Set Player-SEP[00 PlayerID][000 Player X Position][000 Player Y Position][char(1) Player direction][00 PlayerCharacterFile][0 Player AP] - Creates Player of set ID at set co-ords
                                    ///SEW - Set World Block - SEW[000 BlockType][000 X Pos][000 Y Pos] - Creates Block of type 't' at location 'x','y'
                                    ///SEE - Set Enemy - SEE[000 Enemy Type][000 X Pos][000 Y Pos][0000 Deal Damage] - Places enemy and deals damage
                                    ///Delete All Projectiles before sending data
                                    for (int r = 0; r < units.Count; r++)
                                    {
                                        if (units[r].projs.Count > 0)
                                        {
                                            for (int t = 0; t < units[r].projs.Count; t++)
                                            {
                                                Globals.cellListGlobal[units[r].projs[t].x, units[r].projs[t].y].RemoveProjecticle();
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
                                            builder += String.Format("{0:000}", Globals.cellListGlobal[f, z].GetCellType()) + String.Format("{0:000}", f) + String.Format("{0:000}", z);
                                        }
                                        thisClient.Write(builder);
                                    }
                                    for (int f = 0; f < units.Count; f++)
                                    {
                                        String builder = "SEE";
                                        builder += String.Format("{0:000}", (int)units[f].GetUnitType());
                                        builder += String.Format("{0:000}", units[f].x);
                                        builder += String.Format("{0:000}", units[f].y);
                                        if (units[f].alive)
                                        {
                                            builder += String.Format("{0:0000}", 0);
                                        }
                                        else
                                        {

                                            builder += String.Format("{0:0000}", 20);
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
                                        thisClient.Write("SEP" + String.Format("{0:00}", playerID) + String.Format("{0:000}", playerx) + String.Format("{0:000}", playery) + directionOfPlayer + String.Format("{0:00}", players[f].playerFileName.Substring(players[f].playerFileName.Length - 2, 2)+players[f].ChangeActionPoints(0)));

                                    }
                                    thisClient.Write("DAL");
                                    Globals.Pause = false;
                                }
                                break;
                            //
                            //SETWORLDBLOCK.
                            //
                            case "SEW":
                                if (!server)
                                {
                                    for (int z = 0; z < Globals.cellSize; z++)
                                    {
                                        AddLoadValue(100 / Globals.cellSize);
                                        // Debug.WriteLine("Command length is "+commands[i].Length);
                                        // Debug.WriteLine("accessing "+ ((z * 9) + 3)+" that is "+ commands[i].Substring(((z * 9) + 3), 3));
                                        int cellType = Int32.Parse(commands[i].Substring(((z * 9) + 3), 3));
                                        //  Debug.WriteLine("accessing " + ((z * 9) + 6) + " that is " + commands[i].Substring(((z * 9) + 6), 3));
                                        int cellx = Int32.Parse(commands[i].Substring(((z * 9) + 6), 3));
                                        //  Debug.WriteLine("accessing " + ((z * 9) + 9) + " that is " + commands[i].Substring(((z * 9) + 9), 3));
                                        int celly = Int32.Parse(commands[i].Substring(((z * 9) + 9), 3));
                                        Globals.cellListGlobal[cellx, celly] = new Cell(false, Color.FromArgb(0, 0, 0, 0), Globals.floorColor, Globals.roofColor);
                                        if (cellType == 1)
                                        {
                                            Globals.cellListGlobal[cellx, celly].setMat(true, Globals.drawColor);
                                        }
                                    }
                                }
                                break;
                            //
                            //CREATE PROJECTILE
                            //
                            case "CRP":
                                int unitID = Int32.Parse(commands[i].Substring(3, 3));
                                int targetX = Int32.Parse(commands[i].Substring(6, 3));
                                int targetY = Int32.Parse(commands[i].Substring(9, 3));
                                units[unitID].CreateProjectile(targetX, targetY);
                                break;
                            //
                            //SERVER SENT CREATE ENEMY
                            //
                            case "SEE":
                                if (!server)
                                {
                                    int enemyType = Int32.Parse(commands[i].Substring(3, 3));
                                    int enemyX = Int32.Parse(commands[i].Substring(6, 3));
                                    int enemyY = Int32.Parse(commands[i].Substring(9, 3));
                                    int enemyDam = Int32.Parse(commands[i].Substring(12, 4));
                                    Globals.UnitType enemyTypeString = Globals.UnitType.Devil;
                                    switch (enemyType)
                                    {
                                        case (1):
                                            enemyTypeString = Globals.UnitType.Devil;
                                            break;
                                        case (2):
                                            enemyTypeString = Globals.UnitType.SoldierGun;
                                            break;
                                    }
                                    units.Add(Globals.cellListGlobal[enemyX, enemyY].CreateUnit(enemyX, enemyY,units.Count, enemyTypeString));
                                    units[units.Count - 1].DealDamage(enemyDam);
                                    Debug.WriteLine("");
                                }

                                break;

                            //
                            // CLIENT GET NEW PLAYERID
                            //
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
                            //
                            //DealDamageToEnemy
                            //
                        case "SHE":


                            int enemyIndex = Int32.Parse(commands[i].Substring(3, 3));
                            int shootingPlayer = Int32.Parse(commands[i].Substring(6, 2));
                            int damageDone = Int32.Parse(commands[i].Substring(8, 4));
                                
                            players[shootingPlayer].ChangeActionPoints(-1);
                            if (enemyIndex > 0)
                            {
                                this.units[enemyIndex - 1].DealDamage(damageDone);
                            }
                            CheckPlayerHasAP(shootingPlayer);
                                break;
                        //
                        //DealDamageToEnemy Powerup
                        //
                        case "SHW":


                        enemyIndex = Int32.Parse(commands[i].Substring(3, 3));
                        shootingPlayer = Int32.Parse(commands[i].Substring(6, 2));
                        damageDone = Int32.Parse(commands[i].Substring(8, 4));

                        players[shootingPlayer].ChangeActionPoints(-1);
                        if (enemyIndex > 0)
                        {
                            this.units[enemyIndex].DealDamage(damageDone);
                        }
                        break;

                            //
                            //SET COLOUR OF WALLS IF CLIENT
                            //
                            case "SEC":

                                if (!Globals.SinglePlayer && !server)
                                {
                                    Color[] colors = new Color[3];
                                    for (int colorPicker = 0; colorPicker < 3; colorPicker++)
                                    {
                                        colors[colorPicker] = Color.FromArgb(255, Int32.Parse(commands[i].Substring((colorPicker * 9) + 3, 3)), Int32.Parse(commands[i].Substring((colorPicker * 9) + 6, 3)), Int32.Parse(commands[i].Substring((colorPicker * 9) + 9, 3)));
                                        Debug.WriteLine("Colours for " + colorPicker + " are " + colors[colorPicker].R + "," + colors[colorPicker].G + "," + colors[colorPicker].B + ".");
                                    }
                                    Globals.drawColor = colors[0];
                                    Globals.floorColor = colors[1];
                                    Globals.roofColor = colors[2];
                                }
                                break;
                            //
                            //SET UP CLIENT WORLD DETAILS
                            ///SEP - Set Player-SEP[00 PlayerID][000 Player X Position][000 Player Y Position][char(1) Player direction][00 PlayerCharacterFile][0 Player AP] - Creates Player of set ID at set co-ords

                            //
                            case "SEP":

                                if (!Globals.SinglePlayer && !server)
                                {
                                    int playerID1 = Int32.Parse(commands[i].Substring(3, 2));
                                    int playerx1 = Int32.Parse(commands[i].Substring(5, 3));
                                    int playery1 = Int32.Parse(commands[i].Substring(8, 3));
                                    char d = Char.Parse(commands[i].Substring(11, 1));
                                    String playerName = "Player" + String.Format("{0:00}", commands[i].Substring(12, 2));
                                    int apToGive = Int32.Parse(commands[i].Substring(14, 1));
                                    Globals.Message = "Creating new Player " + playerID1 + " at " + playerx1 + "," + playery1 + " count is " + players.Count + ". Player has Skin " + playerName;
                                    Debug.WriteLine("Creating new Player " + playerID1 + " at " + playerx1 + "," + playery1 + " count is " + players.Count + ". Player has Skin " + playerName);
                                    if (this.players.Count == playerID1)
                                    {
                                        this.players.Add(new Player(playerx1, playery1, 1, playerID1, playerName));
                                        this.players[playerID1].ChangeActionPoints(-10);
                                        this.players[playerID1].ChangeActionPoints(apToGive);
                                        Directions dir = Directions.NULL;
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
                                        if (this.playerID != -1 && this.playerID != playerID1)
                                        {
                                            Globals.flags[5] = true;
                                            playerUnits.Add(Globals.cellListGlobal[playerx1, playery1].CreateUnit(playerx1, playery1,players.Count-2,Globals.UnitType.Player,"Resources/Images/Friendly/"+players[playerID1].playerFileName+"/"+ players[playerID1].playerFileName + "_Idle.png"));
                                            Globals.cellListGlobal[playerx1, playery1].SetPlayer(players[playerID1].GetPlayerID());
                                            Debug.WriteLine(this.thisClient.GetName() + ": Created new player @ " + playerx1 + "," + playery1);
                                        }
                                    }
                                    if (playerID != -1 && this.playerID == playerID1)
                                    {
                                        playerUnits.Add(null);
                                        this.thisPlayer = this.players[this.playerID];
                                        playerHud.ChangePlayer(thisPlayer);
                                        this.players[playerID1].SetID(this.playerID);
                                        Globals.cellListGlobal[playerx1, playery1].SetPlayer(players[playerID1].GetPlayerID());
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
                                    Globals.cellListGlobal = new Cell[Globals.cellSize, Globals.cellSize];
                                }
                                break;
                            case "DAL":
                                if (!server)
                                {
                                    Globals.pauseForInfo = false;
                                    RefreshPlayerView(thisPlayer.GetPlayerGun());
                                }
                                break;
                            default:
                                Debug.WriteLine("Invalid command: '" + commands[i].Substring(0, 3) + "'");
                                break;
                        }
                        #endregion
                    }

                }
            }
            commandString = String.Empty;

        }
        /// <summary>
        /// FOrm1_FormCLosing is ued to run 'tidy up' functions when the 'appliction.ext' command is run
        /// </summary>
        /// <param name="sender">The objec that sent the call</param>
        /// <param name="e">The cotext info of the call</param>
        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {

            if (this.thisClient != null)
            {
                if (this.server)
                {
                    thisClient.KillServer();
                }
                this.thisClient.CloseClient();
            }
            Application.Exit();
        }
        private void DrawMenu(Graphics g)
        {
            if (aboutOn)
            {
                g.DrawImage(Image.FromFile("Resources/Images/Menu/How-to.png"), new Point(0, 0));
                back.Draw(g);
               
            }
            else if (drawMapMakeMenu)
            {
                Debug.Write("\n debugging running drawMapMaker");
                g.DrawImage(Image.FromFile("Resources/Images/Title.png"), new Point(0, 0));
                MapDraw md = new MapDraw();
                md.PaintDrawing(g);
                back.Draw(g);
            }
            else
            {
                g.DrawImage(Image.FromFile("Resources/Images/Title.png"), new Point(0, 0));
                play.Draw(g);
                about.Draw(g);
                map.Draw(g);
            }
            

        }
        public void DrawCells(Graphics g)
        {
            DateTime now = DateTime.Now;
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
                int scanAcrossMax = ((maxDepth - loopFromBack) * 2) + 1;
                int halfScan = maxDepth - loopFromBack;
                while (loopFromLeft < scanAcrossMax /*baselineTargetSquare*/)
                {
                    {
                        int screenHeight = this.Height;
                        int screenWidth = this.Width;
                        //Create centreLine)
                        ///<summary>centerLine is the value of the Cell being drawn. 0= to the Left of screen, 1 = down the middle, 2= to the right of screen</summary>
                        int centreLine = 1;
                        //left
                        if (loopFromLeft < halfScan)
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
                            pickY = (thisPlayer.Gety() + 1) - (maxDepth - loopFromBack);
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
                            pickX = (thisPlayer.GetX() - 1) + (maxDepth - loopFromBack);
                        }
                        if (pickX >= 0 && pickX < Globals.cellListGlobal.GetLength(0) && pickY >= 0 && pickY < Globals.cellListGlobal.GetLength(1))
                        {
                            //if (Globals.cellListGlobal[pickX,pickY].GetisUnitPresent()== unitsa)
                            {
                                bool MatFront = false;
                                if (loopFromBack == maxDepth - 1 && loopFromLeft == 1 && Globals.cellListGlobal[pickX, pickY].GetMat())
                                {
                                    MatFront = true;
                                }
                                if (thisPlayer.dir == Directions.UP && pickX > 0 && centreLine == 2)
                                {
                                    if /*(Globals.cellListGlobal[pickX - 1, pickY].GetisUnitPresent() || */(Globals.cellListGlobal[pickX - 1, pickY].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                else if (thisPlayer.dir == Directions.DOWN && pickX < Globals.cellListGlobal.GetLength(0) - 1 && centreLine == 2)
                                {
                                    if /*(Globals.cellListGlobal[pickX + 1, pickY].GetisUnitPresent() ||*/(Globals.cellListGlobal[pickX + 1, pickY].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                else if (thisPlayer.dir == Directions.LEFT && pickY > 0 && centreLine == 2)
                                {
                                    if /*(Globals.cellListGlobal[pickX, pickY + 1].GetisUnitPresent() || */(Globals.cellListGlobal[pickX, pickY + 1].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                else if (thisPlayer.dir == Directions.RIGHT && pickY < Globals.cellListGlobal.GetLength(0) - 1 && centreLine == 2)
                                {
                                    if /*(Globals.cellListGlobal[pickX, pickY - 1].GetisUnitPresent() || */(Globals.cellListGlobal[pickX, pickY - 1].GetMat())
                                    {
                                        matToLeft = true;
                                    }
                                }
                                Globals.cellListGlobal[pickX, pickY].Draw(g, centreLine, loopFromBack, loopFromLeft, maxDepth, screenWidth, screenHeight, Globals.drawLines, matToLeft, MatFront);
                            }

                        }

                    }
                    loopFromLeft++;

                }
                loopFromBack++;
            }
            averageTimeToRunDrawing = averageTimeToRunDrawing.Add(now - DateTime.Now);
            averageTimeToRunDrawing = TimeSpan.FromMilliseconds(averageTimeToRunDrawing.TotalMilliseconds / 2);
            
        }
        /// <summary>
        /// FormUpdate is used to perform the graphical calcualtions for where various objects will appear on the screen, and call their
        /// respective drawing functions in order.
        /// FormUpdate Draws the new 'Frame' to a blank Graphics object, then draws that object onto the form
        /// </summary>
        /// <param name="unitsa">Whether to draw non-cell specfic items (Such as units or player views)</param>
        private void FormUpdate(bool unitsa = false,Graphics e = null){
            DateTime debugDate = DateTime.Now;
            RunCommands();
            Bitmap n = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(n);
            if (!Globals.pauseForInfo){
                if (cursorUp)
                {
                    //this.cursor.DebugPrint();
                    this.cursor.MoveCursor();
                }
                DrawCells(g);
                ///
                ///DRAW TEXT
                ///
                if (drawPrompt)
                {
                    this.textDisplay.GetImage(g, this.pfc);
                }
                ///
                ///DRAW DEBUG DATA
                ///
                if (Globals.drawText){
                    if (Globals.flags[0])
                    {

                        g.DrawString("DrawingMap! ", new Font(pfc.Families[0], 16), new SolidBrush(Color.Red), 0, 0);
                        g.DrawString("Your direction is : " + thisPlayer.dir, new Font(pfc.Families[0], 16), new SolidBrush(Color.Red), 0, 20);
                        g.DrawString("PlayerID : " + this.thisPlayer.GetPlayerID() + " PlayerSkin " + Globals.playerFileName, new Font(pfc.Families[0], 16), new SolidBrush(Color.Yellow), 0, 60);
                        

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


                //If Poqwerup in use draw powerup
                if (Globals.drawingPowerup<8)
                {
                    RefreshPlayerView(this.thisPlayer.GetPowerupImage(),Globals.drawingPowerup);
                    Globals.drawingPowerup++;
                }
                else
                {
                    //DrawGunShooting if gun is shooting
                    if (Globals.flags[1] == true)
                    {
                        RefreshPlayerView(this.thisPlayer.GetPlayerGun());
                        Globals.flags[1] = false;
                        Globals.flags[2] = true;
                    }
                    //Set gun back to normal
                    else
                    {
                        RefreshPlayerView(this.thisPlayer.GetPlayerGun());
                        Globals.flags[2] = false;
                    }
                    
                }
                if (Globals.drawGun)
                {
                    g.DrawImage(thisPlayer.playerView, new Point(this.Width - thisPlayer.playerView.Width - 10, this.Height - thisPlayer.playerView.Height - 40));

                }
                if(!lockPlayer){
                    if (cursorUp)
                    {
                        this.cursor.Draw(g);
                    }
                }
                
                if (Globals.drawHUD)
                {
                    playerHud.DrawHud(g, n.Width, n.Height);
                }

                if (thisPlayer.IsDead())
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(125, 255, 0, 0)), new RectangleF(0, 0, this.Width, this.Height));
                    g.DrawString("You Died", new Font(pfc.Families[0], 60), new SolidBrush(Color.Red), 0, (this.Height / 2) + 40);

                }
            }
            //If Loading Screen
            else{
                Image loadImage = Globals.ResizeImage(Image.FromFile("Resources/Images/LoadingScreen.png"), this.Width, this.Height);
                g.DrawImage(loadImage,0,0);
                Globals.WriteDebug("Form1.cs->UpdateForm()->PauseForInfoTrue",String.Format("Filling square {0},{1} with width/height {2}/{3}", this.Width / 2, this.Height - (this.Height / 15), (this.Width / 200) * loadingProgress, (this.Height / 15)),true);
                g.FillRectangle(new SolidBrush(Color.Aqua), new Rectangle(this.Width/2, this.Height - (this.Height / 15),(this.Width/200)*loadingProgress, (this.Height / 15)));

            }
            g.Dispose();
            e.DrawImage(n, 0, 0, n.Width, n.Height);
            //
            //START DEBUG TIMING
            //
                
            debugTimesTaken++;
            averageTimeToRun += debugDate.Subtract(DateTime.Now);
            averageTimeToRun = TimeSpan.FromMilliseconds(averageTimeToRun.Milliseconds / 2);
            if (debugTimesTaken % Globals.debugTimes == 0)
            {
                Globals.WriteDebug("Form1.cs -> UpdateForm() -> ", " \n -> \n -> \n ->" + Globals.debugTimes + " Debug Times Done, averge time is " + averageTimeToRun.TotalMilliseconds + " ms \n -> \n -> \n ->", true);
                Globals.WriteDebug("Form1.cs -> UpdateForm() -> ", " \n -> \n -> \n -> average  Drawtime is " + averageTimeToRunDrawing.TotalMilliseconds + " ms \n -> \n -> \n ->", true);
            }
            if (debugTimesTaken == 10000)
            {
                debugTimesTaken = 1;
            }
            //
            //END DEBUG TIMING
            //

        }
        /// <summary>
        /// RefreshPlayerView takes a given image array and sets the screens's first-person view  as the iamge in the array at Globals.aniamtions[0], 
        /// resized to the application size
        /// </summary>
        /// <param name="img">The bitmap iamge to be set as playerview</param>
        public void RefreshPlayerView(Bitmap[] img,int frame=-1)
        {
            if (frame <0)
            {
                frame = Globals.animations[0];
            }
            this.thisPlayer.playerView = img[frame];
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
                picker = Globals.cellListGlobal.GetLength(1)- thisPlayer.Gety() ;
            }
            else if (thisPlayer.dir == Directions.RIGHT)
            {
                picker = Globals.cellListGlobal.GetLength(0)-thisPlayer.GetX();
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
                if (x < Globals.cellListGlobal.GetLength(1) - 1)
                {
                    if (!Globals.cellListGlobal[x + 1, y].GetMat() && !Globals.cellListGlobal[x + 1, y].GetisUnitPresent())
                    {
                        Debug.WriteLine("CELL MOVE:Cell "+x+","+y+" Player ID was "+Globals.cellListGlobal[x,y].GetPlayer());
                        Globals.cellListGlobal[x, y].SetPlayer(-1);

                        Debug.WriteLine("CELL MOVE:Cell " + x + "," + y + " Player ID is Noe " + Globals.cellListGlobal[x, y].GetPlayer());

                        Debug.WriteLine("CELL MOVE:Cell " + (x +1)+ "," + y + " Player ID was " + Globals.cellListGlobal[x, y].GetPlayer()+"before move");
                        Globals.cellListGlobal[x + 1, y].SetPlayer(p.GetPlayerID());

                        Debug.WriteLine("CELL MOVE:Cell " + (x+1) + "," + y + " Player ID is now " + Globals.cellListGlobal[x, y].GetPlayer());
                        p.SetX(p.GetX() + 1);
                    }
                }
            }
            else
            {
                if (x > 1)
                {
                    if (!Globals.cellListGlobal[x - 1, y].GetMat() && !Globals.cellListGlobal[x - 1, y].GetisUnitPresent())
                    {
                        Globals.cellListGlobal[x, y].SetPlayer(-1);
                        Globals.cellListGlobal[x - 1, y].SetPlayer(p.GetPlayerID());
                        p.SetX(p.GetX() - 1);
                    }
                }
            }
        }
        /// <summary>
        /// ChangeY trys to change the player's y co-ordinate. This method deals with invalid movement
        /// </summary>
        /// <param name="up">States whether to move the palyer up. If false, player will move down</param>
        private void ChangeY(bool up, Player p)
        {
            int x = p.GetX();
            int y = p.Gety();
            if (up)
            {
                if (y > 1)
                {
                    if (!Globals.cellListGlobal[x, y - 1].GetMat() && !Globals.cellListGlobal[x, y - 1].GetisUnitPresent())
                    {
                        Globals.cellListGlobal[x, y].SetPlayer(-1);
                        Globals.cellListGlobal[x, y - 1].SetPlayer(p.GetPlayerID());
                        p.SetY(p.Gety() - 1);
                    }
                }

            }
            else
            {
                if (y < Globals.cellListGlobal.GetLength(1) - 1)
                {
                    if (!Globals.cellListGlobal[x, y + 1].GetMat() && !Globals.cellListGlobal[x, y + 1].GetisUnitPresent())
                    {
                        Globals.cellListGlobal[x, y].SetPlayer(-1);
                        Globals.cellListGlobal[x, y + 1].SetPlayer(p.GetPlayerID());
                        p.SetY(p.Gety() + 1);
                    }
                }
            }
        }
        private void DoPowerup()
        {
            if (Globals.SinglePlayer)
            {
                AddToCommandString("DPU" + String.Format("{0:00}", 0));
            }
            else
            {
                this.thisClient.Write("DPU" + String.Format("{0:00}", thisClient.GetID()));
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
        private void SendTurnOver()
        {
            ServerSend("ETP"+String.Format("{0:000}",this.playerID));
            Globals.WriteDebug("Form -> SendTurnOver()", "Sending Turn Over",true);

        }
        /// <summary>
        /// OnKeyUp handles key presses done while a <see cref="Form1"/> isntance is the active window
        /// </summary>
        /// <param name="sender">The object that sent this call </param>
        /// <param name="e">The arguments of this particular key press</param>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            bool servermade = false;
            if(!menu)
            {
                Directions playersdir = thisPlayer.dir;
            }
            Thread f = null;
            //Non-player Commands
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Application.Exit();
                    break;
                case Keys.Space:
                    SendTurnOver();
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
                case Keys.D5:
                    Globals.drawHUD = !Globals.drawHUD;
                    break;
                case Keys.D6:
                    Globals.drawOutline = !Globals.drawOutline;
                    break;
                case Keys.D7:
                    Globals.playMusic = !Globals.playMusic;
                    Globals.StopMusic();
                    break;
                //Server
                case Keys.O:
                    if (this.server == false)
                    {
                        this.server = true;
                        Globals.SinglePlayer = false;
                        f = new Thread(ThreadFunctions.ThreadRunnerServer);
                        f.IsBackground = true;
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
                        f.IsBackground = true; ;
                        object args1;
                        this.thisClient = new Client(Globals.port, Globals.Address, "The Client");
                        args1 = this.thisClient;
                        f.Start(args1);
                    }
                    break;
                case Keys.L:
                    Globals.SinglePlayer = false;

                    f = new Thread(ThreadFunctions.ClientThread);
                    f.IsBackground = true; 
                    object args;
                    Client cl = new Client(Globals.port, Globals.Address, "The Client2", 1);
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
                    //New Test Key
                case Keys.Left:
                    Globals.WriteDebug("Form1.cs-?OnKeyUp->LeftKey","Left Key Hit",true);
                    this.drawPrompt = !this.drawPrompt;
                    this.textDisplay = new TextDisplay("Test text to be displayed",this.Width,this.Height);
                    break;

            }
            //Only action if not Serverturn
            if (!lockPlayer)
            {
                switch (e.KeyCode)
                {
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
                    case Keys.R:
                        Globals.drawingPowerup = 0;
                        this.thisPlayer.GetPowerupSound().Play();
                        DoPowerup();
                        break;
                    case Keys.Q:
                        this.thisPlayer.RefreshGun();
                        this.thisPlayer.updateGunSize(this.Width/2,this.Height/2);
                        RefreshPlayerView(this.thisPlayer.GetPlayerGun());
                        break;
                    case Keys.U:
                        Globals.flags[0] = !Globals.flags[0];
                        for (int i = 3; i < Globals.flags.Length; i++)
                        {
                            Globals.flags[i] = false;
                        }
                        PrintMap();
                        break;
                }
                    
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
                    f.IsBackground = true; 
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
        /// A debug function used to print the state of the Form1 <see cref="Form1.Globals.cellListGlobal"/> at call
        /// </summary>
        private void PrintMap()
        {
            //Y
            for (int f = 0; f < Globals.cellListGlobal.GetLength(0); f++)
            {
                Debug.Write("\n\n");
                //X
                for (int i = 0; i < Globals.cellListGlobal.GetLength(1); i++)
                {
                    Debug.Write("[");
                    if (Globals.cellListGlobal[i,f].GetMat())
                    {
                        Debug.Write(" * ");
                    }
                    else if (thisPlayer.GetX() == i && thisPlayer.Gety() == f)
                    {
                        Debug.Write("<3 ");
                    }

                    else if (Globals.cellListGlobal[i, f].GetPlayer() > -1)
                    {
                        Debug.Write(" " + Globals.cellListGlobal[i, f].GetPlayer() + " ");
                    }
                    else if (Globals.cellListGlobal[i, f].GetisUnitPresent())
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
            int unitHit = 0;
            for (int i = 0; i < units.Count; i++)
            {
                Unit unitClicked = units[i];
                if (x > unitClicked.topLeft.X && x < unitClicked.bottomRight.X)
                {
                    if (y < unitClicked.bottomRight.Y && y > unitClicked.topLeft.Y)
                    {
                        Globals.WriteDebug("CursorHandling","Enemy ID is "+i,true);
                        if (unitClicked.alive == true)
                        {
                            Debug.WriteLine("Unit is alive, dealing damage");
                            Debug.WriteLine("Player is " + String.Format("{0:00}", this.playerID));
                            Debug.WriteLine("Gun Damage is " + String.Format("{0:0000}", thisPlayer.GetGunDamage()));
                            Debug.WriteLine("Enemy index i is " + String.Format("{0:000}", i));
                            Debug.WriteLine("Those 3 things together are " + String.Format("{0:000}", i) + "-" + String.Format("{0:00}", this.playerID) + "-" + String.Format("{0:0000}", thisPlayer.GetGunDamage()));
                            unitHit = i+1;
                            // unitClicked.DealDamage(thisPlayer.GetGunDamage());
                        }
                    }
                }

            }
            if (Globals.SinglePlayer)
            {
                AddToCommandString("SHE" + String.Format("{0:000}00{1:0000}", unitHit, thisPlayer.GetGunDamage()));
            }
            else
            {
                this.thisClient.Write("SHE" + String.Format("{0:000}", unitHit) + String.Format("{0:00}", this.playerID) + String.Format("{0:0000}", thisPlayer.GetGunDamage()));
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
            this.cursor = new CursorObject(new Bitmap("Resources/Images/Utility/Crosshair.png"),x,y, this.Height/(10 +thisPlayer.GetGunAccuracy()),50);
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
            Debug.Write("CLicking");
            if (menu)
            {
                if (drawMapMakeMenu)
                {
                    if (e.X > 50 && e.X < (Globals.cellSize * 10) + 50)
                    {
                        if (e.Y > 50 && e.Y < (Globals.cellSize * 10) + 50)
                        {
                            int gridX = (e.X - 50) / 10;
                            int gridY = (e.Y - 50) / 10;
                            if (Globals.cellListGlobal[gridX, gridY].GetMat())
                            {
                                Globals.cellListGlobal[gridX, gridY].setMat(false, Globals.drawColor);
                                DrawMenu(this.CreateGraphics());
                            }
                        }
                    }
                }
               
                if (e.X > play.x && e.X < play.x + play.length)
                {
                    if (e.Y > play.y && e.Y < play.y + play.length)
                    {
                        play.OnClick();
                    }
                }
                if (e.X > back.x && e.X < back.x + back.length)
                {
                    if (e.Y > back.y && e.Y < back.y + back.length)
                    {
                        back.OnClick();
                    }
                }
                if (e.X > about.x && e.X < about.x + about.length)
                {
                    if (e.Y > about.y && e.Y < about.y + about.length)
                    {
                        about.OnClick();
                    }
                }
                if (e.X > map.x && e.X < map.x + map.length)
                {
                    if (e.Y > map.y && e.Y < map.y + map.length)
                    {
                        map.OnClick();
                    }
                }
            }
            else if(!lockPlayer)
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
            
        }
        /// <summary>
        /// MouseMover is an eventlistener class used to create event on mouse move occur while <see cref="Form1"/> is the active window
        /// </summary>
        /// <param name="sender">The object that sends the MouseEvent call</param>
        /// <param name="e">The arguments of that particular MouseClick</param>
        private void MouseMover(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!menu && lockPlayer)
            {
                return;
            }
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
            else if (e.X < 30)
            {
                if (switcher != Directions.LEFT)
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
        /// <summary>
        /// ToggleFunc is a method to be used as a <see cref="DoomMenuItem"/> command.
        /// It is to toggle the 'aboutOn' paramter and as such change what image is displayed in the menu.
        /// Also redraws the menu screen. 
        /// </summary>
        public void ToggleFunc()
        {
           aboutOn = true;
            
            Debug.WriteLine("aboutOn is " + aboutOn.ToString());
            this.DrawMenu(this.CreateGraphics());
        }
        public void ToggleMapFunc()
        {
            drawMapMakeMenu = true;
            Debug.WriteLine("drawMapMakeMenu is " + drawMapMakeMenu.ToString());
            this.DrawMenu(this.CreateGraphics());
        }
        public void BackMenu()
        {
            if (drawMapMakeMenu || aboutOn)
            {
                drawMapMakeMenu = false;
                aboutOn = false;
            }
            Debug.WriteLine("drawMapMakeMenu is " + drawMapMakeMenu.ToString());

            Debug.WriteLine("aboutOn is " + aboutOn.ToString());
            this.DrawMenu(this.CreateGraphics());
        }
    }
  
    public enum Directions
    {
        UP,DOWN,LEFT,RIGHT,NULL
    }

    
    
}
