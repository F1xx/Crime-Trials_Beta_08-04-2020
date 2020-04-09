using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unlock details:
///   - Reach end of level in less than 2 minutes.
/// </summary>
public class Unlock_Gold : UnlockBase
{
    protected override void Awake()
    {
        base.Awake();

        UnlockKey = "Arm";
        UnlockValue = 5;
        UnlockDisplayText = "The Golden Arm!";

        Listen("OnLevelComplete", OnLevelCompleteEvent);

    }


    private void OnLevelCompleteEvent(EventParam param)
    {
        if (WorldTimeManager.TimePassed < 120.0f)
        {
            ActivateUnlock();
        }
    }
}
