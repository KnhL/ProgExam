using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGeneration : MonoBehaviour
{
    [Serializable] private class RoomList
    {
        public string name;
        public List<GameObject> list;
    }

    private enum Generate
    {
        LivingRoom,
        KitchenRoom,
    }
    
    [Header("Settings")]
    [SerializeField] private Generate generationType;
    
    private RaycastHit[] hits;
    
    [Header("Variables")]
    public List<GameObject> wallList;
    [SerializeField] private List<RoomList> objectList;
    private int randomWall;

    [Header("Spawned Objects")]
    [SerializeField] private GameObject[] Objects;

    private HouseGenerator _houseGenerator;
    
    // Start is called before the first frame update
    void Start()
    {
        randomWall = Random.Range(0, 4);
        wallList = new List<GameObject>();
        //FindObjects();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateRooms()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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
    }

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

    private void LivingRoom(Vector3 centerPoint)
    {
        var sofa = Instantiate(Objects[11], centerPoint, quaternion.identity);
        objectList[1].list.Add(sofa);
        sofa.transform.localEulerAngles = new Vector3(0, hits[randomWall].transform.localEulerAngles.y, 0);
        
        Ray ray = new Ray(sofa.transform.position, -sofa.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            sofa.transform.position -= sofa.transform.forward * hit.distance;
        }

        MoveOut(sofa);
        MoveDown(sofa);
        
        RandomPositionOnWall(sofa);

        Ray rayTable = new Ray(sofa.transform.position, sofa.transform.forward);
        RaycastHit hitTable;

        if (Physics.Raycast(rayTable, out hitTable))
        {
            var tvOrNot = Random.Range(17, 19);
            var table = Instantiate(Objects[tvOrNot], hitTable.point, quaternion.identity);
            objectList[1].list.Add(table);
            
            table.transform.localEulerAngles = new Vector3(0, hitTable.transform.localEulerAngles.y, 0);
            
            if (hitTable.distance > 4)
            {
                MoveOut(table);
                table.transform.position = Vector3.Lerp(table.transform.position, sofa.transform.position, 0.5f);
            }
            else
            {  
                MoveOut(table);
            }
        }
    }

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

    private void KitchenRoom(Vector3 centerPoint)
    {
        var randomKitchenDigit = RandomKitchenObject();
        var firstObject = Instantiate(Objects[randomKitchenDigit], centerPoint, quaternion.identity);
        objectList[0].list.Add(firstObject);
        firstObject.transform.localEulerAngles = new Vector3(0, hits[randomWall].transform.localEulerAngles.y, 0);
        
        Ray ray = new Ray(firstObject.transform.position, -firstObject.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            firstObject.transform.position -= firstObject.transform.forward * hit.distance;
        }
        
        MoveDown(firstObject);
        MoveOut(firstObject);
        
        RandomPositionOnWall(firstObject);
        
        var thisObject = firstObject;
        var lastObject = firstObject;
        
        for (int i = 0; i < 10; i++)
        {
            thisObject = lastObject;
            Ray loopRay = new Ray(lastObject.transform.position, lastObject.transform.right);
            RaycastHit loopHit;
            
            if (Physics.Raycast(loopRay, out loopHit))
            {
                if (loopHit.distance > 0.7f)
                {
                    thisObject = Instantiate(Objects[RandomKitchenObject()], lastObject.transform.position, lastObject.transform.rotation);
                    objectList[0].list.Add(thisObject);
                    Collider thisObjectCol = thisObject.GetComponent<Collider>();
                    thisObject.transform.position += thisObject.transform.right * (thisObjectCol.bounds.extents.magnitude / 2);

                    lastObject = thisObject;
                }
            }
        }
    }

    private int RandomKitchenObject()
    {
        return Random.Range(2, 8);
    }

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

    private void MoveOut(GameObject item)
    {
        Collider itemCollider = item.GetComponent<Collider>();
        item.transform.position += item.transform.forward * ((itemCollider.bounds.extents.magnitude / 2) / 2);
    }

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


