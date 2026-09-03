using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomCloneV2
{
    class Hud
    {

        Player player;
        Bitmap[] drawImageArray;
        //Cached GDI+ objects reused across DrawHud calls instead of allocating a new Font/Brush
        //every frame (DrawHud can be called many times a second while the game is running).
        static readonly Font hudFont = new Font("times", 20);
        static readonly SolidBrush healthBrush = new SolidBrush(Color.Red);
        static readonly SolidBrush apBrush = new SolidBrush(Color.Blue);
        static readonly SolidBrush secondaryBrush = new SolidBrush(Color.LightGreen);
        static readonly SolidBrush secondaryReadyBrush = new SolidBrush(Color.Yellow);
        static readonly SolidBrush hudBackBrush = new SolidBrush(Color.Gray);
        public Hud(ref Player newPlayer)
        {
            player = newPlayer;
            drawImageArray = new Bitmap[8];
            int x = 0;
            int y = 0;
            int size = 400;
            Bitmap fullImage = (Bitmap) Image.FromFile("Resources/Images/Friendly/"+newPlayer.playerFileName+"/"+newPlayer.playerFileName+"_Idle.png");
            for(int i=0; i < drawImageArray.Length; i++)
            {
                size = 400;
                x = (((i + 1) % 5) * 20) + (i % 4 * 400);
                y = 0;
                if (i >= (drawImageArray.Length/2))
                {
                    y = 1;
                }
                y= (((y + 1) % 5) * 20) + (y % 4 * 400);
                drawImageArray[i] = Globals.CropImage(fullImage,new Rectangle(x,y,size,size));
                x = drawImageArray[i].Width / 3;
                y = 0;
                size = size / 3;
                drawImageArray[i] = Globals.CropImage(drawImageArray[i], new Rectangle(x, y, size, size));
            }
        }

        public void ChangePlayer(Player p)
        {
            this.player = p;
        }
        public void DrawHud(Graphics g, int width = 100, int height = 100)
        {
            int x = width / 3;
            int y = height - drawImageArray[0].Height;
            g.FillRectangle(hudBackBrush, new Rectangle(0,y,width,height));
            g.DrawImage(drawImageArray[Globals.animations[0]], new Point(x, y));
            String stringToDraw;
            if (player.GetHealth() < 1)
            {
                stringToDraw = "</3 0";
            }
            else
            {
                stringToDraw = "<3 " + player.GetHealth();
            }
            g.DrawString(stringToDraw, hudFont, healthBrush, x - drawImageArray[0].Width, y + drawImageArray[0].Height / 2);
            stringToDraw = "Action Points:" + player.ChangeActionPoints(0);
            g.DrawString(stringToDraw, hudFont, apBrush, x + drawImageArray[0].Width, y+drawImageArray[0].Height/2);

            //Secondary ability indicator: which weapon is equipped, how much ammo is left, and
            //whether it's still available this turn (see Player.CanUseSecondary/ResetSecondaryTurn).
            String secondaryName = player.GetSecondaryType() == Player.SecondaryType.GRENADE ? "Grenade" : "Pistol";
            String readyText = player.CanUseSecondary() ? "READY" : "USED";
            stringToDraw = String.Format("{0} (RMB): {1} ammo [{2}]", secondaryName, player.GetSecondaryAmmo(), readyText);
            SolidBrush secondaryTextBrush = player.CanUseSecondary() ? secondaryReadyBrush : secondaryBrush;
            g.DrawString(stringToDraw, hudFont, secondaryTextBrush, x + drawImageArray[0].Width, y + drawImageArray[0].Height / 2 + 25);

        }

    }
}
