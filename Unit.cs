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
        private int health = 20;
        public Bitmap deadUnitImage;
        public List<Projectile> projs = new List<Projectile>();
        /// <summary>
        /// setUpUnit is a constructor but like later
        /// </summary>
        /// <param name="unitImage">Provides a bitmap image to be read whenever the unit is 'alive'</param>
        /// <param name="deadUnitImage">Provides a bitmap image to be read/drawn wheenever the unit is 'dead'</param>
        public void setUpUnit(int x, int y,Bitmap unitImage, Bitmap deadUnitImage)
        {
            this.x = x;
            this.y = y;
            this.unitImage = unitImage;
            this.deadUnitImage = deadUnitImage;
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
        public void Kill()
        {
            Debug.WriteLine("UNIT  + Unit changing to death animation");
            this.unitImage = deadUnitImage;
            this.dying = true;
            this.alive = false;
            this.offset = Globals.MAXFRAMES - Globals.animations[0];
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
        public void CreateProjectile(String projecticleType,int targetX, int targetY)
        {
            if (this.x < 0 && this.y < 0) { return; }

            Debug.WriteLine("-------------");
            Debug.WriteLine("New "+ projecticleType + " created firing at "+targetX+","+targetY);

            Debug.WriteLine("-------------");
            switch (projecticleType)
            {
                case "Fireball":
                    Debug.WriteLine("-------------");
                    Debug.WriteLine("Proj's count of firing Unit is " + projs.Count());
                    Debug.WriteLine("-------------");
                    this.projs.Add(new Projectile());

                    Debug.WriteLine("-------------");
                    Debug.WriteLine("Proj's count of firing Unit is " + projs.Count()+" after creating, setting values , xand y of unit is"+this.x+","+this.y);
                    Debug.WriteLine("-------------");
                    this.projs[projs.Count-1].SetUpProjecticle(this.x,this.y,new Bitmap("Resources/Images/Projectiles/Fireball/Flame.png"), targetX,targetY,false,4,100);
                    break;
            }
        }
        
    }
}
