using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ClampAxis
{
    x,
    y,
    z
}

public class BoundaryPoint : MonoBehaviour
{
    [SerializeField] private HouseGenerator houseGenerator;
    [SerializeField] private ClampAxis clampAxis;
    [SerializeField] private Vector2 distanceClamp;
    [SerializeField] private LayerMask layerMask;

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
                UnHighlight();
            }
            
            justPressed = true;
        }
        
        if (isPressed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            Vector3 position = transform.position;
            
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.magenta);

            switch (clampAxis)
            {
                case ClampAxis.x:
                    if (Physics.Raycast(ray, out var hit1, Mathf.Infinity, layerMask))
                    {
                        transform.position = new Vector3(hit1.point.x, position.y, position.z);
                    }
                    break;
                
                case ClampAxis.y:
                    if (Physics.Raycast(ray, out var hit2, Mathf.Infinity, layerMask))
                    {
                        transform.position = new Vector3(position.x, hit2.point.y, position.z);
                    }
                    break;
                
                case ClampAxis.z:
                    if (Physics.Raycast(ray, out var hit3, Mathf.Infinity, layerMask))
                    {
                        transform.position = new Vector3(position.x, position.y, hit3.point.z);
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            transform.position = VectorClamp(transform.position,position, distanceClamp, clampAxis);
        }
    }

    private void OnMouseDown()
    {
        if (isPressed)
        {
            UnHighlight();
            return;
        }
        
        isPressed = true;
        justPressed = false;
        thisMat.SetFloat("_Size", 0.2f);
        
    }

    private void UnHighlight()
    {
        isPressed = false;
        thisMat.SetFloat("_Size", -.5f);
    }

    private Vector3 VectorClamp(Vector3 target, Vector3 currentPos, Vector2 clamp, ClampAxis cAxis)
    {
        switch (cAxis)
        {
            case ClampAxis.x:
                target.x = Mathf.Clamp(target.x, clamp.x, clamp.y);
                target.y = currentPos.y;
                target.z = currentPos.z;
                return target;
            
            case ClampAxis.y:
                target.x = currentPos.x;
                target.y = Mathf.Clamp(target.y, clamp.x, clamp.y);
                target.z = currentPos.z;
                return target;
            
            case ClampAxis.z:
                target.x = currentPos.x;
                target.y = currentPos.y;
                target.z = Mathf.Clamp(target.z, clamp.x, clamp.y);
                return target;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(cAxis), cAxis, null);
        }
    }
}
