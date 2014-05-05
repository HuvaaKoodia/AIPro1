using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MastersOfPotsDeGeimWorld;

/// <summary>
/// Author: Roy Triesscheijn (http://www.roy-t.nl)
/// Class providing 3D pathfinding capabilities using A*.
/// Heaviliy optimized for speed therefore uses slightly more memory
/// On rare cases finds the 'almost optimal' path instead of the perfect path
/// this is because we immediately return when we find the exit instead of finishing
/// 'neighbour' loop.
/// </summary>
public static class PathFinder
{
    public static SearchNode FindPath(Map world, Tile start, Tile end, int maxChecks)
	{
		return FindPath(world,new Point3D(end.X,end.Y,0),new Point3D(start.X,start.Y,0), maxChecks); 
	}
	
	/// <summary>
    /// Method that switfly finds the best path from start to end. Doesn't reverse outcome
    /// </summary>
    /// <returns>The end node where each .next is a step back)</returns>
	public static SearchNode FindPath(Map world, Point3D start, Point3D end, int maxChecks)
    {

		SearchNode startNode = new SearchNode(start, 0, 0, null);

        MinHeap openList = new MinHeap();
        openList.Add(startNode);

        int sx = world.W;
        int sy = world.H;
        int sz = 1;
        bool[] closedList = new bool[sx * sy * sz];
        int startPos = start.X + (start.Y + start.Z * sy) * sx;
		closedList[startPos] = true;

		int numCheckedTiles = 0;
        while (openList.HasNext())
        {                
			if (numCheckedTiles == maxChecks)
			{
				break;
			}

			numCheckedTiles++;
            SearchNode current = openList.ExtractFirst();
            //Console.WriteLine(current.position);

            for (int i = 0; i < surrounding.Length; i++)
            {
                var sur_node = surrounding[i];
				Point3D tmp = new Point3D(current.position, sur_node.Point);

                if (tmp == end) return current;//Found a path

                int closedIndex = tmp.X + (tmp.Y + tmp.Z * sy) * sx;

				try{
					if (PositionIsFree(tmp, world,start,end, sx, sy, sz) && closedList[closedIndex] == false)
	                {
						closedList[closedIndex] = true;
						var nextTile = GetTile(world,tmp);

						int one_cost = current.OneStepCost + sur_node.Cost;
						//one_cost+=nextTile.MovementCost;

						int h=0;
						int cph=current.position.GetDistanceHeurastic(end);
						int tph=tmp.GetDistanceHeurastic(end);
						int sub=cph-tph;
						if (sub<=0) h=-sub;

						//h=tmp.GetDistanceHeurastic(end);

						int full_cost = one_cost + h;

						SearchNode node = new SearchNode(tmp, full_cost, one_cost, current);
	                    openList.Add(node);

						//world[tmp.X,tmp.Y].MovementCost=iii++;
	                }
				}
				catch{
					
				}
            }
        }
		//Debug.Log("Checked " + numCheckedTiles + " tiles and didn't find a route");
        return null; //no path found
    }

    private static bool PositionIsFree(Point3D position, Map map, Point3D start, Point3D end, int mapWidth, int mapHeight, int mapDepth)
    {
        if (position == start || position == end) return true;
		var nextTile = GetTile(map,position);
		if (nextTile==null) return false;

		return nextTile.CanMoveTo();
    }

    private static Tile GetTile(Map map,Point3D pos){
        return map.GetTile(pos.X, pos.Y);
    }

    public class SurroundingNode
    {
		public SurroundingNode(int x, int y, int z)
        {
            Point = new Point3D(x, y, z);
            //Cost = x * x + y * y + z * z;
			Cost=1;
        }

        public Point3D Point;
        public int Cost;
    }

    //Neighbour options
	public static SurroundingNode[] surrounding = new SurroundingNode[]{
		new SurroundingNode(1,0,0),
		new SurroundingNode(0,1,0),
		new SurroundingNode(-1,0,0), 
		new SurroundingNode(0,-1,0)
    };
}           
