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
        public bool Hungry { get { return Energy < HungerThreshold; } }//DEV. todo update hunger limit based on distance to food?

        public bool ForceLookForFood = false;
        public bool DEAD = false;
        public bool SenseDanger = false, UnderAttack = false;

        public bool NOFOODLEFT=false;

        public bool OnTheMove = false;
        public SearchNode Path;

        public int CommandType = 4, HungerThreshold=20;

        public Tile MoveTarget{ get;private set; }
        public Tile EatTarget { get; private set; }
        public Tile DigTarget { get; private set; }

        public Entity AttackTarget { get; private set; }
        public Entity ChaseTarget { get; private set; }

        private static int INDEX = 0;

        public AIEntity(Map mapref, Team team)
            : base(mapref, team)
        {
           ai = new AI();
           ai.Master = this;
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

            if (AttackTarget==null&&ChaseTarget != null)
            {
                //update path everytime
                SetMoveTargetCheckEnergy(ChaseTarget.CurrentTile);            
            }

            if (EatTarget != null)
            {
                EatFrom(EatTarget);

                if (EatTarget.Amount == 0)
                {
                    EatTarget = null;
                }
                else {
                    //DEV. lazy wrong place
                    //always eat till full, but only moderate wasting ok
                    if (ai.Master.Energy <= Entity.MaxEnergy - Entity.EnergyGainFromEating * 0.5f)
                        ai.Master.ForceLookForFood = true;
                    else
                        ai.Master.ForceLookForFood = false;
                }
                   
            }
            else if (DigTarget != null)
            {
                DigFrom(DigTarget);
                if (DigTarget.Amount == 0)
                {
                    DigTarget = null;
                }   
            }
            else if (AttackTarget != null)
            {
                Attack(AttackTarget);

                if (AttackTarget.Dead)
                {
                    AttackTarget = null;
                }

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

        public Entity LastAttacker=null;

        protected override void OnTakeDamage(Entity attacker) { 
            UnderAttack=true;
            LastAttacker=attacker;
        }

        public void UpdateBB(){
            ai.BB.AddOrSetValue("CommandType", CommandType);
            ai.BB.AddOrSetValue("Hungry", Hungry || ForceLookForFood);
            ai.BB.AddOrSetValue("SenseDanger", SenseDanger);
            ai.BB.AddOrSetValue("UnderAttack", UnderAttack);
        }

        public void ClearTargets() {
            EatTarget = DigTarget = MoveTarget =null;
            AttackTarget = null;
        }

        public void SetMoveTargetCheckEnergy(Tile target) {

            if (NOFOODLEFT) {
                HungerThreshold = 0;
                SetMoveTarget(target);
                return;
            }
            var distance = Math.Abs(CurrentTile.X - target.X) + Math.Abs(CurrentTile.Y - target.Y);
            int distance_to_food;
            Tile temp;
            GetClosestTile(Tile.Type.food,target, out temp, out distance_to_food);

            if (distance+distance_to_food > ai.Master.Energy)
            {
                //look for food.
                ai.Master.ForceLookForFood = true;
                Console.WriteLine("Not enough energy to move to target! Look for food!");
                return;
            }
            HungerThreshold = distance_to_food+2;
            SetMoveTarget(target);
        }

        public bool CanCloneCheckEnergy()
        {
            int distance_to_food;
            Tile temp;
            GetClosestTile(Tile.Type.food, CurrentTile, out temp, out distance_to_food);

            if (distance_to_food > ai.Master.Energy-Entity.CloneEnergyCost)
            {
                return false;
            }
            HungerThreshold = distance_to_food + 2;

            return true;
        }


        public void SetMoveTarget(Tile target)
        {
            ClearTargets();

            MoveTarget = target;
            SetPath(target);
        }

        void SetPath(Tile target) {
            Path = PathFinder.FindPath(MapReference, CurrentTile, target, -1);
        }

        public void SetEatTarget(Tile target)
        {
            if (!target.IsType(Tile.Type.food)) return;
            ClearTargets();

            EatTarget= target;
        }
        public void SetDigTarget(Tile target)
        {
            if (!target.IsType(Tile.Type.diamond)) return;
            ClearTargets();

            DigTarget = target;
        }

        public void SetAttackTarget(Entity target)
        {
            ClearTargets();

            AttackTarget = target;
        }

        public void SetChaseTarget(Entity target)
        {
            ChaseTarget = target;
        }

        public void ClearChaseTarget()
        {
            ChaseTarget=null;
        }

        public AreaInfo MyArea{get;private set;}
        public void SetAreaOfInterest(AreaInfo area) {
            MyArea = area;
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

        public bool GetClosestTile(Tile.Type type, Tile current,out Tile closest,out int distance){
            //find closest food tile, move to that
            closest = null;
            distance = 100000;

            int x = current.X;
            int y = current.Y;

            foreach (var t in MapReference.map_tiles)
            {
                if (t.TileType == type)
                {
                    var d = Math.Abs(t.X - x) + Math.Abs(t.Y - y);
                    if (d < distance)
                    {
                        distance = d;
                        closest = t;
                    }
                }
            }

            if (closest == null) {
                return false;
            }
            return true;
        }


        public int DistanceTo(Tile tile)
        {
            return Math.Abs(tile.X - X) + Math.Abs(tile.Y - Y);
        }

        //ai set up
        private void SetUpAI()
        {
            //states
            var S_idle = new Idle();
            var S_findfood = new FindFood();
            var S_hide = new Hide();
            var S_fight = new Fight();
            var S_danger = new Defend();

            //commands
            var SC_attack = new Attack();
            var SC_defend = new Defend();
            var SC_clone = new Clone();
            var SC_mine = new Mine();

            //transitions
            var to_findfood = new Transition(S_findfood);
            to_findfood.AddCriterion("Hungry", 1, Comparison.EQUAL);

            var to_idle_from_eat = new Transition(S_idle);
            to_idle_from_eat.AddCriterion("Hungry", 0, Comparison.EQUAL);

            var to_idle_from_hide = new Transition(S_idle);
            to_idle_from_hide.AddCriterion("SenseDanger", 0, Comparison.EQUAL);

            var to_hide = new Transition(S_hide);
            to_hide.AddCriterion("SenseDanger", 1, Comparison.EQUAL);

            var to_danger = new Transition(S_danger);
            to_danger.AddCriterion("UnderAttack", 1, Comparison.EQUAL);

            var to_idle_from_danger = new Transition(S_idle);
            to_idle_from_danger.AddCriterion("UnderAttack", 0, Comparison.EQUAL);

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
            to_idle_from_if_clo.AddCriterion("CommandType", 2, Comparison.EQUAL);

            var to_idle_from_if_def = new Transition(S_idle);
            to_idle_from_if_def.AddCriterion("CommandType", 3, Comparison.EQUAL);

            var to_idle_from_if_mine = new Transition(S_idle);
            to_idle_from_if_mine.AddCriterion("CommandType", 4, Comparison.EQUAL);

            //constructing the machine
            S_hide.Transitions.Add(to_idle_from_hide);
            S_findfood.AddTransition(to_idle_from_eat,to_danger);

            //commands
            S_idle.AddTransition(to_comm_attack, to_comm_clone, to_comm_defend, to_comm_mine);
            S_idle.IsSelector = true;

            SC_clone.AddTransition(to_hide, to_findfood, to_idle_from_if_att, to_idle_from_if_def, to_idle_from_if_mine,to_danger);
            SC_attack.AddTransition(to_findfood, to_idle_from_if_clo, to_idle_from_if_def, to_idle_from_if_mine, to_danger);
            SC_defend.AddTransition(to_findfood, to_idle_from_if_att, to_idle_from_if_clo, to_idle_from_if_mine, to_danger);
            SC_mine.AddTransition(to_findfood, to_idle_from_if_att, to_idle_from_if_clo, to_idle_from_if_def, to_danger);

            S_danger.AddTransition(to_idle_from_danger);

            //set up
            ai.SetState(S_idle);
        }


        public bool IsCommandAttacking()
        {
            return CommandType == 1;
        }

        public bool IsCommandCloning()
        {
            return CommandType == 2;
        }

        public bool IsCommandMining()
        {
            return CommandType == 4;
        }

        public void SetCommandAttack()
        {
            CommandType = 1;
        }

        public void SetCommandMine()
        {
            CommandType = 4;
        }

        public void SetCommandClone()
        {
            CommandType = 2;
        }


    }
}