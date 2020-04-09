using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpBase : Hazard
{
    protected Animator PopupAnimator;
    protected int m_PopUpLayerLayerIndex;
    protected float m_PopAnimationWeight = 0.0f;

    protected bool CanPop = true;
    protected bool IsTriggered = false;

    protected Shootable PopupShootable = null;

    [Header("Audio"), SerializeField]
    ScriptableAudio SoundToPlayOnUp = null;
    [SerializeField]
    ScriptableAudio SoundToPlayOnDown = null;

    [HideInInspector] public AudioChannel m_Channel { get; protected set; }

    protected override void Awake()
    {
        base.Awake();

        PopupShootable = gameObject.GetComponentInChildren<Shootable>();
        m_Channel = GetComponent<AudioChannel>();
    }

    public virtual void Start()
    {
        PopupAnimator = GetComponentInParent<Animator>();
        m_PopUpLayerLayerIndex = PopupAnimator.GetLayerIndex("PopUpLayer");
    }

    // Update is called once per frame
    public virtual void Update()
    {
        //if(IsTriggered)
        //{
        //    PopUp();
        //}

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (PopupShootable.GetCanBeShot())
        {
            if (collision.transform.tag == "Projectile")
            {
                GoDown();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            if (CanPop)
            {
                PopUp();
                IsTriggered = true;
            }
        }
    }

    protected virtual void PopUp()
    {
        if (!IsTriggered)
        {
            m_Channel.PlayAudio(SoundToPlayOnUp);
            PopupShootable.SetCanBeShot(true);

            EventManager.TriggerEvent("OnPopupTriggeredEvent");
        }

        m_PopAnimationWeight = Mathf.Lerp(m_PopAnimationWeight, 1.0f, 15 * Time.deltaTime);
        PopupAnimator.SetLayerWeight(m_PopUpLayerLayerIndex, m_PopAnimationWeight);
        //close enough to 1 to say its popped up
        if (m_PopAnimationWeight >= 0.9f)
        {
            CanPop = false;
        }
    }

    protected virtual void GoDown()
    {
        //go back to idle 
        IsTriggered = false;
        m_PopAnimationWeight = Mathf.Lerp(m_PopAnimationWeight, 0.0f, 60 * Time.deltaTime);
        PopupAnimator.SetLayerWeight(m_PopUpLayerLayerIndex, m_PopAnimationWeight);
        m_Channel.PlayAudio(SoundToPlayOnDown);
        PopupShootable.SetCanBeShot(false);
    }


    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        m_PopAnimationWeight = 0.0f;
        PopupAnimator.SetLayerWeight(m_PopUpLayerLayerIndex, 0.0f);
        IsTriggered = false;
        CanPop = true;
    }
}
