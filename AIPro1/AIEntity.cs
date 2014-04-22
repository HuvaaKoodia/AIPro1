using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    public class AIEntity : Entity
    {
        public AI ai { get; private set; }
        Random r = new Random();

        //game object data
        public bool Hungry { get { return energy < 20; } }

        public bool DEAD = false;
        public bool SenseDanger = false, UnderAttack = false;
        public bool Eat=false;

        public bool OnTheMove = false;
        public SearchNode Path;

        public int CommandType = 1;

        public Tile MoveTarget{ get;private set; }
        public Tile EatTarget { get; private set; }
        public Tile DigTarget { get; private set; }

        public AIEntity(Map mapref, Team team)
            : base(mapref, team)
        {
           ai = new AI();
           ai.MasterEntity = this;
           ai.world = mapref;
        }
        
        public override void Update()
        {
            UpdateBB();

            ai.UpdateAI();

            if (EatTarget != null) {
                EatFrom(EatTarget);
            }
            else if (DigTarget != null) {
                DigFrom(DigTarget);
            }
            else if (MoveTarget != null)
            {
                if (X == Path.position.X && Y == Path.position.Y)
                {
                    Path = Path.next;
                }
                if (Path != null)
                {
                    var next = MapReference.GetTile(Path.position.X, Path.position.Y);
                    if (next.IsType(Tile.Type.empty))
                    {
                        SetPosition(Path.position.X, Path.position.Y);
                    }
                    else {
                        Path = null;
                    }
                }
            }
        }

        public void UpdateBB(){
            ai.BB.AddOrSetValue("CommandType", CommandType);
            ai.BB.AddOrSetValue("Eat",Eat);
            ai.BB.AddOrSetValue("Hungry", Hungry);
            ai.BB.AddOrSetValue("SenseDanger", SenseDanger);
            ai.BB.AddOrSetValue("UnderAttack", UnderAttack);
        }

        void clearTargets() {
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
    }
}
