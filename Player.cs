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
        private int xPos { get; set; }
        private int yPos { get; set; }
        private int playerID = 0;
        private Gun playerGun;
        public Directions dir = Directions.UP;
        public Bitmap playerView;
        public String playerFileName;
        public int health = 20;
        bool dead = false;

        public Player(int x, int y, int gunType,int iD,String playerFileName="Player01")
        {
            CreateGun();
            this.yPos = y;
            this.xPos = x;
            this.playerID = iD;
            this.playerFileName = playerFileName;

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
        private void CreateGun()
        {
            /*
            Random rand = new Random();
            String Camo = "00" + (rand.Next(3) + 2);
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
            String Camo = "444";
            String Damage = "444";
            String Sight = "444";
            String Grip =  "444";
            String Ammo = "444";
            int damage = rand.Next(21);
            Debug.WriteLine("New Gun with Camo/Damage: " + Camo + " \\ " + Damage);
            this.playerGun = new Gun(Camo, "444", Damage, Ammo, "444", "444", "444", "444", Sight, Grip, damage);
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
        public Bitmap[] GetPlayerGun()
        {
            return playerGun.GetImage();
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
    }
}
