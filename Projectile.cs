using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    public class Projectile:Entity
    {
        int targetX;
        int targetY;
        int xDirection=0;
        int yDirection=0;
        int damage = 5;
        bool xBigger = true;
        bool goingExcess=false;
        public Projectile()
        {

        }
        public void SetUpProjecticle(int x, int y, Bitmap unitImage,int targetX, int targetY,bool animated,int drawRatio,int sectionSize,int damage=5)
        {
            this.x = x;
            this.y = y;
            this.alive = false;
            this.unitImage = unitImage;
            this.returnImage = this.unitImage;
            this.animated = false;
            this.xDirection = targetX-x;
            this.yDirection = targetY-y;
            this.targetX = targetX;
            this.targetY = targetY;
            this.sectionSize = sectionSize;
            this.drawRatio = drawRatio;
            this.damage = damage;
            if (Math.Abs(yDirection)> Math.Abs(xDirection))
            {
                xBigger = false;
            }
            if (xDirection != 0)
            {

                xDirection = xDirection / Math.Abs(xDirection);
            }
            if(yDirection != 0)
            {

                yDirection = yDirection / Math.Abs(yDirection);
            }
            //offset = new Random().Next(8);
        }
        public int GetDamage()
        {
            return this.damage;
        }
        //Returns -1 if destroyed,-2 if Still going, int if palyerInt hit
        public int RunProjecticle(Cell[,] cellList)
        {
            Debug.WriteLine("Projectile went from "+x+","+y+" with target X,Y"+targetX+","+targetY);
            cellList[x, y].RemoveProjecticle();
            if (!goingExcess)
            {
                if (x != targetX)
                {
                    x += xDirection;
                }
                if (y!= targetY)
                {
                    y += yDirection;
                }
                if(y==targetY && x==targetX)
                {
                    goingExcess = true;
                }
            }
            else
            {
                y += yDirection;
                x += xDirection;
            }
            if(x>-1 && x<cellList.GetLength(0)&& y > -1 && y < cellList.GetLength(1))
            {

                cellList[x, y].SetProjectile(this);
                if (cellList[x, y].GetMat())
                {
                    cellList[x, y].RemoveProjecticle();
                    return -1;
                }
                if (cellList[x, y].GetPlayer() >= 0)
                {
                    Debug.WriteLine("Projectile arrived to " + x + "," + y + " hitting player " + cellList[x, y].GetPlayer());
                    cellList[x, y].RemoveProjecticle();
                    return cellList[x, y].GetPlayer();
                }
                Debug.WriteLine("Projectile arrived to " + x + "," + y);
                return -2;
            }
            return -1;
        }
    }
}
