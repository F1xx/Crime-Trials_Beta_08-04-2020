using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanSwitch : PhysicalSwitch
{
    [SerializeField]
    List<FanHazard> FansToInteractWith = new List<FanHazard>();
    [SerializeField]
    bool SetFansToMatchButton = false;

    protected override void Awake()
    {
        IgnoretargetNull = true;
        base.Awake();

        if(SetFansToMatchButton)
        {
            SetFans(m_IsActive);
        }
    }

    void SetFans(bool value)
    {
        foreach(FanHazard fan in FansToInteractWith)
        {
            fan.ToggleState(value);
        }
    }

    void ToggleFans()
    {
        foreach (FanHazard fan in FansToInteractWith)
        {
            fan.ToggleState();
        }
    }

    public override bool ToggleState()
    {
        if (base.ToggleState())
        {
            if (SetFansToMatchButton)
            {
                SetFans(m_IsActive);
            }
            else
            {
                ToggleFans();
            }

            return true;
        }

        return false;
    }

    protected override void Activate()
    {
        m_IsActive = true;
        EventManager.TriggerEvent(gameObject, "SwitchActivated");

        CanActivate();
    }

    protected override void Deactivate()
    {
        m_IsActive = false;
        EventManager.TriggerEvent(gameObject, "SwitchDeactivated");

        CanActivate();
    }

    protected override void OnSoftReset()
    {
        MeshToChange.GetComponent<Renderer>().material = ActiveMat;

        HasBeenActivated = false;

        if (m_IsActive == InitialActiveState)
        {
            return;
        }

        m_IsActive = InitialActiveState;

        if (SetFansToMatchButton)
        {
            SetFans(m_IsActive);
        }
    }
}
