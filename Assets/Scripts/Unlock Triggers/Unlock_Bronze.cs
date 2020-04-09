using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unlock details:
///   - Reach end of level.
/// </summary>
public class Unlock_Bronze : UnlockBase
{
    protected override void Awake()
    {
        base.Awake();

        UnlockKey = "Arm";
        UnlockValue = 2;
        UnlockDisplayText = "Bronze Arm!";

        Listen("OnLevelComplete", OnLevelCompleteEvent);

    }

    private void OnLevelCompleteEvent(EventParam param)
    {
        ActivateUnlock();
    }
}
