using DoomCloneV2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace DoomCloneV2
{
    public class CommandReader
    {
        int i;
        List<Player> players;
        Form1 f;
        Cell[,] cellList;
        List<Unit> playerUnits;
        List<Unit> units;
        public CommandReader(ref List<Player> players,Form1 f, ref Cell[,] cellList, ref List<Unit> playerUnits,ref List<Unit> units)
        {
            this.i = i;
            this.players = players;
            this.f = f;
            this.cellList = cellList;
            this.playerUnits = playerUnits;
            this.units = units;
        }
        public void MovePlayer(int playerID,String command)
        {
            int x = Int32.Parse(command.Substring(3, 2));
            int oldx = players[x].GetX();
            int oldy = players[x].Gety();
            Debug.WriteLine(" Moving Player " + x + String.Format(" From {0},{0} ", oldx, oldy));
            if (x > -1 && x < players.Count)
            {
                char directionChar = Char.Parse(command.Substring(5, 1));
                switch (directionChar)
                {
                    case 'F':
                        f.Move("Forward", players[x]);
                        break;
                    case 'B':
                        f.Move("Back", players[x]);
                        break;
                    case 'L':
                        f.Move("Left", players[x]);
                        break;
                    case 'R':
                        f.Move("Right", players[x]);
                        break;
                }
                Debug.WriteLine(String.Format("To  {0},{1}.", players[x].GetX(), players[x].Gety()));
                if (x != playerID)
                {
                    Debug.WriteLine("Removing player on " + oldx + "," + oldy + ", setting new player on" + players[x].GetX() + "," + players[x].Gety());
                    cellList[oldx, oldy].RemoveUnit();
                    Debug.WriteLine("Moving palyer's file name is "+ "Resources/Images/Friendly/" + players[x].palyerFileName + "/" + players[x].palyerFileName + "_Idle.png");
                    playerUnits[x] = (cellList[players[x].GetX(), players[x].Gety()].createUnit(players[x].GetX(), players[x].Gety(), new Bitmap("Resources/Images/Friendly/"+ players[x].palyerFileName+"/"+ players[x].palyerFileName+"_Idle.png"), new Bitmap("Resources/Images/Friendly/"+ players[x].palyerFileName +"/"+players[x].palyerFileName+"_Death.png")));
                    if (cellList[oldx, oldy].GetisUnitPresent())
                    {
                        Debug.WriteLine("There IS a unit on the old coords " + oldx + "," + oldy);
                    }
                    if (cellList[players[x].GetX(), players[x].Gety()].GetisUnitPresent())
                    {
                        Debug.WriteLine("There IS a unit on the new coords " + oldx + "," + oldy);
                    }
                }
            }
        }
        public void RunProjectile(String command, ref Client thisClient,ref String commandStringsNew,bool server)
        {
            int projCount = Int32.Parse(command.Substring(3, 3));
            Debug.WriteLine(" Firing " + projCount + " Projectile's ");

            for (int projCounter = 0; projCounter < projCount; projCounter++)
            {
                Debug.WriteLine("------FIRING PROJECTILE--------");

                int unitNumber = Int32.Parse(command.Substring((projCounter * 6) + 6, 3));
                if (units[unitNumber].projs.Count <= projCounter)
                {
                    Debug.WriteLine("Unit had less projectiles than counter, ending");
                    break;
                }
                int damage = units[unitNumber].projs[projCounter].GetDamage();

                int returnCode = units[unitNumber].projs[projCounter].RunProjecticle(this.cellList);
                Debug.WriteLine("Form1: Fireball returncode is :" + returnCode);

                if (returnCode > -2)

                {
                    Debug.WriteLine("Fireball hit something with returnCode" + returnCode);
                    units[unitNumber].projs.RemoveAt(projCounter);
                    projCounter--;

                }
                if (returnCode > -1)
                {
                    Debug.WriteLine("Fireball hit player " + returnCode);
                    String ProjectileHitString = "SHP" + String.Format("{0:000}{1:000}", returnCode, damage);
                    if (!Globals.SinglePlayer && server)
                    {
                        Debug.WriteLine("Server sending " + ProjectileHitString);
                        thisClient.Write(ProjectileHitString);

                    }
                    if (Globals.SinglePlayer)
                    {
                        Debug.WriteLine("SinglePlayer sending " + ProjectileHitString);
                        commandStringsNew += ProjectileHitString;
                    }
                }
                Debug.WriteLine("------END FIRING PROJECTILE--------");
            }
        }
    }

    
    

}
  