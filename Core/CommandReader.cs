using DoomCloneV2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace DoomCloneV2
{
    public class CommandReader
    {
        List<Player> players;
        Form1 f;
        Cell[,] cellList;
        List<Unit> playerUnits;
        List<Unit> units;
        public CommandReader(ref List<Player> players,Form1 f, ref Cell[,] cellList, ref List<Unit> playerUnits,ref List<Unit> units)
        {
            this.players = players;
            this.f = f;
            this.cellList = cellList;
            this.playerUnits = playerUnits;
            this.units = units;
        }
        public void MovePlayer(int playerID,String command, ref Client thisClient, ref String commandStringsNew, bool server)
        {
           

            int x = Int32.Parse(command.Substring(3, 2));
            if (x > -1 && x < players.Count)
            {
                int oldx = players[x].GetX();
                int oldy = players[x].Gety();
                Debug.WriteLine(" Moving Player " + x + String.Format(" From {0},{0} ", oldx, oldy));
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
                    int IdOfPlayer = cellList[oldx, oldy].GetUnitOnCell().GetId();
                    Debug.WriteLine("Removing player on " + oldx + "," + oldy + ", setting new player on" + players[x].GetX() + "," + players[x].Gety());
                    cellList[oldx, oldy].RemoveUnit();
                    Debug.WriteLine("Moving palyer's file name is " + "Resources/Images/Friendly/" + players[x].playerFileName + "/" + players[x].playerFileName + "_Idle.png");
                    playerUnits.Add(Globals.cellListGlobal[players[x].GetX(), players[x].Gety()].CreateUnit(players[x].GetX(), players[x].Gety(), IdOfPlayer, Globals.UnitType.Player, "Resources/Images/Friendly/" + players[x].playerFileName + "/" + players[x].playerFileName + "_Idle.png"));

                }

                //Ammo pickups: every client replays every "MVP" identically (see class-level notes
                //elsewhere in this architecture), so checking the mover's new cell here - regardless
                //of whether it's "our own" move or a remote player's - keeps pickups in sync across
                //all clients with no extra network opcode required.
                Globals.SecondaryPickupType pickup = cellList[players[x].GetX(), players[x].Gety()].GetPickupType();
                if (pickup != Globals.SecondaryPickupType.None)
                {
                    int ammoGranted = (pickup == Globals.SecondaryPickupType.GrenadeAmmo) ? 1 : 2;
                    Debug.WriteLine(" Player " + x + " picked up " + pickup + ", granting " + ammoGranted + " ammo");
                    players[x].AddSecondaryAmmo(ammoGranted);
                    cellList[players[x].GetX(), players[x].Gety()].RemovePickup();
                }

            }
        }

        public void ChangeDirection(int id,String command)
        {
            char directionChar = Char.Parse(command.Substring(5, 1));
            Debug.WriteLine("Rotating Player " + id + " " + directionChar);
            switch (directionChar)
            {
                case 'L':
                    players[id].ChangeDirection("Left");
                    break;
                case 'R':
                    players[id].ChangeDirection("Right");
                    break;
            }
        }
        /// <summary>
        /// PowerUp Is used to run the commands of a palyer pressing 'Q' and using a poweer up, 
        /// based on what time of powerup is used.
        /// </summary>
        /// <param name="id">The ID of the palyer using the power up</param>
        /// <param name="command"> The Command string that set off this action, see <see cref="Form1.RunCommands"/></param>
        /// <param name="powerupType">The enum of what powerup is being used, see <see cref="Player.PowerUpTypes"/></param>
        public void Powerup(int id, String command,Player.PowerUpTypes powerupType)
        {
            Debug.WriteLine(" PowerUpCommandReader Running");

            if (powerupType == Player.PowerUpTypes.GLOCK)
            {
                Debug.WriteLine(" Firing");

                Unit returnUnit = Globals.FindFirstUnit(players[id].GetX(), players[id].Gety(), players[id].GetDirection(), cellList);
                if (returnUnit != null && returnUnit.GetUnitType() != Globals.UnitType.Player)
                {
                    Debug.WriteLine(" Got a return Unit, adding damage");
                    this.f.commandStringsNew += "SHW" + String.Format("{0:000}{1:00}0020", returnUnit.GetId(), id) + "^";
                }
                else
                {
                    Debug.WriteLine(" No retun unit");
                }


            }
            else
            {
                Entity projReturn = Globals.FindFirstEntityInDistance(players[id].GetX(), players[id].Gety(), players[id].GetDirection(), cellList, 1, 2);
                if (projReturn != null)
                {
                    if (projReturn.type == Entity.EntityTypes.Projectile)

                    {

                        Debug.WriteLine(" Got a return Projectile " + projReturn.ToString() + ", Inverting");
                        Projectile projId = ((Projectile)projReturn);
                        projId.Invert();
                        this.f.commandStringsNew += "RPT001" + String.Format("{0:000}{1:000}", projId.sender.GetId(), projId.projectileId) + "^";
                    }
                    else if (projReturn.type == Entity.EntityTypes.Unit)
                    {
                        Debug.WriteLine(" Got a return Unit, adding damage");
                        Unit returUni = (Unit) projReturn;
                        this.f.commandStringsNew += "SHW" + String.Format("{0:000}{1:00}0020", returUni.GetId(), id) + "^";
                    }
                }
            }

            players[id].usingPowerUpFrame = false;
        }
        /// <summary>
        /// UseSecondary resolves a player's right-click secondary attack. The pistol is a
        /// straight-ahead hitscan (mirroring the GLOCK powerup's FindFirstUnit above); the
        /// grenade travels up to a fixed distance and deals area damage around its landing cell.
        /// Damage is reported via "DSH" rather than "SHE"/"SHW" because those opcodes also cost
        /// the shooter an action point per message - correct for gunfire, but wrong here since a
        /// single grenade can hit several units at once and the secondary is already gated by its
        /// own ammo/once-per-turn limits rather than action points.
        /// </summary>
        /// <param name="id">The ID of the player using their secondary.</param>
        /// <param name="secondaryType">Which secondary is equipped, see <see cref="Player.SecondaryType"/>.</param>
        public void UseSecondary(int id, Player.SecondaryType secondaryType)
        {
            Debug.WriteLine(" UseSecondary Running for player " + id + " with " + secondaryType);
            switch (secondaryType)
            {
                case Player.SecondaryType.PISTOL:
                    {
                        Unit hitUnit = Globals.FindFirstUnit(players[id].GetX(), players[id].Gety(), players[id].GetDirection(), cellList);
                        if (hitUnit != null && hitUnit.GetUnitType() != Globals.UnitType.Player)
                        {
                            Debug.WriteLine(" Pistol hit unit " + hitUnit.GetId());
                            this.f.commandStringsNew += "DSH" + String.Format("{0:000}{1:0000}", hitUnit.GetId(), 15) + "^";
                        }
                        break;
                    }
                case Player.SecondaryType.GRENADE:
                    {
                        System.Drawing.Point landing = Globals.FindGrenadeLandingCell(players[id].GetX(), players[id].Gety(), players[id].GetDirection(), cellList, 4);
                        //Full damage at the landing cell, half (splash) damage to the 8 cells around it.
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int cx = landing.X + dx;
                                int cy = landing.Y + dy;
                                if (cx < 0 || cy < 0 || cx >= cellList.GetLength(0) || cy >= cellList.GetLength(1))
                                {
                                    continue;
                                }
                                if (cellList[cx, cy].GetMat())
                                {
                                    continue;
                                }
                                Unit hitUnit = cellList[cx, cy].GetUnitOnCell();
                                if (hitUnit != null && hitUnit.GetUnitType() != Globals.UnitType.Player)
                                {
                                    int damage = (dx == 0 && dy == 0) ? 40 : 20;
                                    Debug.WriteLine(" Grenade hit unit " + hitUnit.GetId() + " for " + damage);
                                    this.f.commandStringsNew += "DSH" + String.Format("{0:000}{1:0000}", hitUnit.GetId(), damage) + "^";
                                }
                            }
                        }
                        break;
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
                Debug.WriteLine("Form1: "+ units[unitNumber].projs[projCounter].name + " returncode is :" + returnCode);

                if (returnCode > -2)

                {
                    Debug.WriteLine(units[unitNumber].projs[projCounter].name + "hit something with returnCode" + returnCode);
                    units[unitNumber].projs.RemoveAt(projCounter);
                    projCounter--;

                }
                if (returnCode > -1)
                {
                    Globals.WriteDebug("CommandReader()->RunProjectiles->",String.Format("unitNumber is {0} and projCounter is{1} as palyer {2:000} got hit",unitNumber,projCounter, returnCode),true);
                    String ProjectileHitString = "SHP" + String.Format("{0:000}{1:000}", returnCode, damage)+"^";
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
  