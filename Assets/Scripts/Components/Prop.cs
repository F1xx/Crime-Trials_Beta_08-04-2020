using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Prop : BaseObject
{
    Vector3 StartPos = Vector3.zero;
    Quaternion StartRot = Quaternion.identity;

    Rigidbody RB = null;

    // Start is called before the first frame update
    void Start()
    {
        StartPos = transform.position;
        StartRot = transform.rotation;

        RB = GetComponent<Rigidbody>();

#if DEBUG_ENABLED
        if(RB == null)
        {
            Debug.LogError("This Prop: " + gameObject.name + " requires a RigidBody");
        }
#endif
    }

    protected override void OnSoftReset()
    {
        RB.velocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;

        transform.position = StartPos;
        transform.rotation = StartRot;
    }
}
