using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockBase : BaseObject
{

    [SerializeField, Header("Table Settings"), Tooltip("The Unlocks table key.")]
    protected string UnlockKey = "";

    [SerializeField, Tooltip("The Unlocks table value.")]
    protected int UnlockValue = 0;

    [SerializeField, Header("Pop-up Settings"), Tooltip("The Unlock pop-up display sprite.")]
    protected Sprite UnlockDisplaySprite;

    [SerializeField, Tooltip("The Unlock pop-up display text.")]
    protected string UnlockDisplayText;

    [SerializeField, Tooltip("Whether or not this Unlock Trigger is able to unlock whatever it's meant to.")]
    protected bool IsActive = true;


    public void Start()
    {
        if (UnlockKey == "")
        {
            Debug.LogWarning("UnlockBase no set.");
            IsActive = false;
        }
        else if (UnlockValue == 0)
        {
            Debug.LogWarning("UnlockBase no set.");
            IsActive = false;
        }
    }


    protected virtual void ActivateUnlock()
    {
        if (IsActive)
        {
            if (SQLManager.Instance().LoadedUnlocks[UnlockKey][UnlockValue - 1] == false)
            {
                OnUnlockParam param = new OnUnlockParam(UnlockDisplaySprite, UnlockDisplayText, UnlockValue, UnlockKey);
                EventManager.TriggerEvent("OnUnlockArm", param);
            }
        }
    }

}
