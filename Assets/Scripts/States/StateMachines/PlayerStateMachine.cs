using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachineBase
{
    public bool IsHoldingJumpDown = false;

    protected override void Start()
    {
        base.Start();

        Listen("OnPlayerJumpReleased", OnJumpReleased);
        Listen("OnPlayerJumpPressed", OnJumpPressed);
    }

    protected override void OnDeathEvent(EventParam param)
    {
        SetMovementStateForced<StateDisabled>();
    }

    protected override void OnRespawnEvent()
    {
        SetMovementStateForced<StateOnGround>();
    }

    void OnJumpPressed()
    {
        IsHoldingJumpDown = true;
    }

    void OnJumpReleased()
    {
        IsHoldingJumpDown = false;
    }
}
