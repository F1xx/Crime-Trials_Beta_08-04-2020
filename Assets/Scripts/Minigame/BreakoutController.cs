using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakoutController : InputComponentBase
{

    public void Update()
    {
        if (IsResetting())
        {
            EventManager.TriggerEvent("OnSoftReset");
        }
    }

    public override void Init(Character character)
    {
        //throw new NotImplementedException();
    }

    public override void UpdateControls()
    {
        //throw new NotImplementedException();
    }

    public override void SetFacingDirection(Vector3 direction)
    {
        //throw new NotImplementedException();
    }

    public override Vector3 GetControlRotation()
    {
        //throw new NotImplementedException();
        return Vector3.zero;
    }

    public override Vector3 GetMoveInput()
    {
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

            input = Vector3.ClampMagnitude(input, 1.0f);

            return input;
        }
    }

    public override Vector3 GetRelativeMoveInput()
    {
        //throw new NotImplementedException();
        return Vector3.zero;
    }

    public override Vector3 GetLookInput()
    {
        //throw new NotImplementedException();
        return Vector3.zero;
    }

    public override Vector3 GetAimTarget()
    {
        //throw new NotImplementedException();
        return Vector3.zero;
    }

    public override bool IsJumping()
    {
        //throw new NotImplementedException();
        return Input.GetButtonDown("Submit");
    }

    public override bool IsFiring()
    {
        //throw new NotImplementedException();
        return false;
    }

    public override bool IsAiming()
    {
        //throw new NotImplementedException();
        return false;
    }

    public override bool IsCrouching()
    {
        //throw new NotImplementedException();
        return false;
    }

    public bool IsResetting()
    {
        return Input.GetButtonDown("Reset");
    }
}
