using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unlock details:
///   - Player cannot die
///   - Player cannot reset.
///   - Reach end of the level.
/// </summary>
public class Unlock_SpecOps : UnlockBase
{

    protected override void Awake()
    {
        base.Awake();

        UnlockKey = "Arm";
        UnlockValue = 4;
        UnlockDisplayText = "Spec-Ops Arm!";

        Listen("OnPlayerRespawnEvent", OnPlayerRespawnEvent);
        Listen("OnLevelComplete", OnLevelCompleteEvent);

    }

    private void OnPlayerRespawnEvent()
    {
        IsActive = false;
    }

    private void OnLevelCompleteEvent(EventParam param)
    {
        ActivateUnlock();
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        IsActive = false;
    }
}
