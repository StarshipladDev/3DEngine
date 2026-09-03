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
        /// <summary>
        /// The right-click "Secondary" ability, chosen once by the player at the start of a
        /// session (see Form1's secondary-picker screen).
        /// </summary>
        public enum SecondaryType
        {
            PISTOL, GRENADE
        }
        //Max ammo/turn-flag are kept to a single decimal digit so they fit the existing
        //fixed-width network message format (see Form1.cs's SEP handling).
        public const int MaxSecondaryAmmo = 9;

        private int xPos { get; set; }
        private int yPos { get; set; }
        private int playerID = 0;
        private int actionPoints = 5;
        public int health = 20;
        private Gun playerGun;
        private Powerup power;
        private Secondary secondary;
        public Directions dir = Directions.UP;
        public Bitmap playerView;
        public String playerFileName;
        public bool usingPowerUpFrame = false;
        private PowerUpTypes powerUpType = PowerUpTypes.GLOCK;
        private SecondaryType secondaryType = SecondaryType.PISTOL;
        private int secondaryAmmo = 0;
        private bool secondaryUsedThisTurn = false;
        bool dead = false;

        public Player(int x, int y, int gunType,int iD,String playerFileName="Player01")
        {
            SetPowerUp(this.powerUpType);
            CreateGun();
            //Default secondary so `secondary` is never null before a player has actively chosen
            //one (e.g. remote players, or before the local picker screen runs).
            SetSecondaryType(this.secondaryType);
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
        /// <summary>
        /// Sets which secondary ability (grenade or pistol) this player has equipped, (re)builds
        /// its sprite with a fresh random camo recolor, and resets their ammo to that weapon's
        /// starting amount. Called once from the secondary-picker screen at the start of a
        /// session (see Form1.ChooseSecondary), and once with the default from the constructor
        /// so <see cref="secondary"/> is never null.
        /// </summary>
        public void SetSecondaryType(SecondaryType t)
        {
            this.secondaryType = t;
            CreateSecondary();
            this.secondaryAmmo = StartingSecondaryAmmo(t);
            this.secondaryUsedThisTurn = false;
        }
        private static int StartingSecondaryAmmo(SecondaryType t)
        {
            switch (t)
            {
                case SecondaryType.GRENADE:
                    //Grenades hit harder and are area-effect, so players start with fewer.
                    return 2;
                default:
                    return 4;
            }
        }
        private void CreateSecondary()
        {
            String name = this.secondaryType == SecondaryType.GRENADE ? "Grenade" : "Pistol";
            Random rand = new Random();
            Color newColor = Color.FromArgb(255, rand.Next(80,256), rand.Next(80,256), rand.Next(80,256));
            Color newAltColor = Color.FromArgb(255, rand.Next(80,256), rand.Next(80,256), rand.Next(80,256));
            Bitmap recolored = Globals.ReColorImage((Bitmap)Image.FromFile(String.Format("Resources/Images/Secondary/{0}/{0}.png", name)), Secondary.PrimaryMarker, newColor);
            recolored = Globals.ReColorImage(recolored, Secondary.SecondaryMarker, newAltColor);
            this.secondary = new Secondary(recolored, this.secondaryType);
        }
        public SecondaryType GetSecondaryType()
        {
            return this.secondaryType;
        }
        public Bitmap[] GetSecondaryImage()
        {
            return this.secondary.animationImages;
        }
        public System.Media.SoundPlayer GetSecondarySound()
        {
            return this.secondary.GetSecondarySound();
        }
        public int GetSecondaryAmmo()
        {
            return this.secondaryAmmo;
        }
        /// <summary>
        /// Directly sets the player's secondary ammo count (clamped to 0-<see cref="MaxSecondaryAmmo"/>).
        /// Used when syncing a snapshot of another player's state across the network - see the "SEP"
        /// handling in Form1.RunCommands.
        /// </summary>
        public void SetSecondaryAmmo(int value)
        {
            if (value < 0) value = 0;
            if (value > MaxSecondaryAmmo) value = MaxSecondaryAmmo;
            this.secondaryAmmo = value;
        }
        /// <summary>
        /// Adds (or removes, for a negative delta) ammo - used when a player walks over an ammo
        /// pickup. See CommandReader.MovePlayer.
        /// </summary>
        public void AddSecondaryAmmo(int delta)
        {
            SetSecondaryAmmo(this.secondaryAmmo + delta);
        }
        /// <summary>
        /// The secondary is gated by two independent hindrances, per design: a limited ammo pool
        /// that has to be resupplied from map pickups, and a once-per-turn cooldown.
        /// </summary>
        public bool CanUseSecondary()
        {
            return !this.dead && this.secondaryAmmo > 0 && !this.secondaryUsedThisTurn;
        }
        public void UseSecondaryCharge()
        {
            if (this.secondaryAmmo > 0)
            {
                this.secondaryAmmo--;
            }
            this.secondaryUsedThisTurn = true;
        }
        /// <summary>
        /// Clears the once-per-turn flag. Called alongside the existing action-point reset in
        /// Form1's "ETS" (end-of-turn) handling.
        /// </summary>
        public void ResetSecondaryTurn()
        {
            this.secondaryUsedThisTurn = false;
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
