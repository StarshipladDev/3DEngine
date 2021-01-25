using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    public class Cell
    {

        Color drawColor;
        Color floorColor;
        Color roofColor;
        bool mat = false;
        bool isUnitOnCell = false;
        int playerOnCellIndex = -1;
        int type = 0;
        Unit unitOnCell = null;
        Projectile projOnCell = null;
        Corpse corpseOnCell = null;
        public Cell(bool mat, Color drawColor, Color floorColor, Color roofColor)
        {
            this.mat = mat;
            if (mat)
            {
                this.type = 1;
            }
            this.drawColor = drawColor;
            this.floorColor = floorColor;
            this.roofColor = roofColor;
        }
        public void SetPlayer(int playerIndex)
        {
            this.playerOnCellIndex = playerIndex;
        }
        public int GetPlayer()
        {
            return this.playerOnCellIndex;
            
        }
        public Color GetCellColor()
        {
            return this.floorColor;
        }
        public int GetCellType()
        {
            return type;
        }
        public void SetProjectile(Projectile p)
        {
            this.projOnCell = p;
        }
        public void RemoveProjecticle()
        {
            this.projOnCell = null;
        }
        public Projectile GetProjectile()
        {
            return this.projOnCell;
        }
        public void RemoveUnit()
        {
            this.isUnitOnCell = false;
            this.unitOnCell = null;
        }
        public Unit CreateUnit(int x, int y,Globals.UnitType ut = Globals.UnitType.Devil , String backupImage = null)
        {
            this.isUnitOnCell = true;
            if (ut == Globals.UnitType.Player)
            {
                this.unitOnCell = new Unit(backupImage);
                this.unitOnCell.setUpUnit(x, y, ut, true);

            }
            else
            {

                this.unitOnCell = new Unit("Resources/Images/Enemy/" + ut + "/" + ut + "_Idle.png");
                this.unitOnCell.setUpUnit(x, y, ut, false);

            }
            return this.unitOnCell;
        }

        public Corpse CreateCorpse(int x, int y, Globals.UnitType ut = Globals.UnitType.Devil, String backupDeathImage = null)
        {
            if (ut == Globals.UnitType.Player)
            {
                this.corpseOnCell = new Corpse(backupDeathImage, 400);
            }
            else
            {

                this.corpseOnCell = new Corpse("Resources/Images/Enemy/" + ut + "/" + ut + "_Death.png", 400);
            }
            return this.corpseOnCell;
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
        public void setMat(bool mat, Color color)
        {
            this.mat = mat;
            if (mat == true)
            {

                this.type = 1;
            }
            else
            {
                this.type = 0;
            }
            this.drawColor = color;
        }

        public void Draw(Graphics g, int centreLine, int loopFromBack, int loopFromLeft, int maxDepth, int screenLength, int screenHeight, bool drawLines, bool drawLeftMat, bool playerOn = false)
        {

            Point[] points;
            int drawLength = screenLength / (((maxDepth - loopFromBack) * 2) - 1);
            int drawHeight = screenHeight / (((maxDepth - loopFromBack) * 2) - 1);
            int drawLengthPrior = screenLength / (((maxDepth - loopFromBack + 1) * 2) - 1);
            int drawHeightPrior = screenHeight / (((maxDepth - loopFromBack + 1) * 2) - 1);
            int[] topLeft = new int[] { (drawLength * (loopFromLeft - 1)), screenHeight / 2 - (drawHeight / 2) };
            int[] topRight = new int[] { (drawLength * (loopFromLeft - 1)) + drawLength, screenHeight / 2 - (drawHeight / 2) };
            int[] bottomLeft = new int[] { drawLength * (loopFromLeft - 1), screenHeight / 2 - (drawHeight / 2) + drawHeight };
            int[] bottomRight = new int[] { drawLength * (loopFromLeft - 1) + drawLength, screenHeight / 2 - (drawHeight / 2) + drawHeight };
            int PriorLoop = loopFromLeft;
            int[] topLeftPrior = new int[] { drawLengthPrior * (PriorLoop), screenHeight / 2 - (drawHeightPrior / 2) };
            int[] bottomRightPrior = new int[] { drawLengthPrior * (PriorLoop) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2) + drawHeightPrior };
            int[] topRightPrior = new int[] { (drawLengthPrior * (PriorLoop)) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2) };
            int[] bottomLeftPrior = new int[] { drawLengthPrior * (PriorLoop), screenHeight / 2 - (drawHeightPrior / 2) + drawHeightPrior };

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
                        //MakefloorColor red if projectile on it;
                        Color drawColorOfFloor = floorColor;
                        if (this.projOnCell != null)
                        {
                            drawColorOfFloor = Color.FromArgb(125,255,0,0);
                        }
                        //Make floorColor Green if palyer on it;
                        if (this.GetPlayer()>-1 )
                        {
                            drawColorOfFloor = Color.FromArgb(125, 0,255, 0);
                        }
                        g.FillPolygon(new SolidBrush(drawColorOfFloor), points);
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
                        if ((isUnitOnCell) && unitOnCell!=null)
                        {
                            unitOnCell.Draw(g, drawLength, drawHeight, topLeft);
                        }
                        if (this.projOnCell != null)
                        {
                            projOnCell.Draw(g, drawLength, drawHeight, topLeft);
                        }
                        if(this.corpseOnCell != null)
                        {
                            corpseOnCell.Draw(g, drawLength, drawHeight, topLeft);
                        }
                    }


                }
            }

            if (drawLines )
            {
                //Draw outline
                g.DrawRectangle(new Pen(Color.Black), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2), drawLength, drawHeight);
                if (loopFromBack > 0)
                {
                    //Draw connectors
                    //top left
                    g.DrawLine(new Pen(Color.Black), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2),/*draw to */ drawLengthPrior * (loopFromLeft + 1), screenHeight / 2 - (drawHeightPrior / 2));
                    //top right
                    g.DrawLine(new Pen(Color.Black), (drawLength * (loopFromLeft)) + drawLength, screenHeight / 2 - (drawHeight / 2),/*draw to */ (drawLengthPrior * (loopFromLeft + 1)) + drawLengthPrior, screenHeight / 2 - (drawHeightPrior / 2));
                    //bottom left
                    g.DrawLine(new Pen(Color.Black), drawLength * (loopFromLeft), screenHeight / 2 - (drawHeight / 2) + drawHeight,/*draw to */ drawLengthPrior * (loopFromLeft + 1), screenHeight / 2 - (drawHeightPrior / 2) + drawHeightPrior);
                    //bottom right
                    g.DrawLine(new Pen(Color.Black), bottomRight[0], bottomRight[1], bottomLeft[0], bottomLeft[1]);

                }




            }
        }
    }
}
