using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    bool onMove = false;
    float speed = 0;
    float maxSpeed = 3f;
    float acceleration = 120f;
    Vector3 dest;
    Vector3 dir;
    UnityEngine.Tilemaps.Tilemap tilemap;
    AStarGraph graph;
    LinkedList<Vector3> path;


    // Start is called before the first frame update
    /// <summary>
    /// the tilemap should be tagged with "ColliderMap", and every tile placed on it will be considered as a collider.
    /// its possiblie to use annother map for decoration
    /// </summary>
    void Start()
    {
        tilemap = GameObject.FindGameObjectWithTag("ColliderMap").GetComponent<UnityEngine.Tilemaps.Tilemap>();
        graph = new AStarGraph().BuildGraph(tilemap);
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //the added vector is the same as the tile anchor deviation
            var end = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0.5f, 0.5f, 0);
            end.z = 0;
            SetMoveTarget(end);
        }
        if(onMove)
            Move();
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

    /// <summary>
    /// for not being stucked by a wall
    /// </summary>
    void OnCollisionEnter2D()
    {
        var hitInfo = Physics2D.RaycastAll(transform.position, dir);
        if (hitInfo.Length > 1)
        {
            var v = new Vector3(hitInfo[1].point.x, hitInfo[1].point.y, transform.position.z) - transform.position;
            if (Vector3.Angle(v, dir) > 0 && Vector3.Angle(v, dir) < 30 && hitInfo[1].distance < 0.5f)
            {
                onMove = false;
                speed = 0;
            }
        }
    }

    void OnCollisionStay2D()
    {
        var hitInfo = Physics2D.RaycastAll(transform.position, dir);
        if (hitInfo.Length > 1)
        {
            var v = new Vector3(hitInfo[1].point.x, hitInfo[1].point.y, transform.position.z) - transform.position;
            if (Vector3.Angle(v, dir) > 0 && Vector3.Angle(v, dir) < 30 && hitInfo[1].distance < 0.5f)
            {
                onMove = false;
                speed = 0;
            }
        }
    }

    void SetMoveTarget(Vector3 pos)
    {
        var begin = gameObject.transform.position + new Vector3(0.5f, 0.5f, 0);
        LinkedList<Vector3> nPath = null;
        if (!tilemap.HasTile(tilemap.WorldToCell(pos)))
            nPath = graph.FindPathInWorld(begin, pos);
        if (nPath != null)
        {
            onMove = true;
            path = nPath;
            dest = path.First.Value;
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
            onMove = false;
            speed = 0;
        }
        if (speed < maxSpeed)
            Accelerate();
        CorrectDirection();
        gameObject.transform.Translate(dir * speed * Time.deltaTime);
    }

    bool VectorEqual(Vector3 v1, Vector3 v2, float difference)
    {
        return (Mathf.Abs(v1.x - v2.x) <= difference && Mathf.Abs(v1.y - v2.y) <= difference);
    }
}
