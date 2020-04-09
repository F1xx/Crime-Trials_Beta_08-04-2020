using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIInputComponent : InputComponentBase
{

    public override void Init(Character character)
    {

    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    public override void UpdateControls()
    {
    }

    public override void SetFacingDirection(Vector3 direction)
    {
    }

    public override Vector3 GetControlRotation()
    {

        return Vector3.zero;
    }

    public override Vector3 GetMoveInput()
    {

        return Vector3.zero;
    }

    public override Vector3 GetLookInput()
    {

        return Vector3.zero;
    }

    public override Vector3 GetAimTarget()
    {
        return Vector3.zero;
    }

    public override bool IsJumping()
    {
        return false;
    }

    public override bool IsFiring()
    {
        return false;
    }

    public override bool IsAiming()
    {
        return false;
    }

    public override bool IsCrouching()
    {
        return false;
    }

    public override Vector3 GetRelativeMoveInput()
    {
        return Vector3.zero;
    }

    public override float GetRotationSpeed()
    {
        return 0.0f;
    }
}
