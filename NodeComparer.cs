using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeComparer : IComparer<AStarNode>
{
    public int Compare(AStarNode x, AStarNode y)
    {
        if (x.F > y.F)
            return 1;
        else if (x.F == y.F)
            return 0;
        else
            return -1;
        throw new System.NotImplementedException();
    }
}
