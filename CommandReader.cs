using DoomCloneV2;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DoomCloneV2
{
    public class CommandReader
    {
        string[] commands;
        int i;
        List<Player> players;
        Form1 f;
        Cell[,] cellList;
        List<Unit> playerUnits;
        public CommandReader(ref string[] commands, ref int i, ref List<Player> players, ref Form1 f, ref Cell[,] cellList, ref List<Unit> playerUnits)
        {
            this.commands = commands;
            this.i = i;
            this.players = players;
            this.f = f;
            this.cellList = cellList;
            this.playerUnits = playerUnits;
        }
        public void MovePlayer()
        {
            int x = Int32.Parse(commands[i].Substring(3, 2));
            int oldx = players[x].GetX();
            int oldy = players[x].Gety();
            Debug.WriteLine(" Moving Player " + x + String.Format(" From {0},{0} ", oldx, oldy));
            if (x > -1 && x < players.Count)
            {
                char directionChar = Char.Parse(commands[i].Substring(5, 1));
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
                    playerUnits[x] = (cellList[players[x].GetX(), players[x].Gety()].createUnit(players[x].GetX(), players[x].Gety(), new Bitmap("Resources/Images/Friendly/Player1/Player1_Idle.png"), new Bitmap("Resources/Images/Friendly/Player1/Player1_Death.png")));
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
    }
    

}
  