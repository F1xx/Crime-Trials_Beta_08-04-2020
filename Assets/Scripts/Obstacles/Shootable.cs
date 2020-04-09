using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shootable : BaseObject
{
    [Header("Shootable"), SerializeField]
    private bool CanBeShot = true;

    private bool StartValue;

    protected override void Awake()
    {
        base.Awake();

        StartValue = CanBeShot;
    }

#if UNITY_EDITOR
private void OnValidate()
    {
        StartValue = CanBeShot;
    }
#endif

    public void SetCanBeShot(bool value)
    {
        CanBeShot = value;
    }

    public void TriggerShotEvent()
    {
        EventManager.TriggerEvent("OnShootableShot");
    }

    public bool GetCanBeShot()
    {
        return CanBeShot;
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        CanBeShot = StartValue;
    }
}
