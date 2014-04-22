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
            var map = new Map(30,30);
            map.GenerateMap(1111,10,10,20);

            var ai_team = new AI_team(map);
            var entity=ai_team.AddUnit(15, 15);

            Console.WriteLine("Program Start");

            //states
            var S_idle = new Idle();
            var S_findfood = new FindFood();
            var S_eatfood = new EatFood();
            var S_hide = new Hide();
            var S_fight = new Fight();
            //commands
            var SC_attack = new Attack();
            var SC_defend = new Defend();
            var SC_clone = new Clone();

            var SS_work = new State();
            var SS_danger = new State();

            //transitions
            var to_findfood = new Transition(S_findfood);
            to_findfood.AddCriterion("Hungry", 1, Comparison.EQUAL);

            var to_idle_from_eat = new Transition(S_idle);
            to_idle_from_eat.AddCriterion("Hungry", 0, Comparison.EQUAL);

            var to_idle_from_hide = new Transition(S_idle);
            to_idle_from_hide.AddCriterion("SenseDanger", 0, Comparison.EQUAL);

            var to_hide = new Transition(S_hide);
            to_hide.AddCriterion("SenseDanger", 1, Comparison.EQUAL);

            //command transitions
            var to_comm_attack = new Transition(SC_attack);
            to_comm_attack.AddCriterion("CommandType", 1,Comparison.EQUAL);

            var to_comm_clone = new Transition(SC_clone);
            to_comm_clone.AddCriterion("CommandType", 2, Comparison.EQUAL);

            var to_comm_defend = new Transition(SC_defend);
            to_comm_defend.AddCriterion("CommandType", 3, Comparison.EQUAL);

          
            var to_idle_from_if_att = new Transition(S_idle);
            to_idle_from_if_att.AddCriterion("CommandType", 1, Comparison.EQUAL);

            var to_idle_from_if_clo = new Transition(S_idle);
            to_idle_from_if_att.AddCriterion("CommandType", 2, Comparison.EQUAL);

            var to_idle_from_if_def = new Transition(S_idle);
            to_idle_from_if_att.AddCriterion("CommandType", 3, Comparison.EQUAL);

            //SuperState transitions
            var to_danger = new Transition(SS_danger);
            to_danger.AddCriterion("UnderAttack", 1, Comparison.EQUAL);
            var to_work = new Transition(SS_work);
            to_work.AddCriterion("UnderAttack", 0, Comparison.EQUAL);

            //constructing the machine
            S_hide.Transitions.Add(to_idle_from_hide);
            S_findfood.AddTransition(to_idle_from_eat);

            //commands
            S_idle.AddTransition(to_comm_attack, to_comm_clone,to_comm_defend);
            S_idle.IsSelector = true;

            SC_clone.AddTransition(to_hide, to_findfood, to_idle_from_if_att,to_idle_from_if_def);
            SC_attack.AddTransition(to_findfood, to_idle_from_if_clo, to_idle_from_if_def);
            SC_defend.AddTransition(to_findfood, to_idle_from_if_att, to_idle_from_if_clo);

            //superstates
            SS_work.Transitions.Add(to_danger);
            SS_danger.Transitions.Add(to_work);

            SS_work.ChildState = S_idle;
            SS_danger.ChildState = S_fight;

            //parenting
            S_idle.ParentState = SS_work;
            S_findfood.ParentState = SS_work;
            S_eatfood.ParentState = SS_work;
            S_hide.ParentState = SS_work;

            //SC_attack.ParentState = SS_work;
            //SC_defend.ParentState = SS_work;
            //SC_clone.ParentState = SS_work;

            S_fight.ParentState = SS_danger;

            //priorities
            S_idle.SetPriority(1);
            S_findfood.SetPriority(1);
            S_eatfood.SetPriority(1);
            S_hide.SetPriority(1);
            S_fight.SetPriority(2);

            SC_attack.SetPriority(1);
            SC_clone.SetPriority(1);
            SC_defend.SetPriority(1);

            SS_work.SetPriority(1);
            SS_danger.SetPriority(2);

            //set up entities
            entity.ai.SetState(SS_work);

            //program
            while (true)
            { 
                for (int e = map.GameEntities.Count-1; e >= 0; --e)
                {
                    map.DrawMap();

                    var ent = map.GameEntities[e];

                    Console.WriteLine("Input:\n-e to exit\n-anykey to continue");
                    var input = Console.ReadLine();
                    if (input.StartsWith("e")) break;

                    if (input.StartsWith("1")) entity.CommandType= 1;
                    if (input.StartsWith("2")) entity.CommandType = 2;
                    if (input.StartsWith("3")) entity.CommandType = 3;

                    Console.WriteLine("Turn " + map.Turn);

                    //updates
                    entity.Update();
                    entity.LateUpdate();

                    if (entity.Dead) map.GameEntities.Remove(entity);
                }

                if (map.GameEntities.Count == 0) break;
            }

            Console.WriteLine("Program over (e to exit)");
            while (true)
            {
                var input = Console.ReadLine();
                if (input.StartsWith("e")) break;
            }
        }
    }
}
