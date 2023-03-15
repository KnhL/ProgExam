using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClampAxis
{
    x,
    y,
    z
}

public class BoundaryPoint : MonoBehaviour
{
    [SerializeField] private ClampAxis clampAxis;
    [SerializeField] private Vector2 distanceClamp;

    private bool isPressed;
    private bool justPressed;

    private Vector3 startPos;

    private Material thisMat;

    private void Start()
    {
        startPos = transform.position;
        
        thisMat = GetComponent<Renderer>().materials[1];
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isPressed)
        {
            if (justPressed)
            {
                isPressed = false;
                thisMat.SetFloat("_Size", 0);
            }
            
            justPressed = true;
        }
        
        if (isPressed)
        {
            switch (clampAxis)
            {
                case ClampAxis.x:
                    transform.position += new Vector3(Input.mouseScrollDelta.y,0,0);
                    break;
                
                case ClampAxis.y:
                    transform.position += new Vector3(0, Input.mouseScrollDelta.y, 0);
                    break;
                
                case ClampAxis.z:
                    transform.position += new Vector3(0, 0, Input.mouseScrollDelta.y);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            transform.position = VectorClamp(transform.position, distanceClamp, clampAxis);
        }
    }

    private void OnMouseDown()
    {
        isPressed = true;
        justPressed = false;
        thisMat.SetFloat("_Size", 0.7f);
    }

    private Vector3 VectorClamp(Vector3 target, Vector2 clamp, ClampAxis cAxis)
    {
        switch (cAxis)
        {
            case ClampAxis.x:
                target.x = Mathf.Clamp(target.x, clamp.x, clamp.y);
                target.y = startPos.y;
                target.z = startPos.z;
                return target;
            
            case ClampAxis.y:
                target.x = startPos.x;
                target.y = Mathf.Clamp(target.y, clamp.x, clamp.y);
                target.z = startPos.z;
                return target;
            
            case ClampAxis.z:
                target.x = startPos.x;
                target.y = startPos.y;
                target.z = Mathf.Clamp(target.z, clamp.x, clamp.y);
                return target;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(cAxis), cAxis, null);
        }
    }
}
