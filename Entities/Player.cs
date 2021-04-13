using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    public class Player
    {
        public enum PowerUpTypes
        {
            GLOCK,KATANA
        }

        private int xPos { get; set; }
        private int yPos { get; set; }
        private int playerID = 0;
        private int actionPoints = 5;
        public int health = 20;
        private Gun playerGun;
        private Powerup power;
        public Directions dir = Directions.UP;
        public Bitmap playerView;
        public String playerFileName;
        public bool usingPowerUpFrame = false;
        private PowerUpTypes powerUpType = PowerUpTypes.GLOCK;
        bool dead = false;

        public Player(int x, int y, int gunType,int iD,String playerFileName="Player01")
        {
            SetPowerUp(this.powerUpType);
            CreateGun();
            this.yPos = y;
            this.xPos = x;
            this.playerID = iD;
            this.playerFileName = playerFileName;

        }

        public void SetPowerUp(PowerUpTypes p)
        {
            this.powerUpType = p;
            switch (p)
            {
                case PowerUpTypes.GLOCK:
                    CreatePowerup("PUGlock");
                    break;
                case PowerUpTypes.KATANA:
                    CreatePowerup("PUKatana");
                    break;
            }
        }
        public void doDamage(int damage)
        {
            this.health -= damage;
            if (this.health < 1)
            {
                this.dead = true;
                this.health = 0;

                Debug.Write("PLAYER:" + "Took damage, health is " + this.health + ", dead is " + this.IsDead());
            }
            
        }
        public bool IsDead()
        {
            return this.dead;
        }
        public void SetID(int id)
        {
            this.playerID = id;
        }
        public int GetPlayerID()
        {
            return this.playerID;
        }
        public int GetHealth()
        {
            return this.health;
        }
        public Powerup GetPowerup()
        {
            return this.power;
        }
        private void CreatePowerup(String powerUpType="PUGlock")
        {
            Globals.ReColorImage((Bitmap)Image.FromFile(String.Format("Resources/Images/PowerUp/{0}/{0}.png",powerUpType)),Color.FromArgb(255,255,0,225),Globals.katanaColor, String.Format("Resources/Images/PowerUp/{0}/{0}1.png", powerUpType)).Save((String.Format("Resources/Images/PowerUp/{0}/{0}1.png", powerUpType)));
            this.power = new Powerup((Bitmap)Image.FromFile(String.Format("Resources/Images/PowerUp/{0}/{0}1.png", powerUpType)),this.powerUpType);
        }
        private void CreateGun()
        {
            /*
            Random rand = new Random();
            String Damage = "00" + (rand.Next(2) + 1);
            String Sight = "00" + (rand.Next(2));
            String Grip = "00" + (rand.Next(3));
            String Ammo = "00" + (rand.Next(2));
            int damage = rand.Next(21);
            Debug.WriteLine("New Gun with Camo/Damage: " + Camo + " \\ " + Damage);
            this.playerGun = new Gun(Camo, "001", Damage, Ammo, "001", "000", "000", "000", Sight, Grip, damage);
            this.playerView = GetPlayerGun()[0];
            */

            Random rand = new Random();
            String Camo = "00" + (rand.Next(3) + 1);
            String Damage = "001";
            if (rand.Next(2) == 0)
            {
                Damage = "000";
            }
            String Sight = "00" + (rand.Next(2));
            String Grip = "000";
            String Ammo = "030";
            String Armour = "00" + rand.Next(2);
            String Bullet_Type = "001";
            if (rand.Next(2) == 0)
            {
                Bullet_Type = "002";
            }
            int damage = rand.Next(21);
            Debug.WriteLine("New Gun with Camo/Damage: " + Camo + " \\ " + Damage);
            this.playerGun = new Gun(Camo, Bullet_Type, Damage, Bullet_Type, "000",Armour, "000", "000", Sight, Grip, damage);
            this.playerView = GetPlayerGun()[0];
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
        public void updateGunSize(int x, int y)
        {
            this.playerGun.UpdateGunSize(x, y);
        }
        public int GetGunAccuracy()
        {
            return Int32.Parse(""+this.playerGun.scope);
        }
        public Bitmap[] GetPlayerGun()
        {
            return playerGun.GetImage();
        }
        public Bitmap[] GetPowerupImage()
        {
            return power.animationImages;
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
        public void SetDirection(Directions dir)
        {
            this.dir = dir;
        }
        public Directions GetDirection()
        {
            return this.dir;
        }

        public int ChangeActionPoints(int modify)
        {
            this.actionPoints += modify;
            if (actionPoints < 0)
            {
                actionPoints = 0;
            }
            if (actionPoints > 5)
            {
                actionPoints = 5;
            }
            return this.actionPoints;
        }
        public void ChangeDirection(String direction)
        {
            if (direction.Equals("Right"))
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
        public System.Media.SoundPlayer GetPowerupSound()
        {
            return this.power.GetPowerupSound();
        }
    }
}
