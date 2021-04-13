using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    /// <summary>
    /// Unit is the class responsible for holding the information required to draw an object when viewed by the palyer,
    ///and also as an area to stroe unit-instance-specific information such as a damage and health
    /// </summary>
    public class Unit:Entity
    {
        Globals.UnitType unitType;
        private int health = 20;
        public String deadUnitImage;
        public List<Projectile> projs = new List<Projectile>();
        int id = 0;
        String m = "";
        public Unit(String m) : base(m)
        {
            this.m = m;
            this.type = EntityTypes.Unit;
        }

        /// <summary>
        /// setUpUnit is a constructor but like later
        /// </summary>
        /// <param name="unitImage">Provides a bitmap image to be read whenever the unit is 'alive'</param>
        /// <param name="deadUnitImage">Provides a bitmap image to be read/drawn wheenever the unit is 'dead'</param>
        public void setUpUnit(int x, int y, Globals.UnitType enemyType, bool friendly,int id)
        {
            this.x = x;
            this.y = y;
            this.unitType = enemyType;
            this.id = id;
            if(unitType == Globals.UnitType.Player)
            {
                friendly = true;
            }
            if (friendly)
            {
                this.unitImage = (Bitmap)Image.FromFile(m);
                this.deadUnitImage = m;
            }
            else
            {
                this.unitImage = (Bitmap)Image.FromFile("Resources/Images/Enemy/" + enemyType + "/" + enemyType + "_Idle.png");
                this.deadUnitImage = "Resources/Images/Enemy/" + enemyType + "/" + enemyType + "_Death.png";
            }
            this.returnImage = this.unitImage;
            offset = new Random().Next(8);
        }
        public void setUpUnit(int x, int y, Bitmap a, String b)
        {
            this.x = x;
            this.y = y;
            this.unitType = Globals.UnitType.Player;
            this.unitImage = a;
            this.deadUnitImage = b;
            
            this.returnImage = this.unitImage;
            offset = new Random().Next(8);
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
            this.unitImage = img;
        }

        public int GetId()
        {
            return this.id;
        }
        public Globals.UnitType GetUnitType()
        {
            return this.unitType;
        }
        public void Kill()
        {
            Debug.WriteLine("UNIT  + Unit changing to death animation");
            Globals.cellListGlobal[x, y].RemoveUnit();
            this.alive = false;
            Globals.cellListGlobal[x, y].CreateCorpse(x, y, unitType, deadUnitImage);
            if (unitType == Globals.UnitType.SoldierGun)
            {
                for(int i = 0; i < projs.Count(); i++)
                {
                    Globals.cellListGlobal[projs[i].x,projs[i].y].RemoveProjecticle();
                }
            }
        }
        /// <summary>
        /// DealDamage in an accessor method used to edit the unit's current health, and perform required changes to the unit's state
        /// such as making it's viewable image 'dead'
        /// </summary>
        public void DealDamage(int damage)
        {
            Debug.WriteLine(damage + " damage dealt to unit");
            this.health -= damage;
            if (this.health <= 0)
            {
                this.Kill();
            }
        }


        public override void Draw(Graphics g, int drawLength, int drawHeight, int[] topLefta)
        {
            base.Draw(g,drawLength,drawHeight,topLefta);
            if (alive)
            {
                Random rand = new Random();
                if (rand.Next(20) == 3 && alive)
                {
                    if (rand.Next(2) == 0)
                    {
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer("Resources/Sound/Boof.wav");
                        player.Play();
                        player.Dispose();
                    }
                    else
                    {
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer("Resources/Sound/Boof2.wav");
                        player.Play();
                        player.Dispose();
                    }

                }
            }
        }
        public void CreateProjectile(int targetX, int targetY)
        {
            if (this.x < 0 && this.y < 0) { return; }

            Debug.WriteLine("-------------");
            Debug.WriteLine("New projectile created firing at "+targetX+","+targetY);
            Debug.WriteLine("-------------");
            switch (unitType)
            {
                case Globals.UnitType.Devil:
                    Debug.WriteLine("-------------");
                    Debug.WriteLine("Proj's count of firing Unit is " + projs.Count());
                    Debug.WriteLine("-------------");
                    this.projs.Add(new Projectile("Resources/Images/Projectiles/Fireball/Flame.png", 100,this.projs.Count(),this));

                    Debug.WriteLine("-------------");
                    Debug.WriteLine("Proj's count of firing Unit is " + projs.Count()+" after creating, setting values , xand y of unit is"+this.x+","+this.y);
                    Debug.WriteLine("-------------");
                    this.projs[projs.Count-1].SetUpProjecticle(this.x,this.y,new Bitmap("Resources/Images/Projectiles/Fireball/Flame.png"), targetX,targetY,false,4,400,10);
                    break;
                case Globals.UnitType.SoldierGun:
                    Debug.WriteLine("-------------");
                    Debug.WriteLine("Proj's count of firing Unit is " + projs.Count());
                    Debug.WriteLine("-------------");
                    this.projs.Add(new Projectile("Resources/Images/Projectiles/Target/Target.png",400, this.projs.Count(), this));
                    Debug.WriteLine("-------------");
                    Debug.WriteLine("Proj's count of firing Unit is " + projs.Count() + " after creating, setting values , xand y of unit is" + this.x + "," + this.y);
                    Debug.WriteLine("-------------");
                    this.projs[projs.Count - 1].SetUpProjecticle(targetX, targetY, new Bitmap("Resources/Images/Projectiles/Target/Target.png"), targetX, targetY, true, 4, 400,5,true);
                    break;
            }
        }
        
    }
}
