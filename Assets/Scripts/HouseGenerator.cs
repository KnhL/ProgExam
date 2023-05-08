using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public enum Directions
{
    Front,
    Back,
    Left,
    Right
}
public class HouseGenerator : MonoBehaviour
{
    public GameObject wall;

    [Serializable] private class WallList
    {
        public GameObject wall;
        public string name;
    }

    [SerializeField] private RoomGeneration roomGen;
    [SerializeField] private List<WallList> currentOuterWalls = new List<WallList>();
    [SerializeField] private List<WallList> currentInnerWalls = new List<WallList>();
    public List<Vector3> roomCenterPoints = new List<Vector3>();
    private List<Vector3> intersectionPoints = new List<Vector3>();

    [SerializeField] private bool update;

    [Range(0f, 1f)][SerializeField] private float roomPointArea = 0.2f;

    [SerializeField] private LayerMask wallMask;
    
    private Vector3 colCenter;

    private Vector3 leftSide;
    private Vector3 rightSide;
    private Vector3 frontSide;
    private Vector3 backSide;
    
    [SerializeField] private BoxCollider col;

    [SerializeField] private Transform zMaxBoundPoint;
    [SerializeField] private Transform zMinBoundPoint;
    [SerializeField] private Transform xMaxBoundPoint;
    [SerializeField] private Transform xMinBoundPoint;

    [SerializeField] private float intersectionPointMergeDistance;
    [SerializeField] private float intersectionWallMergeDistance;
    [SerializeField] private int iterations;

    private Coroutine roomGenerator;

    private Vector3 oldSize;

    private void Update()
    {
        float centerX = (xMinBoundPoint.position.x + xMaxBoundPoint.position.x) / 2;
        float centerY = (xMinBoundPoint.position.y + xMaxBoundPoint.position.y + zMaxBoundPoint.position.y + zMinBoundPoint.position.y) / 4;
        float centerZ = (zMaxBoundPoint.position.z + zMinBoundPoint.position.z) / 2;
        
        Vector3 center = new Vector3(centerX, centerY, centerZ);

        Vector3 size = new Vector3(Vector3.Distance(xMinBoundPoint.position, xMaxBoundPoint.position), col.size.y,
            Vector3.Distance(zMaxBoundPoint.position, zMinBoundPoint.position));
        
        col.center = center;
        col.size = size;

        var bounds1 = col.bounds;
        xMaxBoundPoint.position = new Vector3(bounds1.max.x, centerY, centerZ);
        xMinBoundPoint.position = new Vector3(bounds1.min.x, centerY, centerZ);
        zMaxBoundPoint.position = new Vector3(centerX, centerY, bounds1.max.z);
        zMinBoundPoint.position = new Vector3(centerX, centerY, bounds1.min.z);
        

        colCenter = col.center;

        leftSide = FindBoundSide(Directions.Left);
        rightSide = FindBoundSide(Directions.Right);
        frontSide = FindBoundSide(Directions.Front);
        backSide = FindBoundSide(Directions.Back);
        
        if (update || oldSize != col.bounds.size)
        {
            Generate();
        }
    }

    public List<Vector3> ReturnCenterPoints()
    {
        return roomCenterPoints;
    }

