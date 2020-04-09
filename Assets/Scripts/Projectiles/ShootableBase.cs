using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShootableBase : BaseObject
{
    [Header("ShootableBase Settings")]
    public float Speed = 50.0f;
    public float FireRate = 4.0f;
    public float LifeTime = 5.0f;

    [HideInInspector]
    public PooledObject Pool = null; //the component that shot us
    [HideInInspector]
    public GameObject BarrelPosition = null; //a reference to the location that shot us (used to keep muzzleflash on point)
    protected abstract void Init();

    public virtual void Shoot(GameObject SpawnLoc, PooledObject pool, Quaternion dir, GameObject parent)
    {
        transform.root.transform.rotation = dir;
        transform.root.transform.position = SpawnLoc.transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        Pool = pool;
        BarrelPosition = SpawnLoc;

        TrailRenderer rend = GetComponent<TrailRenderer>();

        //set parenting
        ParentObject = parent;
        PainVolume pv = GetComponentInChildren<PainVolume>();
        if (pv != null)
        {
            pv.ParentObject = ParentObject;
        }

        Init();
    }
}
