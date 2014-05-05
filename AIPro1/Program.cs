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
            map.GenerateMap(1111, 10, 10, 20);

            var ai_team = new AI_team("Turbo",ConsoleColor.DarkRed,map);
            ai_team.AddUnit(2, 2);

            var temp_team = new AI_team("Dummy", ConsoleColor.Yellow, map);
            temp_team.AddUnit(27, 27);

            map.Teams.Add(ai_team);
            map.Teams.Add(temp_team);

            Console.WriteLine("Program Start");

            map.GameLoop();

            Console.WriteLine("Program over (e to exit)");
            while (true)
            {
                var input = Console.ReadLine();
                if (input.StartsWith("e")) break;
            }
        }
    }
}
