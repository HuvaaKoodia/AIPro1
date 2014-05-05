using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    public class AIEntity : Entity
    {
        public AI ai { get; private set;}
        Random r = new Random();

        //game object data
        public bool Hungry { get { return energy < 20; } }//DEV. todo update hunger limit based on distance to food?

        public bool ForceLookForFood = false;
        public bool DEAD = false;
        public bool SenseDanger = false, UnderAttack = false;
        public bool Eat=false;

        public bool OnTheMove = false;
        public SearchNode Path;

        public int CommandType = 4;

        public Tile MoveTarget{ get;private set; }
        public Tile EatTarget { get; private set; }
        public Tile DigTarget { get; private set; }

        private static int INDEX = 0;

        public AIEntity(Map mapref, Team team)
            : base(mapref, team)
        {
           ai = new AI();
           ai.MasterEntity = this;
           ai.world = mapref;

           Name = "Entity " + INDEX;
           ++INDEX;

           SetUpAI();
        }

        public override void Update()
        {
            UpdateBB();

            Console.WriteLine("Command index: "+CommandType);
            ai.UpdateAI();

            if (EatTarget != null)
            {
                if (EatTarget.Amount == 0)
                {
                    EatTarget = null;
                }
                else
                    EatFrom(EatTarget);
            }
            else if (DigTarget != null)
            {
                if (DigTarget.Amount == 0)
                {
                    DigTarget = null;
                }
                else
                    DigFrom(DigTarget);
            }
            else if (MoveTarget != null)
            {
                if (Path == null || X == MoveTarget.X && Y == MoveTarget.Y)
                {
                    //destination reached
                    MoveTarget = null;
                }
                else if (X == Path.position.X && Y == Path.position.Y)
                {
                    Path = Path.next;
                }
                if (Path != null)
                {
                    var next = MapReference.GetTile(Path.position.X, Path.position.Y);
                    if (next.CanMoveTo())
                    {
                        MoveToPos(Path.position.X, Path.position.Y);
                    }
                    else
                    {
                        Path = null;
                    }
                }
            }
        }

        public void UpdateBB(){
            ai.BB.AddOrSetValue("CommandType", CommandType);
            ai.BB.AddOrSetValue("Eat",Eat);
            ai.BB.AddOrSetValue("Hungry", Hungry || ForceLookForFood);
            ai.BB.AddOrSetValue("SenseDanger", SenseDanger);
            ai.BB.AddOrSetValue("UnderAttack", UnderAttack);
        }

        public void clearTargets() {
            EatTarget = DigTarget = MoveTarget = null;
        }

        public void SetMoveTarget(Tile target) {
            clearTargets();

            MoveTarget= target;
            Path = PathFinder.FindPath(MapReference, CurrentTile, target, -1);
        }
        public void SetEatTarget(Tile target)
        {
            if (!target.IsType(Tile.Type.food)) return;
            clearTargets();

            EatTarget= target;
        }
        public void SetDigTarget(Tile target)
        {
            if (!target.IsType(Tile.Type.diamond)) return;
            clearTargets();

            DigTarget = target;
        }

        public override void GetInput(string input)
        {
            if (input.StartsWith("1")) CommandType = 1;
            if (input.StartsWith("2")) CommandType = 2;
            if (input.StartsWith("3")) CommandType = 3;
            if (input.StartsWith("4")) CommandType = 4;
        }

        public bool CloneSelf()
        {
            if (HasEnoughEnergyToClone())
            {
                var clone = new AIEntity(MapReference, MyTeam);
                PlaceCloneToFirstOpenNeigbourTile(clone);
                SpendCloningEnergy();
                return true;
            }
            return false;
        }

        //ai set up
        private void SetUpAI()
        {
            //states
            var S_idle = new Idle();
            var S_findfood = new FindFood();
            var S_hide = new Hide();
            var S_fight = new Fight();
            //commands
            var SC_attack = new Attack();
            var SC_defend = new Defend();
            var SC_clone = new Clone();
            var SC_mine = new Mine();

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
            to_comm_attack.AddCriterion("CommandType", 1, Comparison.EQUAL);

            var to_comm_clone = new Transition(SC_clone);
            to_comm_clone.AddCriterion("CommandType", 2, Comparison.EQUAL);

            var to_comm_defend = new Transition(SC_defend);
            to_comm_defend.AddCriterion("CommandType", 3, Comparison.EQUAL);

            var to_comm_mine = new Transition(SC_mine);
            to_comm_mine.AddCriterion("CommandType", 4, Comparison.EQUAL);

            var to_idle_from_if_att = new Transition(S_idle);
            to_idle_from_if_att.AddCriterion("CommandType", 1, Comparison.EQUAL);

            var to_idle_from_if_clo = new Transition(S_idle);
            to_idle_from_if_att.AddCriterion("CommandType", 2, Comparison.EQUAL);

            var to_idle_from_if_def = new Transition(S_idle);
            to_idle_from_if_att.AddCriterion("CommandType", 3, Comparison.EQUAL);

            var to_idle_from_if_mine = new Transition(S_idle);
            to_idle_from_if_mine.AddCriterion("CommandType", 4, Comparison.EQUAL);

            //SuperState transitions
            var to_danger = new Transition(SS_danger);
            to_danger.AddCriterion("UnderAttack", 1, Comparison.EQUAL);
            var to_work = new Transition(SS_work);
            to_work.AddCriterion("UnderAttack", 0, Comparison.EQUAL);

            //constructing the machine
            S_hide.Transitions.Add(to_idle_from_hide);
            S_findfood.AddTransition(to_idle_from_eat);

            //commands
            S_idle.AddTransition(to_comm_attack, to_comm_clone, to_comm_defend, to_comm_mine);
            S_idle.IsSelector = true;

            SC_clone.AddTransition(to_hide, to_findfood, to_idle_from_if_att, to_idle_from_if_def, to_idle_from_if_mine);
            SC_attack.AddTransition(to_findfood, to_idle_from_if_clo, to_idle_from_if_def, to_idle_from_if_mine);
            SC_defend.AddTransition(to_findfood, to_idle_from_if_att, to_idle_from_if_clo, to_idle_from_if_mine);
            SC_mine.AddTransition(to_findfood, to_idle_from_if_att, to_idle_from_if_clo, to_idle_from_if_def);

            //superstates
            SS_work.Transitions.Add(to_danger);
            SS_danger.Transitions.Add(to_work);

            SS_work.ChildState = S_idle;
            SS_danger.ChildState = S_fight;

            //parenting
            S_idle.ParentState = SS_work;
            S_findfood.ParentState = SS_work;
            S_hide.ParentState = SS_work;

            //SC_attack.ParentState = SS_work;
            //SC_defend.ParentState = SS_work;
            //SC_clone.ParentState = SS_work;

            S_fight.ParentState = SS_danger;

            //priorities
            S_idle.SetPriority(1);
            S_findfood.SetPriority(1);
            S_hide.SetPriority(1);
            S_fight.SetPriority(2);

            SC_attack.SetPriority(1);
            SC_clone.SetPriority(1);
            SC_defend.SetPriority(1);
            SC_mine.SetPriority(1);

            SS_work.SetPriority(1);
            SS_danger.SetPriority(2);

            //set up
            ai.SetState(SS_work);
        }
    }
}
