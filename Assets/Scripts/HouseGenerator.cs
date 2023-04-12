using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

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

    [SerializeField] private List<WallList> currentWalls = new List<WallList>();

    [SerializeField] private bool update;
    
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

        GenerateWall(wall, leftSide, bounds.center, bounds.size, "LEFT");
        GenerateWall(wall, rightSide, bounds.center, bounds.size, "RIGHT");
        GenerateWall(wall, frontSide, bounds.center, bounds.size, "FRONT");
        GenerateWall(wall, backSide, bounds.center, bounds.size, "BACK");

        update = false;
        oldSize = col.bounds.size;
    }

    private void GenerateWall(GameObject wallObject, Vector3 position, Vector3 origin, Vector3 size, string type)
    {
        //print("WallGen");

        GameObject item;
        var itemSize = Vector3.zero;
        WallList itemList = new WallList();
        
        switch (type)
        {
            case "BACK":
                for (int i = 0; i < currentWalls.Count; i++)
                {
                    if (currentWalls[i].name == "BACK")
                    {
                        Destroy(currentWalls[i].wall);
                        currentWalls.RemoveAt(i);
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
            
                currentWalls.Add(itemList);
                break;
            case "FRONT":
                for (int i = 0; i < currentWalls.Count; i++)
                {
                    if (currentWalls[i].name == "FRONT")
                    {
                        Destroy(currentWalls[i].wall);
                        currentWalls.RemoveAt(i);
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
                currentWalls.Add(itemList);
                break;
            case "RIGHT":
                for (int i = 0; i < currentWalls.Count; i++)
                {
                    if (currentWalls[i].name == "RIGHT")
                    {
                        Destroy(currentWalls[i].wall);
                        currentWalls.RemoveAt(i);
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
                currentWalls.Add(itemList);
                break;
            case "LEFT":
                for (int i = 0; i < currentWalls.Count; i++)
                {
                    if (currentWalls[i].name == "LEFT")
                    {
                        Destroy(currentWalls[i].wall);
                        currentWalls.RemoveAt(i);
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
                currentWalls.Add(itemList);
                break;
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
