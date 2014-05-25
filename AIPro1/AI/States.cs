using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    public class FindFood : State
    {
        public override void End(AI ai)
        {
            ai.Master.ClearTargets();
            ai.Master.ForceLookForFood = false;
        }

        public override void Update(AI ai)
        {
            if (ai.Master.NOFOODLEFT) {
                ai.Master.ForceLookForFood = false;
                ai.Master.HungerThreshold = 0;
                return;
            }

            if (ai.Master.EatTarget == null)
            {
                Console.WriteLine("LOOKING FOR FOOD.");
                //eat if next to food
                int x = ai.Master.X, y = ai.Master.Y;
                Tile food_target = null;
                foreach (var s in PathFinder.surrounding)
                {
                    var tile = ai.world.GetTile(x + s.Point.X, y + s.Point.Y);
                    if (tile.IsType(Tile.Type.food))
                    {
                        food_target = tile;
                    }
                }

                if (food_target != null)
                {
                    ai.Master.SetEatTarget(food_target);
                    ai.Master.ForceLookForFood = false;

                    Console.WriteLine("set food target");
                }
                else if (ai.Master.MoveTarget == null || !ai.Master.MoveTarget.IsType(Tile.Type.food))
                {
                    //find closest food tile, move to that
                    Tile closest;
                    int distance;

                    ai.Master.GetClosestTile(Tile.Type.food,ai.Master.CurrentTile, out closest, out distance);

                    if (closest!=null) ai.Master.SetMoveTarget(closest);
                }
            }
            else
            {
                Console.WriteLine("Eating FOOD.");
            }
        }
    }

    public class Hide : State
    {
        public override void Update(AI ai)
        {
            Console.WriteLine("TRYING TO HIDE.");
        }
    }

    public class Idle : State
    {
        public override void Update(AI ai)
        {
            Console.WriteLine("IDLING.");
        }
    }

    public class Fight : State
    {
        public override void Update(AI ai)
        {
            Console.WriteLine("Fighting.");
        }
    }

    //commands

    public class Defend : State
    {
        public override void Update(AI ai)
        {
            Console.WriteLine("Defending.");
            //dev. todo run away if orders are to do so.

            if (ai.Master.LastAttacker != null)
            {
                ai.Master.SetAttackTarget(ai.Master.LastAttacker);

                Console.WriteLine("set target");
            }
        }
    }

    public class Clone : State
    {
        public override void Update(AI ai)
        {
            if (!ai.Master.CanCloneCheckEnergy())
            {
                //look for food.
                ai.Master.ForceLookForFood = true;
                Console.WriteLine("Not enough energy for cloning! Look for food!");
            }
            else { 
                if (ai.Master.CloneSelf())
                {
                    Console.WriteLine("Cloned!");
                }
            }
        }
    }

    public class Attack : State
    {
        public override void End(AI ai)
        {
            ai.Master.ClearTargets();
            ai.Master.ClearChaseTarget();
        }

        public override void Update(AI ai)
        {
            if (ai.Master.AttackTarget != null){
                var distance=ai.Master.DistanceTo(ai.Master.AttackTarget.CurrentTile);

                if (distance > 1)
                {
                    ai.Master.ClearTargets();
                }
                else
                {
                    Console.WriteLine("Attacking");
                }
            }
            if (ai.Master.AttackTarget == null)
            {
                Console.WriteLine("LOOKING FOR target.");
                if (ai.Master.ChaseTarget==null)
                {
                    //find closest target
                    Entity closest = null;
                    int distance=10000;

                    foreach (var e in ai.world.GameEntities) {
                        if (e.TeamNumber == ai.Master.TeamNumber) continue;
                        var tile = e.CurrentTile;
                        var d = ai.Master.DistanceTo(tile);
                        if (d < distance) {
                            distance = d;
                            closest = e;
                        }
                    }

                    if (closest != null) ai.Master.SetChaseTarget(closest);
                }
                if (ai.Master.ChaseTarget != null) {
                    
                    //next to target
                    int x = ai.Master.X, y = ai.Master.Y;
                    Tile target = null;
                    foreach (var s in PathFinder.surrounding)
                    {
                        var tile = ai.world.GetTile(x + s.Point.X, y + s.Point.Y);
                        if (tile.EntityReference == ai.Master.ChaseTarget)
                        {
                            target = tile;
                        }
                    }

                    if (target != null)
                    {
                        ai.Master.SetAttackTarget(target.EntityReference);

                        Console.WriteLine("set attack target");
                    }
                }
            }
        }
    }

    public class Mine : State
    {
        public override void End(AI ai)
        {
            ai.Master.ClearTargets();
        }

        public override void Update(AI ai)
        {
            if (ai.Master.DigTarget == null)
            {
                Console.WriteLine("LOOKING FOR DIAMONDS.");
                //dig if next to diamonds
                int x = ai.Master.X, y = ai.Master.Y;
                Tile target = null;
                foreach (var s in PathFinder.surrounding)
                {
                    var tile = ai.world.GetTile(x + s.Point.X, y + s.Point.Y);
                    if (tile.IsType(Tile.Type.diamond))
                    {
                        target = tile;
                    }
                }

                if (target != null)
                {
                    ai.Master.SetDigTarget(target);
                }
                else if (ai.Master.MoveTarget == null||!ai.Master.MoveTarget.IsType(Tile.Type.diamond))
                {
                    //find closest tile, move to that
                    Tile closest;
                    int distance;

                    ai.Master.GetClosestTile(Tile.Type.diamond,ai.Master.CurrentTile, out closest, out distance);

                    if (closest!=null) ai.Master.SetMoveTargetCheckEnergy(closest);
                }
            }
            if (ai.Master.DigTarget != null)
            {
                Console.WriteLine("Mining DIAMONDS!!.");
            }
        }
    }
}