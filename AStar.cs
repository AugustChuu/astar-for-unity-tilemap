using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    public float maxSpeed = 3f;
    public float acceleration = 90f;
    public float radius = 0.25f;
    bool onMove = false;
    float speed = 0;
    Vector3 dest;
    Vector3 finalDest;
    Vector3 dir;
    GameObject Ptr;
    GameObject baseObj;
    UnityEngine.Tilemaps.Tilemap tilemap;
    AStarGraph graph;
    LinkedList<Vector3> path;


    // Update is called once per frame
    void Update()
    {
        if (onMove)
        {
            Move();
        }
    }

    void CorrectDirection()
    {
        dir = dest - gameObject.transform.position;
        dir.z = 0;
        dir.Normalize();
    }

    void Accelerate()
    {
        if (speed + acceleration * Time.deltaTime <= maxSpeed)
            speed += acceleration * Time.deltaTime;
        else
            speed = maxSpeed;
    }

    public void Build()
    {
        graph = new AStarGraph().BuildGraph(tilemap, radius, baseObj, gameObject);
    }

    public AStar SetBaseObject(GameObject obj)
    {
        baseObj = obj;
        return this;
    }
    public AStar SetTileMap(GameObject obj)
    {
        tilemap = obj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        return this;
    }

    public AStar SetPointer(GameObject obj)
    {
        Ptr = obj;
        Ptr.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        return this;
    }

    public AStar SetRadius(float _radius)
    {
        this.radius = _radius;
        if (graph != null)
        {
            graph.radius = _radius;
        }
        return this;
    }

    public void UpdateGraph()
    {
        graph.UpdateGraph();
    }

    public void SetMoveTarget(Vector3 pos)
    {
        var begin = gameObject.transform.position;
        LinkedList<Vector3> nPath = null;
        float[] dx = { 1, -1, 0, 0 };
        float[] dy = { 0, 0, 1, -1 };
        if (!graph.IsCollider(pos))
        {
            for (int i = 0; i < 4; i++)
            {
                var hitInfo = Physics2D.Raycast(pos, new Vector2(dx[i], dy[i]));
                if (hitInfo.distance != 0 && hitInfo.distance < radius)
                {
                    pos -= new Vector3(dx[i], dy[i], 0) * (radius - hitInfo.distance);
                }
            }
            nPath = graph.FindPathInWorld(begin, pos);
        }
        if (nPath != null)
        {
            onMove = true;
            path = nPath;
            dest = path.First.Value;
            finalDest = path.Last.Value;

            if (Ptr != null)
            {
                Ptr.transform.position = finalDest;
                Ptr.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
            }
        }
    }

    void Move()
    {
        if (VectorEqual(gameObject.transform.position, dest, 0.05f) && path.Count > 0)
        {
            dest = path.First.Value;
            path.RemoveFirst();
        }
        else if (VectorEqual(gameObject.transform.position, dest, 0.05f) && path.Count == 0)
        {
            if (Ptr != null)
            {
                Ptr.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
            onMove = false;
            speed = 0;
        }
        if (speed < maxSpeed)
            Accelerate();
        CorrectDirection();
        gameObject.transform.Translate(dir * speed * Time.deltaTime);
    }

    // judge if the object reach the destination in the limit of inaccuracy
    bool VectorEqual(Vector3 v1, Vector3 v2, float difference)
    {
        return (Mathf.Abs(v1.x - v2.x) <= difference && Mathf.Abs(v1.y - v2.y) <= difference);
    }

}
