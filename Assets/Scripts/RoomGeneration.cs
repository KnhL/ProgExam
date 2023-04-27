using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGeneration : MonoBehaviour
{
    private RaycastHit[] hits;
    public List<GameObject> wallList;
    private int randomWall;
    
    private Vector3 randomWallMaxBound;
    private Vector3 randomWallMinBound;

    [SerializeField] private GameObject testCube;
    [SerializeField] private GameObject[] Objects;
     
    // Start is called before the first frame update
    void Start()
    {
        randomWall = Random.Range(0, 4);
        wallList = new List<GameObject>();
        FindObjects();
        TvAndSofa();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            randomWall = Random.Range(0, 4);
            FindObjects();
            //LivingRoom();
            TvAndSofa();
        }
    }

    public void FindObjects()
    {
        Vector3[] directions = { transform.forward, transform.right, -transform.right, -transform.forward, -transform.up };
        hits = new RaycastHit[5];
        for (int i = 0; i < directions.Length; i++)
        {
            Ray ray = new Ray(transform.position, directions[i]);
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
        //Debug.Log("Wall list:");
        //foreach (GameObject wall in wallList)
        //{
        //    Debug.Log(wall.name);
        //}
    }

    private void LivingRoom()
    {
         
         randomWallMaxBound = wallList[randomWall].GetComponent<Collider>().bounds.max;
         randomWallMinBound = wallList[randomWall].GetComponent<Collider>().bounds.min;
         
         Instantiate(testCube, randomWallMaxBound, quaternion.identity);
         Instantiate(testCube, randomWallMinBound, quaternion.identity);
         

         var item = Instantiate(Objects[11], transform.position, quaternion.identity);
         item.transform.localEulerAngles = new Vector3(0, hits[randomWall].transform.localEulerAngles.y, 0);
         
         MoveOut(item);
         
    }
    
    private void TvAndSofa()
    {
        var item = Instantiate(Objects[11], transform.position, quaternion.identity);
        item.transform.localEulerAngles = new Vector3(0, hits[randomWall].transform.localEulerAngles.y, 0);
         
        MoveOut(item);
        
        Ray rayTable = new Ray(item.transform.position, item.transform.forward);
        RaycastHit hitTable;
        
        if (Physics.Raycast(rayTable, out hitTable))
        {
            var Table = Instantiate(Objects[2], transform.position, quaternion.identity);
            Table.transform.localEulerAngles = new Vector3(0, hitTable.transform.localEulerAngles.y, 0);
    
            
            if (hitTable.distance > 3)
            {        
                Table.transform.position = hitTable.point;
                MoveOut(Table);   
            }
            else
            {
                //Table.transform.position = hitTable.transform.position;
            }
        }
    }

    private void MoveOut(GameObject item)
    {
        Ray ray = new Ray(item.transform.position, -item.transform.forward);
        RaycastHit hit;
        Collider itemCollider = item.GetComponent<Collider>();
        
        if (Physics.Raycast(ray, out hit))
        {
            print("move out hit " + hit.transform.name);
            item.transform.position -= item.transform.forward * hit.distance;
            item.transform.position += item.transform.forward * (itemCollider.bounds.extents.magnitude / 2);
            print(itemCollider.bounds.extents.magnitude / 2);
        }
        
        Ray rayDown = new Ray(item.transform.position, -item.transform.up);
        RaycastHit hitDown;
        
        if (Physics.Raycast(rayDown, out hitDown))
        {
            item.transform.position -= item.transform.up * hitDown.distance;
        }
    }
}


