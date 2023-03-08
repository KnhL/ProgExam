using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGeneration : MonoBehaviour
{
    Collider m_Collider;
    Vector3 m_Center;
    Vector3 m_Size, m_Min, m_Max;

    // Start is called before the first frame update
    void Start()
    {
        m_Collider = GetComponent<Collider>();
        
    }

    public void GetColliderBoundaries()
    {
        var bounds = m_Collider.bounds;
        m_Center = bounds.center;
        m_Size = bounds.size;
        m_Min = bounds.min;
        m_Max = bounds.max;
    }

    public void Kitchen()
    {
        
        
    }

    public void LivingRoom()
    {
        
        
    }

    public void Bedroom()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
