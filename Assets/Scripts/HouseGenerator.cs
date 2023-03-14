using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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

    private Vector3 oldSize;

    private void FixedUpdate()
    {
        colCenter = col.center;

        col.hasModifiableContacts = true;
        
        leftSide = FindBoundSide(Directions.Left);
        rightSide = FindBoundSide(Directions.Right);
        frontSide = FindBoundSide(Directions.Front);
        backSide = FindBoundSide(Directions.Back);

        if (update || oldSize != col.bounds.size)
        {
            var bounds = col.bounds;

            GenerateWall(wall, leftSide, bounds.center, bounds.size);
            GenerateWall(wall, rightSide, bounds.center, bounds.size);
            GenerateWall(wall, frontSide, bounds.center, bounds.size);
            GenerateWall(wall, backSide, bounds.center, bounds.size);

            update = false;
        }

        oldSize = col.bounds.size;
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

    private void GenerateWall(GameObject wallObject, Vector3 position, Vector3 origin, Vector3 size)
    {
        //print("WallGen");
        
        if (position.x < origin.x) // BACK
        {
            for (int i = 0; i < currentWalls.Count; i++)
            {
                if (currentWalls[i].name == "BACK")
                {
                    Destroy(currentWalls[i].wall);
                    currentWalls.RemoveAt(i);
                }
            }
            
            GameObject item = Instantiate(wallObject, position, quaternion.identity);
            item.transform.eulerAngles = new Vector3(0, 90, 0);

            var itemSize = item.transform.localScale;
            itemSize = new Vector3(size.z, size.y, itemSize.z);
            item.transform.localScale = itemSize;
            
            WallList itemList = new WallList
            {
                wall = item,
                name = "BACK"
            };
            
            currentWalls.Add(itemList);
        }
        else if (position.x > origin.x) // FRONT
        {
            for (int i = 0; i < currentWalls.Count; i++)
            {
                if (currentWalls[i].name == "FRONT")
                {
                    Destroy(currentWalls[i].wall);
                    currentWalls.RemoveAt(i);
                }
            }
            
            GameObject item = Instantiate(wallObject, position, quaternion.identity);
            item.transform.eulerAngles = new Vector3(0, 270, 0);

            var itemSize = item.transform.localScale;
            itemSize = new Vector3(size.z, size.y, itemSize.z);
            item.transform.localScale = itemSize;   
            
            WallList itemList = new WallList
            {
                wall = item,
                name = "FRONT"
            };
            currentWalls.Add(itemList);
        }
        else if (position.z < origin.z) // RIGHT
        {
            for (int i = 0; i < currentWalls.Count; i++)
            {
                if (currentWalls[i].name == "RIGHT")
                {
                    Destroy(currentWalls[i].wall);
                    currentWalls.RemoveAt(i);
                }
            }
            
            GameObject item = Instantiate(wallObject, position, quaternion.identity);
            item.transform.eulerAngles = new Vector3(0, 0, 0);

            var itemSize = item.transform.localScale;
            itemSize = new Vector3(size.x, size.y, itemSize.z);
            item.transform.localScale = itemSize;
            
            WallList itemList = new WallList
            {
                wall = item,
                name = "RIGHT"
            };
            currentWalls.Add(itemList);
        }
        else if (position.z > origin.z) // LEFT
        {
            for (int i = 0; i < currentWalls.Count; i++)
            {
                if (currentWalls[i].name == "LEFT")
                {
                    Destroy(currentWalls[i].wall);
                    currentWalls.RemoveAt(i);
                }
            }
            
            GameObject item = Instantiate(wallObject, position, quaternion.identity);
            item.transform.eulerAngles = new Vector3(0, 0, 0);

            var itemSize = item.transform.localScale;
            itemSize = new Vector3(size.x, size.y, itemSize.z);
            item.transform.localScale = itemSize;
            
            WallList itemList = new WallList
            {
                wall = item,
                name = "LEFT"
            };
            currentWalls.Add(itemList);
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
