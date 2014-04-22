using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Author: Roy Triesscheijn (http://www.roy-t.nl)
/// Class defining Nodes used in path finding to mark our routes
/// </summary>
public class SearchNode
{
    public Point3D position;
    public int FullPathCost;
    public int OneStepCost;
    public SearchNode next;
    public SearchNode nextListElem;

	public SearchNode(Point3D position, int full_cost, int step_Cost, SearchNode next)
    {
        this.position = position;
		this.FullPathCost = full_cost;
		this.OneStepCost = step_Cost;
        this.next = next;
    }
}
