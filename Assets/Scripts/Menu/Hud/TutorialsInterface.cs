using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsInterface : BaseObject
{
    [Header("TutorialsInterface"), SerializeField]
    TutorialPopup PopupToShowOnTrigger = null;
    [SerializeField, Tooltip("If this is true then it will not trigger again after being triggered once.")]
    bool ShouldDisableAfterTrigger = true;
    [SerializeField, Tooltip("If the player hits 'R' and respawns and hits this trigger again then they will get the tutorial again.")]
    bool ShouldResetOnSoftReset = true;

    TutorialsHandler m_Handler = null;
    bool m_HasBeenTriggered = false;

    protected override void Awake()
    {
        base.Awake();

        m_Handler = GameObject.FindObjectOfType<TutorialsHandler>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Broadcast();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Broadcast();
        }
    }

    /// <summary>
    /// gets the HUD to show this thing's tutorial. If ShouldDisableAfterTrigger
    /// is true and it has been triggered it will not display again
    /// </summary>
    void Broadcast()
    {
        if(m_Handler == false || PopupToShowOnTrigger == null)
        {
            return;
        }

        if(ShouldDisableAfterTrigger && m_HasBeenTriggered)
        {
            return;
        }

        m_Handler.AddTutorial(PopupToShowOnTrigger);
        m_HasBeenTriggered = true;
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        if(ShouldResetOnSoftReset)
        {
            m_HasBeenTriggered = false;
        }
    }
}
