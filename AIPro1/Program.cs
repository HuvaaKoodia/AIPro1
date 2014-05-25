using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;
using AIpro_FSM.AI;

namespace AIpro_FSM
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(100, 60);

            //map setup
            var map = new Map(30, 30);
            map.DisableVictoryConditions = false;

            var ai_team = new AI_team("Turbo",1, ConsoleColor.DarkRed, map);
            map.Teams.Add(ai_team);

            var temp_team = new AI_team("Dummy",2, ConsoleColor.Yellow, map);
            map.Teams.Add(temp_team);

            map.GameLoop();
        }
    }
}
