using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : Hazard
{

    Vector3 OriginalPos = Vector3.zero;
    bool HasLeft = false;

    Rigidbody rigidBody;
    [Header("Movement Settings")]
    public bool ShouldMove = true;

    public float Amplitude = 20.0f;

    public float XPushForce = 1.0f;
    public float YPushForce = 1.0f;
    public float ZPushForce = 1.0f;

    public bool MoveAlongX = true;
    public bool MoveAlongY = true;
    public bool MoveAlongZ = false;

    [Header("Rotation Settings")]
    public bool ShouldRotate = false;

    public bool RotateOnX = false;
    public bool RotateOnY = true;
    public bool RotateOnZ = false;

    public float XRotateSpeed = 1.0f;
    public float YRotateSpeed = 1.0f;
    public float ZRotateSpeed = 1.0f;
    public float MaximumRotateSpeed = 10.0f;



    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        OriginalPos = this.gameObject.transform.position;

        rigidBody = GetComponent<Rigidbody>();

        if (ShouldMove)
        {
            if (!MoveAlongX)
            {
                XPushForce = 0.0f;
            }

            if (!MoveAlongY)
            {
                YPushForce = 0.0f;
            }

            if (!MoveAlongZ)
            {
                ZPushForce = 0.0f;
            }
            //initial push to start it moving
            Push();
        }
       if (ShouldRotate)
       {
       
           if (!RotateOnX)
           {
               XRotateSpeed = 0.0f;
           }
       
           if (!RotateOnY)
           {
                YRotateSpeed = 0.0f;
           }
       
           if (!RotateOnZ)
           {
                ZRotateSpeed = 0.0f;
           }
        }
    }

    void FixedUpdate()
    {
        if(ShouldMove)
        {
            if(Mathf.Abs(CheckDistTraveled()) > Amplitude )
            {
                Push();
                HasLeft = true;
            }
            if(MoveAlongX)
            {

                if (MathUtils.AlmostEquals(transform.position.x, OriginalPos.x, 1.0f) && HasLeft)
                {
                    HasLeft = false;
                    Push();
                }
            }
            else if (MoveAlongY)
            {

                if (MathUtils.AlmostEquals(transform.position.y, OriginalPos.y, 1.0f) && HasLeft)
                {
                    HasLeft = false;
                    Push();
                }
            }
            else if (MoveAlongZ)
            {

                if (MathUtils.AlmostEquals(transform.position.z, OriginalPos.z, 1.0f) && HasLeft)
                {
                    HasLeft = false;
                    Push();
                }
            }
        }

        if (ShouldRotate)
        {
            this.gameObject.transform.Rotate(new Vector3(XRotateSpeed, YRotateSpeed, ZRotateSpeed));
            //if(rigidBody.angularVelocity.magnitude < 10.0f)
            //{
            //    rigidBody.AddTorque(XRotateSpeed, YRotateSpeed, ZRotateSpeed);
            //}
        }
    }

    public void Push()
    {
        //clear all movement on the platform 
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.AddForce(XPushForce, YPushForce, ZPushForce);
        //once the force has been applied, reverse it so next time it is pushed it will be pushed in the opposite direction
        XPushForce = -XPushForce;
        YPushForce = -YPushForce;
        ZPushForce = -ZPushForce;
    }
    private float CheckDistTraveled()
    {
        return (transform.position - OriginalPos).magnitude;
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
    }
}

//public class MovingPlatform : Hazard
//{

//    Rigidbody rigidBody;
//    [Header("Movement Settings")]
//    public bool ShouldMove = true;

//    public float YAmplitudeValue = 1.0f;
//    public float ZAmplitudeValue = 1.0f;

//    public float YFrequency = 1.0f;
//    public float ZFrequency = 1.0f;

//    public bool MoveAlongY = true;
//    public bool MoveAlongZ = false;

//    [Header("Rotation Settings")]
//    public bool ShouldRotate = false;

//    public float XRotateSpeed = 1.0f;
//    public float YRotateSpeed = 1.0f;
//    public float ZRotateSpeed = 1.0f;



//    protected override void Awake()
//    {
//        base.Awake();
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
//        rigidBody = GetComponent<Rigidbody>();

//        rigidBody.isKinematic = true;
//        RaycastHit YCast = new RaycastHit();
//        RaycastHit ZCast = new RaycastHit();
//        if (Physics.Raycast(this.gameObject.transform.position, transform.up, out YCast))
//        {
//            if(((YCast.distance / 2) - 3) < YAmplitudeValue)
//                YAmplitudeValue = (YCast.distance / 2) - 3; // player height plus one more unit for safty
//        }
//        if (Physics.Raycast(this.gameObject.transform.position, transform.forward, out ZCast))
//        {
//            if((ZCast.distance / 2) - (this.gameObject.transform.localScale.z / 2) < ZAmplitudeValue)
//                ZAmplitudeValue = (ZCast.distance / 2) - (this.gameObject.transform.localScale.z / 2);
//        }
//    }

//    void FixedUpdate()
//    {
//        if (ShouldMove)
//        {
//            float y = 0.0f;
//            float z = 0.0f;

//            if (MoveAlongY)
//            {
//                y = YAmplitudeValue;
//            }
//            else
//            {
//                y = 0.0f;
//            }

//            if (MoveAlongZ)
//            {
//                z = ZAmplitudeValue;
//            }
//            else
//            {
//                z = 0.0f;
//            }

//            this.gameObject.transform.Translate(new Vector3( 0.0f, y * Mathf.Sin(YFrequency * Time.time) * Time.deltaTime, z * Mathf.Sin(ZFrequency * Time.time) * Time.fixedDeltaTime));
//        }
//        if(ShouldRotate)
//        {
//            this.gameObject.transform.Rotate(new Vector3(XRotateSpeed, YRotateSpeed, ZRotateSpeed));
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {

//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        //can add turret once turret respawn is figured out when parented to the platform
//        if(other.tag == "Player" /*|| other.tag == "GroundTurret"*/)
//            other.transform.SetParent(this.gameObject.transform);
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.tag == "Player" /*|| other.tag == "GroundTurret"*/)
//            other.transform.SetParent(null);
//    }

//    protected override void OnSoftReset()
//    {
//        base.OnSoftReset();
//    }
//}
