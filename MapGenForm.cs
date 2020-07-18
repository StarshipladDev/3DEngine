using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoomCloneV2
{
    class MapGenForm : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components = null;
        private Panel buttonPanel;
        private Panel drawingPanel;
        private bool draw = false;
        private Button createButton;
        private int maxNodes = 6;
        private int maxCells = 20;
        Point[] initalNodes;
        Random rand = new Random();
        bool[,] cellListSeePlayer;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        public MapGenForm()
        {
            InitializeComponent();
        }
        private Cell OpenCell(Cell cell,int x, int y,bool seePlayer)
        {

            cellListSeePlayer[x, y] = seePlayer;
            cell.setMat(false, Globals.cellListGlobal[x, y].GetCellColor());
            return cell;
        }
        public void DrawMap(object sender, EventArgs e)
        {
            ResetMap();
            cellListSeePlayer = new bool[20, 20];
            initalNodes = new Point[maxNodes];
            //Set player to be one of 'x' nodes and then mark that node as 'seeablePlayer'
            int x = rand.Next(20);
            int y = rand.Next(20);
            initalNodes[0] = new Point(x, y);
            OpenCell(Globals.cellListGlobal[x, y], x, y,true);
            //For Each MaxNode , set a random node to be non-mat 
            for(int i=1; i< maxNodes;)
            {
                bool goodToGo=true;
                int x2 = rand.Next(20);
                int y2 = rand.Next(20);
                for(int f = 0; f < i; f++)
                {
                    if (initalNodes[f].X==x2 || initalNodes[f].Y == y2)
                    {
                        goodToGo = false;
                    }
                }
                if (goodToGo)
                {

                    Globals.cellListGlobal[x2, y2].setMat(false, Globals.cellListGlobal[x, y].GetCellColor());
                    initalNodes[i] = new Point(x2,y2);
                    i++;
                }

            }
            //For each node, fill in the nearby nodes
            for (int i = 0; i < maxNodes;i++)
            {
                //Select Room Size
                bool isBigRoom = (rand.Next(4) == 0);
                int sizeOfRoom = 1;
                if (isBigRoom)
                {
                    sizeOfRoom = 2;
                }
                //X
                for(int f =-sizeOfRoom; f < sizeOfRoom+1; f++)
                {
                    //Y
                    for (int z = -sizeOfRoom; z < sizeOfRoom+1; z++){
                        if(f+initalNodes[i].X>=0 && f+initalNodes[i].X < maxCells)
                        {
                            if (z + initalNodes[i].Y >= 0 && z + initalNodes[i].Y < maxCells)
                            {
                                bool seePlayer = false;
                                if (i == 0)
                                {
                                    seePlayer = true;
                                }
                                OpenCell(Globals.cellListGlobal[f + initalNodes[i].X, z + initalNodes[i].Y], f + initalNodes[i].X, z + initalNodes[i].Y, seePlayer);
                            }
                        }
                    }

                }
            }

            //Connect each inital node with pathways
            Point currentPoint = initalNodes[0];
            for (int i=0; i < initalNodes.Length-1; i++)
            {
                //Go Vertical First
                if (rand.Next(2) == 0)
                {
                    currentPoint = DrawTunnel(currentPoint, initalNodes[i + 1], "vertical");
                    currentPoint = DrawTunnel(currentPoint, initalNodes[i + 1], "horizontal");
                }
                else
                {
                    currentPoint = DrawTunnel(currentPoint, initalNodes[i + 1], "horizontal");
                    currentPoint = DrawTunnel(currentPoint, initalNodes[i + 1], "vertical");
                }
            }
            draw = true;
            Refresh();
        }
        public Point DrawTunnel(Point startPoint,Point endPoint,String direction)
        {
            Debug.WriteLine("Drawing Tunnels");
            int goal;
            int start;
            if (direction.Equals("vertical"))
            {
                start = startPoint.Y;
                goal = endPoint.Y;
            }
            else
            {
                start = startPoint.X;
                goal = endPoint.X;
            }
            int difference = goal - start;
            bool goDown=false;
            if (difference < 0)
            {
                goDown = true;
                difference *= -1;
            }

            Debug.WriteLine("Difference is "+difference+" going "+direction);
            for (int f = 0; f < difference; f++)
            {
                int change = f;
                if (goDown)
                {
                    change *= -1;
                }
                if (direction.Equals("vertical"))
                {
                    int x = startPoint.X;
                    int y = startPoint.Y + change;
                    Debug.WriteLine("Tunnel Opening "+x+","+y+" to get to "+endPoint.ToString());
                    Globals.cellListGlobal[x,y ] = OpenCell(Globals.cellListGlobal[startPoint.X, startPoint.Y + change], startPoint.X, startPoint.Y + change, true);
                }
                else
                {

                    int x = startPoint.X + change;
                    int y = startPoint.Y ;
                    Debug.WriteLine("Tunnel Opening " + x + "," + y + " to get to " + endPoint.ToString());
                    Globals.cellListGlobal[startPoint.X + change, startPoint.Y] = OpenCell(Globals.cellListGlobal[startPoint.X + change, startPoint.Y], startPoint.X + change, startPoint.Y, true);
                }
            }
            if (direction.Equals("vertical"))
            {
                return new Point(startPoint.X,endPoint.Y);
            }
            else
            {
                return new Point(endPoint.X, startPoint.Y);
            }
        }
        public void paintDrawing(object sender, PaintEventArgs e)
        {
            if (draw)
            {
                Graphics g = e.Graphics;
                for (int i = 0; i < Globals.cellListGlobal.GetLength(0); i++)
                {
                    for (int f = 0; f < Globals.cellListGlobal.GetLength(1); f++)
                    {
                        
                        Color c = Globals.cellListGlobal[i,f].GetCellColor();
                        if (Globals.cellListGlobal[i, f].GetMat()==false)
                        {
                            c = Color.Red;
                        }
                        if (cellListSeePlayer[i,f] == true)
                        {
                            c = Color.Purple;
                        }
                        int x = (i * 10) + 50;
                        int y = (f * 10) + 50;
                        g.FillRectangle(new SolidBrush(c), new Rectangle(x, y, 10, 10));

                    }
                }
               // g.Dispose();
            }
           
        }
        public void ResetMap()
        {
            Globals.cellListGlobal = new Cell[20, 20];

            Color floorColor = Color.FromArgb(255, rand.Next(254), rand.Next(254), rand.Next(254));
            Color drawColor = Color.FromArgb(255, rand.Next(254), rand.Next(254), rand.Next(254));
            Color roofColor = Color.FromArgb(255, rand.Next(254), rand.Next(254), rand.Next(254));
            for (int i = 0; i < Globals.cellListGlobal.GetLength(0); i++)
            {
                for (int f = 0; f < Globals.cellListGlobal.GetLength(1); f++)
                {
                    Globals.cellListGlobal[i, f] = new Cell(true,drawColor,floorColor,roofColor);
                }
            }
        }
        public void  InitializeComponent()
        {
            ResetMap();
            this.Icon = new Icon("Resources/Images/MapMaker.ico");
            this.SuspendLayout();
            //BUtton
            this.createButton = new Button();
            this.createButton.Click += DrawMap;
            this.createButton.Size = new Size(100,25);
            this.createButton.Location = new Point(50, 50);
            this.createButton.Text = "Create Map";
            //BUTTONPANEL

            this.buttonPanel = new Panel();

            this.buttonPanel.BackColor = Color.Gray;
            this.buttonPanel.Size = new Size(500,200);
            this.buttonPanel.Location = new Point(0, 0);
            this.buttonPanel.Controls.Add(this.createButton);
            

            //DRAWINGPANEL
            this.drawingPanel = new Panel();
            this.drawingPanel.BackColor = Color.Black;
            this.drawingPanel.Size = new Size(500, 300);
            this.drawingPanel.Location = new Point(0, 200);
            this.drawingPanel.Paint +=new PaintEventHandler( paintDrawing);


            //FORM
            this.Text = "Map Maker Untitled Doom Clone";
            this.ClientSize = new System.Drawing.Size(500,500);
            this.Controls.Add(drawingPanel);
            this.Controls.Add(buttonPanel);
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.ResumeLayout();
            DrawMap(this, new EventArgs());
        }
    }
}
