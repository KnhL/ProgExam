using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private bool update;
    
    [SerializeField] private Vector3 maxBounds;
    [SerializeField] private Vector3 colCenter;

    [SerializeField] private Vector3 leftSide;
    [SerializeField] private Vector3 rightSide;
    [SerializeField] private Vector3 frontSide;
    [SerializeField] private Vector3 backSide;
    
    [SerializeField] private BoxCollider col;
    
    private void Update()
    {
        maxBounds = col.bounds.max;
        colCenter = col.center;
        leftSide = new Vector3(transform.position.x, transform.position.y,
            transform.position.z + col.bounds.max.z);
    }

    private void OnValidate()
    {
        maxBounds = col.bounds.max;
        colCenter = col.center;
        leftSide = FindBoundSide(Directions.Left);
        rightSide = FindBoundSide(Directions.Right);
        frontSide = FindBoundSide(Directions.Front);
        backSide = FindBoundSide(Directions.Back);
        update = false;
    }

    private Vector3 FindBoundSide(Directions direction)
    {
        switch (direction)
        {
            case Directions.Left:
                return new Vector3(colCenter.x, colCenter.y,
                    col.bounds.max.z);
            case Directions.Right:
                return new Vector3(colCenter.x, colCenter.y,
                    colCenter.z+ (col.bounds.min.z));
            case Directions.Front:
                return new Vector3(col.bounds.max.x, colCenter.y,
                    colCenter.z);
            case Directions.Back:
                return new Vector3(col.bounds.min.x, colCenter.y,
                    colCenter.z);
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
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
