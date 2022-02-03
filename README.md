# astar-for-unity-tilemap
a* algorithm based on unity tilemap using c#, the path will be smoothed after searching.

![sample](https://raw.githubusercontent.com/AugustChuu/astar-for-unity-tilemap/main/sample.gif "sample")
# How to use
add the "AStar" component to your object. then call the function "SetMoveTarget(Vector3 pos)" to find a way and move to the position

### Example
```
public class MainCharacterController : MonoBehaviour
{
    AStar aStar;

    void Start()
    {
        aStar = gameObject.GetComponent<AStar>();

            // [optional] enable to display the destination point, the pointer should have a SpriteRenderer component
        aStar.SetPointer(GameObject pointer)
        
            // [necessary] the collider radius of the controlled object
            .SetRadius(float radius)
            
            // [necessary] the base object is considered as the point (0, 0), you shuold place it on the bottom left of the tilemap
            .SetBaseObject(GameObject baseObject)
            
            // [necessary]
            .SetTileMap(GameObject tilemap);
        
        // aStar.maxSpeed = (float);
        // aStar.accelaration = (float);
        
        aStar.Build();
    }


    void Update()
    {
        // receive left mouse button click position for destination
        if (Input.GetMouseButtonDown(0))
        {
            var dest = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dest.z = transform.position.z;
            
            // set the destination that the object will move to
            aStar.SetMoveTarget(dest);
        }

        // when the map has changed
        // aStar.UpdateGraph();
    }


}
```

