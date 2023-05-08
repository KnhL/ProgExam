using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDolley : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1;
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");

            transform.eulerAngles += new Vector3(0, mouseX * rotationSpeed, 0);
            
        }
    }
}
