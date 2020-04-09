using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTurretWaypoints : MonoBehaviour
{
    [Tooltip("i think its pretty obvious")]
    public float DebugCircleRadius = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(gameObject.transform.position, DebugCircleRadius);
    }
}
