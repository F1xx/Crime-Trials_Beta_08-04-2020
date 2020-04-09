using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootComponent : ShootComponent
{
    [Tooltip("If this is on then Aim Assist will target the closest object it can find, if off it will target the thing closest to the center of the cross hair")]
    public bool ShootCloser = false;

    [SerializeField]
    ShootableBase[] ShootableBullets = null;

    protected override void Awake()
    {
        base.Awake();
        Listen("CurrentPlayerArm", LoadNewBullet);
        LoadSettings();

        if (FirePoint == null)
        {
            FirePoint = GameObject.Find("ThisWhereBoomHappen");
        }
    }


    void LoadSettings()
    {
        LoadNewBullet();
    }

    void LoadNewBullet()
    {
        int val = PlayerPrefs.GetInt("CurrentPlayerArm", 0);

        ChangeShootableObject(ShootableBullets[val]);
    }

    protected override Quaternion DetermineDirection()
    {
        if (PlayerPrefs.HasKey("OnToggleAimAssist"))
        {
            int x = PlayerPrefs.GetInt("OnToggleAimAssist");
            bool y = Convert.ToBoolean(x);

            if (y)
            {
                return HandleAimAssist();
            }
        }

        return HandlePlainAim();
    }

    Quaternion HandlePlainAim()
    {
        Ray ray = Camera.main.ViewportPointToRay(PlayerCrosshair.AimLocation.pivot);

        Quaternion dir = Quaternion.LookRotation(ray.direction, Vector3.up);

        return dir;
    }

    Quaternion HandleAimAssist()
    {
        Quaternion dir = FirePoint.transform.rotation;
        Ray ray = Camera.main.ViewportPointToRay(PlayerCrosshair.AimLocation.pivot);

        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        List<RaycastHit> hitInfo = new List<RaycastHit>();
        //If we found something to hit we need to find the best one to consider a target
        if(MathUtils.SphereCastAllFromScreenCenterIgnoringNonVisibleObjects(Camera.main, ray, PlayerCrosshair.Radius, out hitInfo, Mathf.Infinity, layerMask, "Shootable"))
        {
            RaycastHit target = new RaycastHit();
            bool didhit = false;

            if (ShootCloser)
            {
                didhit = FindClosestTarget(hitInfo, out target);
            }
            else
            {
                didhit = FindCenterMosttarget(hitInfo, out target);
            }

            if(!didhit)
            {
                return HandlePlainAim();
            }

            dir = GetDirectionToTarget(target.point, dir);
        }
        else
        {
            dir = HandlePlainAim();
        }


        return dir;
    }

    Quaternion GetDirectionToTarget(Vector3 target, Quaternion dir)
    {
        Transform point = FirePoint.transform;

        Vector3 direction = (target - point.position).normalized;

        float dot = Vector3.Dot(direction, point.forward.normalized);

        dir.SetLookRotation(direction);

        if (dot < 0.8f)
        {
            dir = HandlePlainAim();
        }

        return dir;
    }

    /// <summary>
    /// Note this works until the radius of the check gets absurd. Then it starts being weird
    /// </summary>
    /// <param name="hitInfo"></param>
    /// <returns></returns>
    bool FindClosestTarget(List<RaycastHit> hitInfo, out RaycastHit target)
    {
        target = new RaycastHit();
        float dist = float.MaxValue;
        bool TargetWasSet = false;

        foreach(RaycastHit hit in hitInfo)
        {
            //first check that it can actually be shot
            Shootable shootable = hit.collider.transform.root.gameObject.GetComponentInChildren<Shootable>();

            if (shootable)
            {
                if (shootable.GetCanBeShot() == false)
                {
                    continue;
                }
            }

            float tempDist = Vector3.Distance(FirePoint.transform.position, hit.collider.transform.position);

            if(tempDist < dist)
            {
                TargetWasSet = true;
                target = hit;
                dist = tempDist;
            }
        }

        return TargetWasSet;
    }

    bool FindCenterMosttarget(List<RaycastHit> hitInfo, out RaycastHit target)
    {
        target = new RaycastHit();
        float dist = float.MaxValue;
        bool TargetWasSet = false;

        foreach (RaycastHit hit in hitInfo)
        {
            //first check that it can actually be shot
            Shootable shootable = hit.collider.transform.root.gameObject.GetComponentInChildren<Shootable>();

            if (shootable)
            {
                if (shootable.GetCanBeShot() == false)
                {
                    continue;
                }
            }

            Vector2 pos = Camera.main.WorldToViewportPoint(hit.point, Camera.MonoOrStereoscopicEye.Mono);

            float tempDist = Vector2.Distance(PlayerCrosshair.AimLocation.pivot, pos);

            if (tempDist < dist)
            {
                TargetWasSet = true;
                target = hit;
                dist = tempDist;
            }
        }

        return TargetWasSet;
    }
}
