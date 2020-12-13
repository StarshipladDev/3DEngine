using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    public class Gun
    {
        /*
         * muzzle_Flare -> Flare_[bullet Type]_[DamageRate]_[Name]
receiver -> Receiver_[Camo Code]_[DamageRate]_[Name]
ejection-> Ejection_[bullet Type]_[DamageRate]_[Name]
front_Body-> Front_Body_[Camo Code]_[bullet Type]_[Name]
barrel-> Barrel_[Camo Code]_[DamageRate]_[Name]
magazine-> Magazine_[bullet Type]_[Ammo_Cap]_[Name]
grip-> Grip_[Camo Code]_[Grip_Type]_[Name]
arm-> Arm_[Uniform Code]_[Armour_Code]_[Name]
hand-> Hand_[Race Code]_[Style Code]_[Name]
rail-> Rail_[Bullet Type]_[DamageRate]_[Name]
sight-> Receiver_[Camo Code]_[Sight_Code]_[Name]
         * */
        int damage;
        String camo_Code; // 000-Universal // 001- Tiger Strike // 002-Pinky
        String bullet_Type; // 000-Universal // 001 - Bullets //002 Rail
        String damage_Rate; // 000-Universal // 001 -Medium //002-Heavy
        String ammo_Cap; // 000 20-30 Rounds // 001 - 60-100 // 002 -1-5
        String uniform_Code; // 000 - Universal // 001 - Green Camo // 002 - Navy 
        String armour_Code; // 000-No Armour
        String race_Code;// 000-Human
        String style_Code;// 000- Ungloved White
        String sight_Code;// 000 - Irons/No scope // 001 -Red dot
        String grip_Type; // 000 - Medium Grip // 001 -Bipod // 002- L        Bitmap muzzle_Flare;
        Bitmap receiver;
        Bitmap ejection;
        Bitmap front_Body;
        Bitmap barrel;
        Bitmap magazine;
        Bitmap grip;
        Bitmap arm;
        Bitmap hand;
        Bitmap token;
        Bitmap rail;
        Bitmap sight;
        Bitmap muzzle_Flare;
        Bitmap picture;
        Bitmap pictureShoot;
        System.Media.SoundPlayer player;
        public Gun(String camo_Code, String bullet_Type, String damage_Rate, String ammo_Cap, String uniform_Code, String armour_Code, String race_Code, String style_Code, String sight_Code, String grip_Type, int damage)
        {
            this.damage = damage;
            this.camo_Code = camo_Code; // 000-Universal // 001- Tiger Strike // 002-Pinky
            this.bullet_Type = bullet_Type; // 000-Universal // 001 - Bullets //002 Rail
            this.damage_Rate = damage_Rate; // 000-Universal // 001 -Medium //002-Heavy
            this.ammo_Cap = ammo_Cap; // 000 20-30 Rounds // 001 - 60-100 // 002 -1-5
            this.uniform_Code = uniform_Code; // 000 - Universal // 001 - Green Camo // 002 - Navy 
            this.armour_Code = armour_Code; // 000-No Armour
            this.race_Code = race_Code;// 000-Human
            this.style_Code = style_Code;// 000- Ungloved White
            this.sight_Code = sight_Code;// 000 - Irons/No scope // 001 -Red dot
            this.grip_Type = grip_Type;
            this.buildGun();
        }
        private void buildGun()
        {
            setComponent("Barrel");
            setComponent("Muzzle_Flare");
            setComponent("Hand");
            setComponent("Grip");
            setComponent("Ejection");
            setComponent("Rail");
            setComponent("Sight");
            setComponent("Front_Body");
            setComponent("Receiver");
            /*
            setComponent("Token");
            */
            setComponent("Arm");
            setComponent("Magazine");
            picture = new Bitmap(arm.Width, arm.Height, PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(picture);
            g.DrawImage(this.barrel, new Point(0, 0));
            g.DrawImage(this.hand, new Point(0, 0));
            g.DrawImage(this.grip, new Point(0, 0));
            g.DrawImage(this.rail, new Point(0, 0));
            g.DrawImage(this.sight, new Point(0, 0));
            g.DrawImage(this.front_Body, new Point(0, 0));
            g.DrawImage(this.receiver, new Point(0, 0));

            g.DrawImage(this.ejection, new Point(0, 0));
            //g.DrawImage(this.token, new Point(0, 0));
            g.DrawImage(this.arm, new Point(0, 0));
            g.DrawImage(this.magazine, new Point(0, 0));
            g.Dispose();
            pictureShoot = new Bitmap(arm.Width, arm.Height, PixelFormat.Format32bppPArgb);
            g = Graphics.FromImage(pictureShoot);
            g.DrawImage(this.barrel, new Point(0, 0));
            g.DrawImage(this.muzzle_Flare, new Point(0, 0));
            g.DrawImage(this.hand, new Point(0, 0));
            g.DrawImage(this.grip, new Point(0, 0));
            g.DrawImage(this.rail, new Point(0, 0));
            g.DrawImage(this.sight, new Point(0, 0));
            g.DrawImage(this.front_Body, new Point(0, 0));
            g.DrawImage(this.receiver, new Point(0, 0));

            g.DrawImage(this.ejection, new Point(0, 0));
            //g.DrawImage(this.token, new Point(0, 0));
            g.DrawImage(this.arm, new Point(0, 0));
            g.DrawImage(this.magazine, new Point(0, 0));
            g.RotateTransform(20);
            g.Dispose();
            player = new System.Media.SoundPlayer("Resources/Sound/Bang_" + this.bullet_Type + "_" + this.damage_Rate + ".wav");
            Debug.WriteLine("Resources/Sound/Bang_" + this.bullet_Type + "_" + this.damage_Rate + ".wav");
        }

        public System.Media.SoundPlayer GetSound()
        {
            return this.player;
        }
        public Bitmap GetImage()
        {
            return this.picture;
        }
        public int GetDamage()
        {
            return this.damage;
        }
        public Bitmap GetImageShoot()
        {
            return this.pictureShoot;
        }
        private void setComponent(String gunPart)
        {
            Random rand = new Random();
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;

            String imgPath = RunningPath + "Resources\\Images\\Gun_Parts\\" + gunPart + "\\";
            String[] possibleImgs = System.IO.Directory.GetFiles(imgPath, "*.png");
            List<String> relevantImgs = new List<String>();
            for (int i = 0; i < possibleImgs.Count(); i++)
            {
                String fileName = possibleImgs[i].Substring(imgPath.Length, possibleImgs[i].Length - imgPath.Length);
                int firstVarStart = gunPart.Length + 1; // 1 '_' character before the first code
                int secondVarStart = gunPart.Length + 5;//2 '_' characters plus 3 numbers
                switch (gunPart)
                {
                    case "Barrel":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Muzzle_Flare":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Receiver":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Ejection":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Front_Body":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.bullet_Type))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Magazine":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.ammo_Cap))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Grip":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.grip_Type))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Arm":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.uniform_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.armour_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Hand":
                        if (fileName.Substring(firstVarStart, 3).Equals(this.race_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.style_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Rail":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals("000") || fileName.Substring(secondVarStart, 3).Equals(this.damage_Rate))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    case "Sight":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.camo_Code))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.sight_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    /*
                    case "Token":
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.grip_Type))
                        {
                            if (fileName.Substring(secondVarStart, 3).Equals(this.style_Code))
                            {
                                relevantImgs.Add(possibleImgs[i]);
                            }
                        }
                        break;
                    */
                    default:
                        break;
                }
            }
            int numberPicked = rand.Next(relevantImgs.Count());
            switch (gunPart)
            {
                case "Barrel":
                    this.barrel = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Muzzle_Flare":
                    this.muzzle_Flare = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Receiver":
                    this.receiver = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Ejection":
                    this.ejection = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Front_Body":
                    this.front_Body = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Magazine":
                    this.magazine = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Grip":
                    this.grip = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Arm":
                    this.arm = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Hand":
                    this.hand = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Rail":
                    this.rail = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Sight":
                    this.sight = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Token":
                    this.token = new Bitmap(relevantImgs[numberPicked]);

                    break;
                default:
                    Debug.WriteLine("Unkown Switch " + gunPart);
                    break;
            }


        }
    }
}
