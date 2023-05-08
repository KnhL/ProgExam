using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGeneration : MonoBehaviour
{
    
    //List of rooms, serializable so it can be seen through the inspector
    [Serializable] private class RoomList
    {
        public string name;
        public List<GameObject> list;
    }

    //Enum of rooms that can be generated
    private enum Generate
    {
        LivingRoom,
        KitchenRoom,
    }
    
    [Header("Settings")]
    //Based on Generate enum, made to be able to test different room spawns
    [SerializeField] private Generate generationType;
    
    //Array of raycast hits, to store which objects are the walls.
    private RaycastHit[] hits;
    
    [Header("Variables")]
    public List<GameObject> wallList;
    [Header("Spawned Objects")]
    [SerializeField] private List<RoomList> objectList;
    private int randomWall;

    [Header("Spawnable Objects")]
    [SerializeField] private GameObject[] Objects;

    //Gets HouseGenerator script
    private HouseGenerator _houseGenerator;

    //Finds a random number between 0-4, 4 is excluded, and also creates a new wall list.
    void Start()
    {
        randomWall = Random.Range(0, 4);
        wallList = new List<GameObject>();
    }

    //Function that generates furniture on positions gotten from HouseGenerator script, each time function is called, previous objects gets deleted, and new ones gets generated.
    public void GenerateFurniture()
    {
        _houseGenerator = FindObjectOfType<HouseGenerator>();

        DeleteObjects();

        foreach (var centerPoint in _houseGenerator.ReturnCenterPoints())
        {
            randomWall = Random.Range(0, 4);
            FindObjects(centerPoint);

            var randomNumber = Random.Range(0, 2);

            if (randomNumber == 1)
            {
                KitchenRoom(centerPoint);
            }
            else if (randomNumber == 0)
            {
                LivingRoom(centerPoint);
            }
        }
    }

    //Finds the walls surrounding the room, and adds it to the wall list.
    public void FindObjects(Vector3 centerPoint)
    {
        Vector3[] directions = { transform.forward, transform.right, -transform.right, -transform.forward, -transform.up };
        hits = new RaycastHit[5];
        for (int i = 0; i < directions.Length; i++)
        {
            Ray ray = new Ray(centerPoint, directions[i]);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hits[i] = hit;
                if (!wallList.Contains(hit.collider.gameObject))
                {
                    wallList.Add(hit.collider.gameObject);
                }
            }
            else
            {
                hits[i] = new RaycastHit();
            }
        }
    }

    //Generates the LivingRoom.
    private void LivingRoom(Vector3 centerPoint)
    {
        //instantiates an object, and adds it to the object list, and sets the objects rotation to the random wall chosen's rotation.
        //Item gets added to the object list after.
        var sofa = Instantiate(Objects[11], centerPoint, quaternion.identity);
        sofa.transform.localEulerAngles = new Vector3(0, hits[randomWall].transform.localEulerAngles.y, 0);
        objectList[1].list.Add(sofa);
        
        //Shoots a ray backwards from the objects position
        Ray ray = new Ray(sofa.transform.position, -sofa.transform.forward);
        RaycastHit hit;

        //Moves the object back into the wall, based on the raycast.
        if (Physics.Raycast(ray, out hit))
        {
            sofa.transform.position -= sofa.transform.forward * hit.distance;
        }

        //Uses three functions to move the object out from the wall, and down to the floor, and after, a random position along the wall.
        MoveOut(sofa);
        MoveDown(sofa);
        RandomPositionOnWall(sofa);

        //Shoots a new ray forwards from the items position.
        Ray rayTable = new Ray(sofa.transform.position, sofa.transform.forward);
        RaycastHit hitTable;

        //Instantiates a new object based on the raycasts hit point, and choses randomly if the object should contain a TV or not.
        //Item gets added to the object list after.
        if (Physics.Raycast(rayTable, out hitTable))
        {
            var tvOrNot = Random.Range(17, 19);
            var table = Instantiate(Objects[tvOrNot], hitTable.point, quaternion.identity);
            objectList[1].list.Add(table);
            
            table.transform.localEulerAngles = new Vector3(0, hitTable.transform.localEulerAngles.y, 0);
            
            //If the raycast distance from the previous object, is larger than 4, lerp the current object closer to the previous object.
            if (hitTable.distance > 4)
            {
                MoveOut(table);
                table.transform.position = Vector3.Lerp(table.transform.position, sofa.transform.position, 0.5f);
            }
            else
            {  
                //If not, keep it along the wall
                MoveOut(table);
            }
        }
    }

    //Finds two points along a wall, and lerps the item to a random spot along the wall.
    private void RandomPositionOnWall(GameObject item)
    {
        Ray ray2 = new Ray(item.transform.position, item.transform.right);
        RaycastHit hit2;
        
        Ray ray3 = new Ray(item.transform.position, -item.transform.right);
        RaycastHit hit3;

        if (Physics.Raycast(ray2, out hit2) && Physics.Raycast(ray3, out hit3))
        {
            item.transform.position = Vector3.Lerp(hit2.point, hit3.point, Random.Range(0.2f, 0.8f));
        }
    }

    //Generates the KitchenRoom
    private void KitchenRoom(Vector3 centerPoint)
    {
        //Gets a random kitchen object, instantiates an object, adds it to the object list, and sets the objects rotation to the random wall chosen's rotation.
        //Item gets added to the object list after.
        var randomKitchenDigit = RandomKitchenObject();
        var firstObject = Instantiate(Objects[randomKitchenDigit], centerPoint, quaternion.identity);
        firstObject.transform.localEulerAngles = new Vector3(0, hits[randomWall].transform.localEulerAngles.y, 0);
        objectList[0].list.Add(firstObject);
        
        //Shoots a new raycast backwards from the instantiated object.
        Ray ray = new Ray(firstObject.transform.position, -firstObject.transform.forward);
        RaycastHit hit;

        //Based on the raycast, moves the object back to the wall that the raycast hit.
        if (Physics.Raycast(ray, out hit))
        {
            firstObject.transform.position -= firstObject.transform.forward * hit.distance;
        }
        
        //Uses three functions to move the object out from the wall, and down to the floor, and after, a random position along the wall.
        MoveDown(firstObject);
        MoveOut(firstObject);
        RandomPositionOnWall(firstObject);
        
        //Sets the first instantiated object to thisObject and lastObject, so the first loop is based on the first object.
        var thisObject = firstObject;
        var lastObject = firstObject;
        
        //For loop that generates the kitchen based on the first instantiated object.
        for (int i = 0; i < 10; i++)
        {
            thisObject = lastObject;
            
            //Shoots a new raycast to the right of the previous object.
            Ray loopRay = new Ray(lastObject.transform.position, lastObject.transform.right);
            RaycastHit loopHit;
            
            if (Physics.Raycast(loopRay, out loopHit))
            {
                if (loopHit.distance > 0.7f)
                {
                    //Instantiates a new random kitchen object, based on the previous object, and adds it to the object list.
                    thisObject = Instantiate(Objects[RandomKitchenObject()], lastObject.transform.position, lastObject.transform.rotation);
                    objectList[0].list.Add(thisObject);
                    
                    //Gets the new instantiated objects collider, and uses the colliders size to move the object to the right.
                    Collider thisObjectCol = thisObject.GetComponent<Collider>();
                    thisObject.transform.position += thisObject.transform.right * (thisObjectCol.bounds.extents.magnitude / 2);

                    //Sets the current instantiated object, to the last object, and runs the loop again.
                    lastObject = thisObject;
                }
            }
        }
    }

    //Gives a random number between 2-8, and returns it. 8 is excluded.
    private int RandomKitchenObject()
    {
        return Random.Range(2, 8);
    }

    //Goes along the object list, and deletes all objects in the list.
    private void DeleteObjects()
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            for (int j = 0; j < objectList[i].list.Count; j++)
            {
                Destroy(objectList[i].list[j].gameObject);
            }
        }
    }

    //Takes the collider of the item, and moves the item forward based on the colliders size.
    private void MoveOut(GameObject item)
    {
        Collider itemCollider = item.GetComponent<Collider>();
        item.transform.position += item.transform.forward * ((itemCollider.bounds.extents.magnitude / 2) / 2);
    }

    //Sends a raycast downwards from the item, and moves the item down based on the raycast distance.
    private void MoveDown(GameObject item)
    {
        Ray rayDown = new Ray(item.transform.position, -item.transform.up);
        RaycastHit hitDown;
        
        if (Physics.Raycast(rayDown, out hitDown))
        {
            item.transform.position -= item.transform.up * hitDown.distance;
        }
    }
}