    private Vector3 FindBoundSide(Directions direction)
    {
        var bounds = col.bounds;
        var position = transform.position;
        
        return direction switch
        {
            Directions.Left => new Vector3(colCenter.x+ position.x, colCenter.y + position.y, bounds.max.z),
            Directions.Right => new Vector3(colCenter.x + position.x, colCenter.y + position.y, bounds.min.z),
            Directions.Front => new Vector3(bounds.max.x, colCenter.y + position.y, colCenter.z + position.z),
            Directions.Back => new Vector3(bounds.min.x, colCenter.y + position.y, colCenter.z + position.z),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    private void Generate()
    {
        var bounds = col.bounds;

        GenerateWall(wall, leftSide, bounds.size, "LEFT");
        GenerateWall(wall, rightSide, bounds.size, "RIGHT");
        GenerateWall(wall, frontSide, bounds.size, "FRONT");
        GenerateWall(wall, backSide, bounds.size, "BACK");

        foreach (var t in currentInnerWalls)
        {
            t.wall.layer = LayerMask.NameToLayer("Default");
            Destroy(t.wall);
        }
        currentInnerWalls.Clear();
        intersectionPoints.Clear();
        roomCenterPoints.Clear();
        roomGenerator = StartCoroutine(GenerateRoom(bounds.center, iterations));

        update = false;
        oldSize = col.bounds.size;
    }

    private void GenerateWall(GameObject wallObject, Vector3 position, Vector3 size, string type)
    {
        //print("WallGen");

        GameObject item;
        var itemSize = Vector3.zero;
        WallList itemList = new WallList();
        
        switch (type)
        {
            case "BACK":
                for (int i = 0; i < currentOuterWalls.Count; i++)
                {
                    if (currentOuterWalls[i].name == "BACK")
                    {
                        Destroy(currentOuterWalls[i].wall);
                        currentOuterWalls.RemoveAt(i);
                    }
                }
            
                item = Instantiate(wallObject, position, quaternion.identity);
                item.transform.eulerAngles = new Vector3(0, 90, 0);

                itemSize = item.transform.localScale;
                itemSize = new Vector3(size.z, size.y, itemSize.z);
                item.transform.localScale = itemSize;
            
                itemList = new WallList
                {
                    wall = item,
                    name = "BACK"
                };
            
                currentOuterWalls.Add(itemList);
                break;
            case "FRONT":
                for (int i = 0; i < currentOuterWalls.Count; i++)
                {
                    if (currentOuterWalls[i].name == "FRONT")
                    {
                        Destroy(currentOuterWalls[i].wall);
                        currentOuterWalls.RemoveAt(i);
                    }
                }
            
                item = Instantiate(wallObject, position, quaternion.identity);
                item.transform.eulerAngles = new Vector3(0, 270, 0);

                itemSize = item.transform.localScale;
                itemSize = new Vector3(size.z, size.y, itemSize.z);
                item.transform.localScale = itemSize;   
            
                itemList = new WallList
                {
                    wall = item,
                    name = "FRONT"
                };
                currentOuterWalls.Add(itemList);
                break;
            case "RIGHT":
                for (int i = 0; i < currentOuterWalls.Count; i++)
                {
                    if (currentOuterWalls[i].name == "RIGHT")
                    {
                        Destroy(currentOuterWalls[i].wall);
                        currentOuterWalls.RemoveAt(i);
                    }
                }
            
                item = Instantiate(wallObject, position, quaternion.identity);
                item.transform.eulerAngles = new Vector3(0, 0, 0);
                
                itemSize = item.transform.localScale;
                itemSize = new Vector3(size.x, size.y, itemSize.z);
                item.transform.localScale = itemSize;
            
                itemList = new WallList
                {
                    wall = item,
                    name = "RIGHT"
                };
                currentOuterWalls.Add(itemList);
                break;
            case "LEFT":
                for (int i = 0; i < currentOuterWalls.Count; i++)
                {
                    if (currentOuterWalls[i].name == "LEFT")
                    {
                        Destroy(currentOuterWalls[i].wall);
                        currentOuterWalls.RemoveAt(i);
                    }
                }
            
                item = Instantiate(wallObject, position, quaternion.identity);
                item.transform.eulerAngles = new Vector3(0, 180, 0);

                itemSize = item.transform.localScale;
                itemSize = new Vector3(size.x, size.y, itemSize.z);
                item.transform.localScale = itemSize;
            
                itemList = new WallList
                {
                    wall = item,
                    name = "LEFT"
                };
                currentOuterWalls.Add(itemList);
                break;
        }
    }

    private void GenerateWall(GameObject wallObject, Vector3 position, Vector3 size, Vector3 lookPos)
    {
        var itemSize = Vector3.zero;
        WallList itemList = new WallList();
        
        var item = Instantiate(wallObject, position, quaternion.identity);
        item.transform.LookAt(lookPos);

        itemSize = item.transform.localScale;
        itemSize = new Vector3(itemSize.x, size.y, size.z);
        item.transform.localScale = itemSize;   
            
        itemList = new WallList
        {
            wall = item,
            name = "InnerWall" + currentInnerWalls.Count
        };
        item.name = "InnerWall" + currentInnerWalls.Count;
        currentInnerWalls.Add(itemList);
    }

    private IEnumerator GenerateRoom(Vector3 center, int iterations)
    {
        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        
        Vector2 pointOffset = new Vector2(Random.Range(-col.size.x / 2 * roomPointArea, col.size.x / 2 * roomPointArea),
            Random.Range(-col.size.z / 2 * roomPointArea, col.size.z / 2 * roomPointArea));
        Vector3 intersectionPoint = new Vector3(center.x + pointOffset.x, center.y, center.z + pointOffset.y);

        // Merge intersection points that are close to each other
        foreach (var t in intersectionPoints.Where(t => Vector3.Distance(intersectionPoint, t) <= intersectionPointMergeDistance))
        {
            intersectionPoint = t;
        }
        
        intersectionPoints.Add(intersectionPoint);
        
        Debug.DrawRay(intersectionPoint, transform.up * 2, color, 1);
        
        // Find wall direction
        Vector3 direction = new Vector3();
        int dirNumber = Random.Range(0, 4);
        var right = transform.right;
        var forward = transform.forward;
        
        direction = dirNumber switch
        {
            0 => forward,
            1 => -forward,
            2 => right,
            3 => -right,
            _ => direction
        };
        
        Debug.DrawRay(intersectionPoint, direction * 100,  color, 1f);
        // Find wall lenght
        if (Physics.Raycast(intersectionPoint, direction, out var hit, 100, wallMask))
        {
            print("test");
            Debug.DrawLine(intersectionPoint, hit.point,  color, 1f);

            GameObject halfPoint = new GameObject
            {
                transform =
                {
                    position = Vector3.Lerp(intersectionPoint, hit.point, 0.5f)
                }
            };

            halfPoint.transform.LookAt(hit.point);

            
            // Checks if the wall has to merge with other walls
            Vector3 mergePoint = GetIntersectionMergePoint(halfPoint, intersectionWallMergeDistance, wallMask);

            if (mergePoint != Vector3.zero)
            {
                // Wall is close enough to another wall to merge
                intersectionPoint += mergePoint;
                if (Physics.Raycast(intersectionPoint, direction, out hit, 100, wallMask))
                {
                    halfPoint.transform.position = Vector3.Lerp(intersectionPoint, hit.point, 0.5f);
                    halfPoint.transform.LookAt(hit.point);
                }
            }

            Vector3 wallSize = new Vector3(0, col.bounds.size.y, hit.distance);
            
            GenerateWall(wall, halfPoint.transform.position, wallSize, hit.point);
            
            Destroy(halfPoint.gameObject);
        }

        int vectorRotation = Random.Range(0, 2);
        
        Vector3 newDirection = Vector3.zero;
        
        newDirection = Vector3.Lerp(direction, -direction, vectorRotation);
        newDirection = new Vector3(newDirection.z, newDirection.y, newDirection.x);
        

       
        Debug.DrawRay(intersectionPoint, newDirection * 100,  color, 1f);
        if (Physics.Raycast(intersectionPoint, newDirection, out var hit2, 100, wallMask))
        {
            print("test2");
            
            Debug.DrawLine(intersectionPoint, hit2.point,  color, 1f);
            
            Vector3 halfPoint = Vector3.Lerp(intersectionPoint, hit2.point, 0.5f);
            Vector3 wallSize = new Vector3(0, col.bounds.size.y, hit2.distance);
            
            GenerateWall(wall, halfPoint, wallSize, hit2.point);

            if (Physics.Raycast(halfPoint, direction, out var roomHit, 100, wallMask)) 
            {
                Vector3 roomCenterPoint = Vector3.Lerp(halfPoint, roomHit.point, 0.5f);
                roomCenterPoint.y = center.y;
                Debug.DrawRay(roomCenterPoint, transform.up * 2, color, 1);
                
                if (roomCenterPoint != Vector3.zero)
                {
                    roomCenterPoints.Add(roomCenterPoint);
                }
            }
        }
        
        // Wait one frame
        yield return 0;
        
        // Repeat if iterations is over 1 
        if (iterations > 1)
        {
            roomGenerator = StartCoroutine(GenerateRoom(center, iterations - 1));
        }
        else
        {
            roomGen.GenerateRooms();
        }
    }

    private Vector3 GetIntersectionMergePoint(GameObject start, float mergeLenght, LayerMask layer)
    {
        // Check right
        if (Physics.Raycast(start.transform.position, start.transform.right, out var hitInfo, mergeLenght, layer))
        {
            Debug.DrawLine(start.transform.position, hitInfo.point, Color.red, 1);
            
            if (hitInfo.transform.eulerAngles == start.transform.eulerAngles ||
                hitInfo.transform.eulerAngles == new Vector3(start.transform.eulerAngles.x,
                    start.transform.eulerAngles.y - 180, start.transform.eulerAngles.z)) 
            {
                Vector3 betweenVector = hitInfo.point - start.transform.position;
                return betweenVector;    
            }
            
        }
        // Check left
        if (Physics.Raycast(start.transform.position, -start.transform.right, out hitInfo, mergeLenght, layer))
        {
            Debug.DrawLine(start.transform.position, hitInfo.point, Color.red, 1);
            
            if (hitInfo.transform.eulerAngles == start.transform.eulerAngles ||
                hitInfo.transform.eulerAngles == new Vector3(start.transform.eulerAngles.x,
                    start.transform.eulerAngles.y - 180, start.transform.eulerAngles.z)) 
            {
                Vector3 betweenVector = hitInfo.point - start.transform.position;
                return betweenVector;    
            }
        }
        
        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(leftSide, .2f);
        Gizmos.DrawWireSphere(rightSide, .2f);
        Gizmos.DrawWireSphere(frontSide, .2f);
        Gizmos.DrawWireSphere(backSide, .2f);
        
        Gizmos.color = Color.green;
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.2f);
        Gizmos.DrawCube(col.center, new Vector3(col.size.x * roomPointArea, col.size.y, col.size.z * roomPointArea));
    }
}
