using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarGraph
{
    int width;
    int height;
    public float radius;
    static List<List<AStarNode>> matrix;
    List<AStarNode> open;
    HashSet<AStarNode> closed;
    UnityEngine.Tilemaps.Tilemap tilemap;
    Vector3Int basePos;
    Vector3 basePosF;
    public GameObject self;

    public AStarGraph()
    {

    }

    public AStarGraph BuildGraph(UnityEngine.Tilemaps.Tilemap tilemap, float radius, GameObject baseObj, GameObject self)
    {
        this.radius = radius;
        this.tilemap = tilemap;
        this.self = self;
        basePos = tilemap.WorldToCell(baseObj.transform.position);
        basePosF = baseObj.transform.position;
        width = tilemap.size.x;
        height = tilemap.size.y;
        if (matrix == null)
        {
            matrix = new List<List<AStarNode>>();
            List<AStarNode> line;
            for (int i = 0; i < height; i++)
            {
                line = new List<AStarNode>();
                for (int j = 0; j < width; j++)
                {
                    line.Add(new AStarNode(j, i, !HasCollider(basePosF + new Vector3(j, i, 0))));
                }
                matrix.Add(line);
            }
        }
        return this;
    }

    // use the ray to judge whether a point is visitable, it also works on the colliders that not in the tilemap
    public bool HasCollider(Vector3 pos)
    {
        int[] dirX = { 0, 0, 1, -1 };
        int[] diry = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            var hitInfo = Physics2D.Raycast(pos, new Vector2(dirX[i], diry[i]));
            if (hitInfo.distance < radius && hitInfo.collider != null && hitInfo.transform != self.transform)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCollider(Vector3 pos)
    {
        int[] dirX = { 0, 0, 1, -1 };
        int[] diry = { 1, -1, 0, 0 };
        for (int i=0; i<4; i++)
        {
            var hitInfo = Physics2D.Raycast(pos, new Vector2(dirX[i], diry[i]));
            if (hitInfo.distance < 0.01f && hitInfo.collider != null)
            {
                return true;
            }
        }
        return false;
    }

    // translate the vector3 objects into node objects
    public LinkedList<Vector3> FindPathInWorld(Vector3 begin, Vector3 end)
    {
        var beginCell = tilemap.WorldToCell(begin);
        var beginG = new AStarNode(beginCell.x, beginCell.y, true);
        var endCell = tilemap.WorldToCell(end);
        var endG = new AStarNode(endCell.x, endCell.y, true);

        var path = FindPath(beginG, endG);
        if(path != null)
        {
            path.RemoveFirst();
            path.AddLast(end);
            SmoothPath(path, begin);
        }
        return path;
    }
         
    public LinkedList<Vector3> FindPath(AStarNode begin, AStarNode end)
    {
        open = new List<AStarNode>();
        closed = new HashSet<AStarNode>();
        begin.x -= basePos.x;
        begin.y -= basePos.y;
        begin.G = begin.F = 0;
        end.x -= basePos.x;
        end.y -= basePos.y;
        AStarNode current, newNode;
        int[] dx = {1, -1, 0, 0};
        int[] dy = {0, 0, -1, 1};
        int x, y;
        open.Add(begin);
        while (open.Count > 0)
        {
            open.Sort(new NodeComparer());
            current = open[0];
            open.Remove(current);
            closed.Add(current);

            if (current.Equals(end))
            {
                return BuildPath(current);
            }

            for (int i = 0; i < 4; i++)
            {
                x = current.x + dx[i];
                y = current.y + dy[i];
                if (InRange(x, y)) 
                {
                    newNode = matrix[y][x];
                    if (!closed.Contains(newNode) && newNode.isReachable)
                    {
                        if (open.Contains(newNode))
                        {
                            // use the Euclidean distance as heuristic function, and weighted 0.5
                            if (newNode.G <= current.G + 1f + newNode.Distance(end) * 0.5f)
                            {
                                continue;
                            }
                            else
                            {
                                newNode.G = current.G + 1f;
                                newNode.H = newNode.Distance(end);
                                newNode.F = newNode.G + newNode.H * 0.5f;
                                newNode.parent = current;
                            }
                        }
                        else
                        {
                            newNode.G = current.G + 1f;
                            newNode.H = newNode.Distance(end);
                            newNode.F = newNode.G + newNode.H * 0.5f;
                            newNode.parent = current;
                            open.Add(newNode);
                        }
                    }
                }
            }
        }
        return null;
    }


    // shorten the path by not just walking along the axis
    public void SmoothPath(LinkedList<Vector3> path, Vector3 begin)
    {
        LinkedListNode<Vector3> node;
        for (node = path.First;  node.Next != null; node = node.Next)
        {
            while (node.Next.Next != null)
            {
                if(node.Next.Next.Next == null)
                {
                    var hitInfo = Physics2D.Raycast(node.Value, node.Next.Next.Value - node.Value);
                    if (hitInfo.collider == null || hitInfo.distance > Distance(node.Value, node.Next.Next.Value))
                        path.Remove(node.Next);

                }
                else if (IsSmoothable(node.Value, node.Next.Next.Value, false))
                {
                    path.Remove(node.Next);
                }
                else
                {
                    break;
                }

            }
        }
        for (node = path.First; node.Next != null; )
        {
            if (IsSmoothable(node.Next.Value, begin, true))
            {
                var pre = node;
                node = node.Next;
                path.Remove(pre);
            }
            else
            {
                break;
            }
        }

    }
    public void UpdateGraph()
    {
        width = tilemap.size.x;
        height = tilemap.size.y;
        matrix = new List<List<AStarNode>>();
        List<AStarNode> line;
        for (int i = 0; i < height; i++)
        {
            line = new List<AStarNode>();
            for (int j = 0; j < width; j++)
            {
                line.Add(new AStarNode(j, i, !HasCollider(basePosF + new Vector3(j, i, 0))));
            }
            matrix.Add(line);
        }
    }


    // after finded the destination, track through the parent node until the beginning
    public LinkedList<Vector3> BuildPath(AStarNode node)
    {
        LinkedList<Vector3> path = new LinkedList<Vector3>();
        while (node != null)
        {
            path.AddFirst(tilemap.CellToWorld(new Vector3Int(node.x, node.y, 0) + basePos) + new Vector3(0.5f, 0.5f, 0));
            node = node.parent;
        }
        return path;
    }

    public bool InRange(int x, int y)
    {
        return (
            x >= 0 && y >= 0 && x < width && y < height
        ); 
    }

    public bool IsSmoothable(Vector3 start, Vector3 end, bool isFromBegin)
    {
        float[] dx = { 0, 0, -radius, radius };
        float[] dy = { radius, -radius, 0, 0 };
        float distance = Distance(start, end);
        RaycastHit2D hitInfo;
        bool smoothable = true;
        if (!isFromBegin)
        {
            for (int i = 0; i < 4; i++)
            {
                hitInfo = Physics2D.Raycast(start + new Vector3(dx[i], dy[i], 0), end - start);
                smoothable = smoothable && (hitInfo.collider == null || hitInfo.distance > distance);
            }
        }
        else
        {
            hitInfo = Physics2D.Raycast(start, end - start);
            smoothable = hitInfo.transform == self.transform;
        }
        
        return smoothable;
    }

    public static float Distance(Vector3 v1, Vector3 v2)
    {
        return (
            Mathf.Sqrt(((v1.x - v2.x) * (v1.x - v2.x)) + ((v1.y - v2.y) * (v1.y - v2.y)))
        );
    }

    public void PrintGraph()
    {
        string s;
        foreach (var line in matrix)
        {
            s = "";
            foreach (var node in line)
            {
                if (node.isReachable)
                    s += "0";
                else
                    s += "1";
            }
            Debug.Log(s);
        }
    }
}
