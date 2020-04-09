using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputComponentBase : BaseObject
{
    public bool IsDisabled { get; protected set; }
    protected bool m_Disabled = false;

    public abstract void Init(Character character);

    public abstract void UpdateControls();

    public abstract void SetFacingDirection(Vector3 direction);

    public abstract Vector3 GetControlRotation();

    public abstract Vector3 GetMoveInput();
    public abstract Vector3 GetRelativeMoveInput();

    public abstract Vector3 GetLookInput();

    public abstract Vector3 GetAimTarget();

    public abstract bool IsJumping();

    public abstract bool IsFiring();

    public abstract bool IsAiming();

    public abstract bool IsCrouching();

    public virtual void SetDisabled()
    {
        m_Disabled = true;
    }

    public virtual void SetEnabled()
    {
        m_Disabled = false;
    }

    public virtual void ToggleInput()
    {
        m_Disabled = !m_Disabled;
    }

    public virtual float GetRotationSpeed()
    {
        return 0.0f;
    }
}
