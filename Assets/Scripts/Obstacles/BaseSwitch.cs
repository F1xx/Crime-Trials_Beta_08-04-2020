using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSwitch : BaseObject
{
    [SerializeField, Tooltip("Active means the target will be Enabled by default. If you want something disabled by default make this false.")]
    protected bool m_IsActive = true;

    protected bool InitialActiveState;

    [SerializeField, Tooltip("If true this button toggle once and then no longer receive input")]
    protected bool ShouldBreakAfterActivation = true;

    protected bool HasBeenActivated = false;

    [SerializeField, Tooltip("The object to enable/disable")]
    protected GameObject Target = null;

    [Tooltip("State can be Toggled by hooking the switch to an event. List that event name here.")]
    public string EventToListenTo = "";

    protected Shootable CanBeShot = null;
    protected bool IgnoretargetNull = false;

    protected override void Awake()
    {
        base.Awake();
        InitialActiveState = m_IsActive;

        if (Target == null && IgnoretargetNull == false)
        {
            Debug.LogError("Switch requires a target to run");
        }

        if(m_IsActive == false)
        {
            Deactivate();
        }

        if (string.IsNullOrEmpty(EventToListenTo) == false)
        {
            Listen(EventToListenTo, EventToggleState);
        }

        CanBeShot = gameObject.GetComponentInChildren<Shootable>();

        if(CanBeShot == null)
        {
            Debug.LogError("Switch: " + gameObject.transform.root.name + " Cannot Find Shootable");
        }
    }

    public void BreakSwitch()
    {

    }

    public virtual bool CanActivate()
    {
        if(ShouldBreakAfterActivation == true)
        {
            if(HasBeenActivated == true)
            {
                CanBeShot.SetCanBeShot(false);
                return false;
            }
        }
        CanBeShot.SetCanBeShot(true);
        return true;
    }

    public void EventToggleState()
    {
        ToggleState();
    }

    //returns if it actually toggled
    public virtual bool ToggleState()
    {
        if (CanActivate())
        {
            if(ShouldBreakAfterActivation)
                HasBeenActivated = true;

            if (m_IsActive)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
            EventManager.TriggerEvent(gameObject, "SwitchToggled");
            return true;
        }
        return false;
    }

    protected virtual void Activate()
    {
        m_IsActive = true;

        if(Target)
        {
            Target.SetActive(true);
            EventManager.TriggerEvent(gameObject, "SwitchActivated");
        }

        CanActivate();
    }

    protected virtual void Deactivate()
    {
        m_IsActive = false;

        if (Target)
        {
            Target.SetActive(false);
            EventManager.TriggerEvent(gameObject, "SwitchDeactivated");
        }

        CanActivate();
    }


    protected override void OnSoftReset()
    {
        HasBeenActivated = false;

        if (m_IsActive == InitialActiveState)
        {
            return;
        }

        m_IsActive = InitialActiveState;

        if(Target)
        {
            Target.SetActive(InitialActiveState);
        }
    }
}
