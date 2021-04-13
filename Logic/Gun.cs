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
         * !!!!!!!File names!!!!
        444 is testing for all
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
        public int scope = 0;
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
        Bitmap[] animation;
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

        Color newThirdColor;
        Color newAltColor;
        Color  newColor;
        System.Media.SoundPlayer player;
        /// <summary>
        /// Gun constructor takes a numerical code for rach component and then stores the image array of the created weapon bitmaps.
        /// </summary>
        /// <param name="camo_Code"></param>
        /// <param name="bullet_Type"></param>
        /// <param name="damage_Rate"></param>
        /// <param name="ammo_Cap"></param>
        /// <param name="uniform_Code"></param>
        /// <param name="armour_Code"></param>
        /// <param name="race_Code"></param>
        /// <param name="style_Code"></param>
        /// <param name="sight_Code"></param>
        /// <param name="grip_Type"></param>
        /// <param name="damage"></param>
        public Gun(String camo_Code, String bullet_Type, String damage_Rate, String ammo_Cap, String uniform_Code, String armour_Code, String race_Code, String style_Code, String sight_Code, String grip_Type, int damage)
        {
            this.damage = damage ;
            this.camo_Code = camo_Code; // 000-Universal // 001- Tiger Strike // 002-Pinky
            this.bullet_Type = bullet_Type; // 000-Universal // 001 - Bullets //002 Big Bullet
            this.damage_Rate = damage_Rate; // 000-Universal // 001 -Standard  //001 - Sileneced
            this.ammo_Cap = ammo_Cap; // 000 20-30 Rounds // 001 - 60-100 // 002 -1-5
            this.uniform_Code = uniform_Code; // 000 - Universal // 001 - Green Camo // 002 - Navy 
            this.armour_Code = armour_Code; // 000-No Armour
            this.race_Code = race_Code;// 000-Human
            this.style_Code = style_Code;// 000- Ungloved White
            this.sight_Code = sight_Code;// 000 - Irons/No scope // 001 -Red dot
            this.grip_Type = grip_Type;
            //this.buildGun();
            animation = BuildGunAnimated();
        }

        /// <Author> Starshipladdev </Author>
        /// <Updated12/01/2021 </Updated>
        /// 
        /// <summary>buildGunAnimated uses <see cref="setComponent"/> to Find the 8 Frame images of each 
        /// gun part, and then combines them 8 times into 8 unique frames so that there is an array to animate.
        /// </summary>
        /// <returns> An array of each Bitmap frame of the combined animated gun </returns>
        private Bitmap[] BuildGunAnimated()
        {
            Random rand = new Random();
             newColor = Color.FromArgb(255, rand.Next(200) + 20, rand.Next(200) + 20, rand.Next(200) + 20);
            newAltColor = Color.FromArgb(255, newColor.R + 30, newColor.G + 30, newColor.B + 30);
            newThirdColor = Color.FromArgb(255, newColor.R - 20, newColor.G -20, newColor.B - 20);
            Bitmap[] gunFrames= new Bitmap[8];
            setComponent("Barrel");
            //setComponent("Muzzle_Flare");
            setComponent("Hand");
            setComponent("Grip");
            setComponent("Ejection");
            setComponent("Rail");
            setComponent("Sight");
            setComponent("Front_Body");
            setComponent("Receiver");
            //setComponent("Token");
            setComponent("Arm");
            setComponent("Magazine");
            Graphics g = null;
            int layer = 0;
            for (int i = 0; i < gunFrames.Length; i++)
            {

                if (i - 4 >= 0)
                {
                    layer = 1;
                }
                picture = new Bitmap(arm.Width, arm.Height, PixelFormat.Format32bppPArgb);
                int boxSize = (arm.Height - (arm.Height % 20)) / 2;
                int yGrid = boxSize / 20;

                Debug.WriteLine(String.Format("Image is x, y {0},{1}",arm.Width,arm.Height));
                Rectangle rec = new Rectangle((((i % 4)+1)* yGrid) + (i % 4) * (boxSize), layer * (boxSize) + (layer+1)*yGrid,boxSize,boxSize);
                Debug.WriteLine(String.Format("Rectangle made with x,y {0},{1} and width/height {2}/{3}", rec.X, rec.Y, rec.Width, rec.Height));
                g = Graphics.FromImage(picture);
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
                picture = picture.Clone(rec, picture.PixelFormat);
                gunFrames[i] = picture;
            }
            player = new System.Media.SoundPlayer("Resources/Sound/Bang_"+bullet_Type+"_"+damage_Rate+".wav");

            g.Dispose();
            return gunFrames;

        }
        /// <summary>
        /// GetSound returns the <see cref="SoundPlayer"/> associated with this instance.
        /// This is to get a sound to play as required.
        /// </summary>
        /// <returns>The gun sound attatched to this Gun </returns>
        public System.Media.SoundPlayer GetSound()
        {
            return this.player;
        }
        /// <summary>
        /// GetImage returns the <see cref="Bitmap"/> array associated with this instance's animation.
        /// This is to have an iteratable array of animation frames to animate. 
        /// </summary>
        /// <returns>The frame array attatched to this Gun </returns>
        public Bitmap[] GetImage()
        {
            //return this.picture;
            return this.animation;
        }
        /// <summary>
        /// GetDamage returns damage value associated with this instance.
        /// This is to have the damage value associated wih the gun isntance. 
        /// </summary>
        /// <returns>The damage value assosiated with thi gun instance </returns>
        public int GetDamage()
        {
            return this.damage;
        }
        /// <summary>
        /// GetImageShoot is outdated as of 12/01/2021 due to animations
        /// </summary>
        /// <returns>The static bitmap image of the gun shooting</returns>
        public Bitmap GetImageShoot()
        {
            return this.animation[0];
        }
        /// <summary>
        /// UpdateGunSize is used to update the scale of all frames in 'animation'
        /// This is so all gun frames drawn are to the correct scale.
        /// This uses <see cref="Globals.ResizeImage(Image, int, int)"/>
        /// </summary>
        /// <param name="x">The width of the new scale</param>
        /// <param name="y">The height of the new scale</param>
        public void UpdateGunSize(int x, int y)
        {
            for (int i = 0; i < animation.Length; i++)
            {
                animation[i] = Globals.ResizeImage(animation[i], animation[i].Width*8,animation[i].Height*8);
            }
        }
        /// <summary>
        /// setComponent take a string name and finds relevant image files that are compatible wih the current Gun instance.
        /// This is to Select a valid image for randomly generating Gun classes.
        /// </summary>
        /// <param name="gunPart"> One of the compatible images for this Gun instance from the 'Resources' folder</param>
        private void setComponent(String gunPart)
        {
            scope = Int32.Parse(this.sight_Code);
            Random rand = new Random();
            String RunningPath = AppDomain.CurrentDomain.BaseDirectory;
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
                        if (fileName.Substring(firstVarStart, 3).Equals("000") || fileName.Substring(firstVarStart, 3).Equals(this.bullet_Type))
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
                        if (fileName.Substring(firstVarStart, 3).Equals("000"))
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
                    this.receiver = Globals.ReColorImage(new Bitmap(relevantImgs[numberPicked]), Color.FromArgb(255, 0, 0, 255), newColor, "TestGun.png");
                    this.receiver = Globals.ReColorImage(this.receiver, Color.FromArgb(255, 0, 0, 125), newAltColor, "TestGun1.png");

                    break;
                case "Ejection":
                    this.ejection = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Front_Body":
                   
                    this.front_Body = Globals.ReColorImage(new Bitmap(relevantImgs[numberPicked]), Color.FromArgb(255, 0, 0, 255), newColor, "TestGun.png");
                    this.front_Body = Globals.ReColorImage(this.front_Body, Color.FromArgb(255, 0, 0, 125), newAltColor, "TestGun.png");

                    break;
                case "Magazine":
                    this.magazine = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Grip":
                    this.grip = new Bitmap(relevantImgs[numberPicked]);

                    break;
                case "Arm":
                    this.arm = new Bitmap(relevantImgs[numberPicked]);
                    this.arm = Globals.ReColorImage(new Bitmap(relevantImgs[numberPicked]), Color.FromArgb(255, 0, 0, 255), newColor, "TestGun.png");
                    this.arm = Globals.ReColorImage(this.arm, Color.FromArgb(255, 0, 0, 125), newAltColor, "TestGun1.png");
                    this.arm = Globals.ReColorImage(this.arm, Color.FromArgb(255, 0, 0, 25), newAltColor, "TestGun1.png");

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
