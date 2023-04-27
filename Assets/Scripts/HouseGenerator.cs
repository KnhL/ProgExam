using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    [SerializeField] private Vector2 minMaxRoomPoint;
    
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

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            colCenter = col.center;
            leftSide = FindBoundSide(Directions.Left);
            rightSide = FindBoundSide(Directions.Right);
            frontSide = FindBoundSide(Directions.Front);
            backSide = FindBoundSide(Directions.Back);
        
            update = false;
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

    private void GenerateWall(GameObject wallObject, Vector3 position, Vector3 size)
    {
        var itemSize = Vector3.zero;
        WallList itemList = new WallList();
        
        var item = Instantiate(wallObject, position, quaternion.identity);
        item.transform.eulerAngles = new Vector3(0, 0, 0);

        itemSize = item.transform.localScale;
        itemSize = new Vector3(size.z, size.y, itemSize.z);
        item.transform.localScale = itemSize;   
            
        itemList = new WallList
        {
            wall = item,
            name = "InnerWall" + currentInnerWalls.Count
        };
        currentInnerWalls.Add(itemList);
    }

    private void GenerateRoom(Vector3 center, int iterations)
    {

        Vector2 pointOffset = new Vector2(Random.Range(minMaxRoomPoint.x, minMaxRoomPoint.y),
            Random.Range(minMaxRoomPoint.x, minMaxRoomPoint.y));
        Vector3 intersectionPoint = new Vector3(center.x + pointOffset.x, center.y, center.z + pointOffset.y);

        Vector3 direction = new Vector3();
        int dirNumber = Random.Range(0, 4);

        switch (dirNumber)
        {
            case 0:
                direction = Vector3.forward;
                break;
            case 1:
                direction = Vector3.back;
                break;
            case 2:
                break;
            case 3:
                break;
        }
        
        if (Physics.Raycast(intersectionPoint, ))
        {
            
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(leftSide, .2f);
        Gizmos.DrawWireSphere(rightSide, .2f);
        Gizmos.DrawWireSphere(frontSide, .2f);
        Gizmos.DrawWireSphere(backSide, .2f);
    }
}
