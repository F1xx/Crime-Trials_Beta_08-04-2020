using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateAngularVelocity : MonoBehaviour
{
    
    float lastAngle; 
    float newAngle;
 
    void Start()
    {
    }
 
    private void Awake()
    {
        lastAngle = this.gameObject.transform.parent.transform.rotation.eulerAngles.magnitude;
      
    }
 
    public void FixedUpdate()
    {
      
        newAngle = this.gameObject.transform.parent.transform.rotation.eulerAngles.magnitude - lastAngle;
        lastAngle = this.gameObject.transform.rotation.eulerAngles.magnitude;
 
    }
 
    public float AngularVelocity()
    {
         return newAngle;
    }
}
