using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [SerializeField] private List<WallList> currentOuterWalls = new List<WallList>();
    [SerializeField] private List<WallList> currentInnerWalls = new List<WallList>();

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

    public void Generate()
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
        GenerateRoom(bounds.center, 2);

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

    private void GenerateRoom(Vector3 center, int iterations)
    {
        if (iterations <= 0) { return; }

        Vector2 pointOffset = new Vector2(Random.Range(-col.size.x / 2 * roomPointArea, col.size.x / 2 * roomPointArea),
            Random.Range(-col.size.z / 2 * roomPointArea, col.size.z / 2 * roomPointArea));
        Vector3 intersectionPoint = new Vector3(center.x + pointOffset.x, center.y, center.z + pointOffset.y);
        
        Vector3 direction = new Vector3();
        int dirNumber = Random.Range(0, 4);
        
        Debug.DrawRay(intersectionPoint, transform.up * 2, Color.red, 1);

        direction = dirNumber switch
        {
            0 => transform.forward,
            1 => -transform.forward,
            2 => transform.right,
            3 => -transform.right,
            _ => direction
        };

        Debug.DrawRay(intersectionPoint, direction * 100,  Color.red, 0.1f);
        
        if (Physics.Raycast(intersectionPoint, direction, out var hit, 100, wallMask))
        {
            Vector3 halfPoint = Vector3.Lerp(intersectionPoint, hit.point, 0.5f);
            Vector3 wallSize = new Vector3(0, col.bounds.size.y, hit.distance);
            
            GenerateWall(wall, halfPoint, wallSize, hit.point);
        }

        int vectorRotation = Random.Range(0, 2);
        int vectorMirror = Random.Range(0, 2);
        Vector3 newDirection = Vector3.zero;
        
        if (vectorMirror == 1)
        {
            newDirection = Vector3.Lerp(direction, -direction, vectorMirror);
        }
        else
        {
            direction = Vector3.Lerp(direction, -direction, vectorRotation);
            newDirection = new Vector3(direction.z, direction.y, direction.x);
        }

        if (Physics.Raycast(intersectionPoint, newDirection, out var hit2, 100, wallMask))
        {
            Vector3 halfPoint = Vector3.Lerp(intersectionPoint, hit2.point, 0.5f);
            Vector3 wallSize = new Vector3(0, col.bounds.size.y, hit2.distance);
            
            GenerateWall(wall, halfPoint, wallSize, hit2.point);
        }
        
        if (iterations > 1)
        {
            GenerateRoom(center, iterations - 1);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(leftSide, .2f);
        Gizmos.DrawWireSphere(rightSide, .2f);
        Gizmos.DrawWireSphere(frontSide, .2f);
        Gizmos.DrawWireSphere(backSide, .2f);
        
        Gizmos.DrawCube(col.center, new Vector3(col.size.x * roomPointArea, col.size.y, col.size.z * roomPointArea));
    }
}
