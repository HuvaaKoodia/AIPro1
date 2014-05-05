using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    public class FindFood : State
    {
        public override void Start(AI ai)
        {
        }

        public override void End(AI ai)
        {
            ai.MasterEntity.clearTargets();
        }

        public override void Update(AI ai)
        {
            if (ai.MasterEntity.EatTarget == null)
            {
                Console.WriteLine("LOOKING FOR FOOD.");
                //eat if next to food
                int x = ai.MasterEntity.X, y = ai.MasterEntity.Y;
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
                    ai.MasterEntity.SetEatTarget(food_target);
                    Console.WriteLine("set food target");
                }
                else if (ai.MasterEntity.MoveTarget == null || !ai.MasterEntity.MoveTarget.IsType(Tile.Type.food))
                {
                    //find closest food tile, move to that
                    Tile closest = null;
                    int distance = 100000;

                    foreach (var t in ai.world.map_tiles)
                    {
                        if (t.TileType == Tile.Type.food)
                        {
                            {
                                var d = (t.X - x) * (t.X - x) + (t.Y - y) * (t.Y - y);
                                if (d < distance)
                                {
                                    distance = d;
                                    closest = t;
                                }
                            }
                        }
                    }
                    if (closest!=null) ai.MasterEntity.SetMoveTarget(closest);
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
        }
    }

    public class Clone : State
    {
        public override void Start(AI ai)
        {
        }

        public override void End(AI ai)
        {
            ai.MasterEntity.ForceLookForFood = false;
            ai.MasterEntity.clearTargets();
        }

        public override void Update(AI ai)
        {
            if (!ai.MasterEntity.HasEnoughEnergyToClone())
            {
                //look for food.
                ai.MasterEntity.ForceLookForFood = true;
                Console.WriteLine("Not enough energy for cloning! Look for food!");
            }
            else { 
                if (ai.MasterEntity.CloneSelf())
                {
                    Console.WriteLine("Cloned!");
                }
            }
        }
    }

    public class Attack : State
    {
        public override void Update(AI ai)
        {
            Console.WriteLine("Attacking.");
        }
    }

    public class Mine : State
    {
        public override void Start(AI ai)
        {
        }

        public override void End(AI ai)
        {
            ai.MasterEntity.clearTargets();
        }

        public override void Update(AI ai)
        {
            if (ai.MasterEntity.DigTarget == null)
            {
                Console.WriteLine("LOOKING FOR DIAMONDS.");
                //dig if next to diamonds
                int x = ai.MasterEntity.X, y = ai.MasterEntity.Y;
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
                    ai.MasterEntity.SetDigTarget(target);
                }
                else if (ai.MasterEntity.MoveTarget == null||!ai.MasterEntity.MoveTarget.IsType(Tile.Type.diamond))
                {
                    //find closest tile, move to that
                    Tile closest = null;
                    int distance = 100000;

                    foreach (var t in ai.world.map_tiles)
                    {
                        if (t.TileType == Tile.Type.diamond)
                        {
                            {
                                var d = (t.X - x) * (t.X - x) + (t.Y - y) * (t.Y - y);
                                if (d < distance)
                                {
                                    distance = d;
                                    closest = t;
                                }
                            }
                        }
                    }

                    if (closest!=null) ai.MasterEntity.SetMoveTarget(closest);
                }
            }
            if (ai.MasterEntity.DigTarget != null)
            {
                Console.WriteLine("Mining DIAMONDS!!.");
            }
        }
    }
}
