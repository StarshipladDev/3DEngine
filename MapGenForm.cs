using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        Random rand = new Random();
        Cell[,] cellList;
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
        public void DrawMap(object sender, EventArgs e)
        {
            draw = true;
            Refresh();
        }
        public void paintDrawing(object sender, PaintEventArgs e)
        {
            if (draw)
            {

                MessageBox.Show("Running Command!");
                Graphics g = e.Graphics;
                for (int i = 0; i < cellList.GetLength(0); i++)
                {
                    for (int f = 0; f < cellList.GetLength(1); f++)
                    {
                        
                        Color c = cellList[i,f].GetCellColor();
                        int x = (i * 5) + 50;
                        int y = (f * 5) + 50;
                        g.FillRectangle(new SolidBrush(c), new Rectangle(x, y, 50, 50));
                        Debug.WriteLine("Drawing Color with G " + c.G + " , at " + x + "," + y);

                    }
                }
               // g.Dispose();
            }
            else
            {
                MessageBox.Show("Drawing trash!");
                Graphics g = e.Graphics;
                g.FillEllipse(new SolidBrush(Color.Brown), 0, 0, 500, 500);
            }
           
        }
        public void  InitializeComponent()
        {
            this.cellList = new Cell[20, 20];
            for(int i=0; i < cellList.GetLength(0); i++)
            {
                for(int f= 0; f < cellList.GetLength(1); f++)
                {
                    cellList[i, f] = new Cell(true,Color.FromArgb(255,125,125,180), Color.FromArgb(255,0,rand.Next(204)+50,rand.Next(204)+50), Color.FromArgb(255, 125, 125, 180));
                }
            }
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
        }
    }
}
