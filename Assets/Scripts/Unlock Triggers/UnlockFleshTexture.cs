using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockFleshTexture : UnlockBase
{
    protected override void Awake()
    {
        base.Awake();

        UnlockKey = "Arm";
        UnlockValue = 3;
        UnlockDisplayText = "Skin Arm!";
    }

    private void OnTriggerEnter(Collider other)
    {
        string name = other.gameObject.transform.root.name;

        if (name.Contains("Player") && name.Contains("Projectile") == false)
        {
            ActivateUnlock();
            EventManager.TriggerEvent("SkinArmUnlock");
        }
    }
}
