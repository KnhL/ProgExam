using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGeneration : MonoBehaviour
{
    private RaycastHit[] hits;
    public List<GameObject> wallList;

    // Start is called before the first frame update
    void Start()
    {
        wallList = new List<GameObject>();
        findObjects();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void findObjects()
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

        Debug.Log("Wall list:");
        foreach (GameObject wall in wallList)
        {
            Debug.Log(wall.name);
        }
    }
}
