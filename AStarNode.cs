using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    // Start is called before the first frame update
    public int x
    {
        set;
        get;
    }
    public int y
    {
        set;
        get;
    }
    public bool isReachable
    {
        set;
        get;
    }
    public AStarNode parent
    {
        set;
        get;
    }

    public float F;
    public float G;
    public float H;

    public AStarNode()
    {
        this.x = 0;
        this.y = 0;
        this.isReachable = true;
    }

    public AStarNode(int _x, int _y, bool _isReachable)
    {
        this.x = _x;
        this.y = _y;
        this.isReachable = _isReachable;
    }

    public bool Equals(AStarNode node)
    {
        return this.x == node.x && this.y == node.y;
    }

    public float Distance(AStarNode node)
    {
        var dx = Mathf.Abs(this.x - node.x);
        var dy = Mathf.Abs(this.y - node.y);
        return Mathf.Sqrt(dx*dx + dy*dy);
    }
}
