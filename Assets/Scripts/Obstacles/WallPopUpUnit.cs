using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPopUpUnit : PopUpBase
{
    [Tooltip("How much time shooting it down will reduce the players time")]
    public float TimeReductionAmount = 3.0f;

    bool CanReduceTime = true;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        if (IsTriggered)
        {
            PopUp();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Projectile")
        {
            GoDown();
            if (CanReduceTime)
            {
                WorldTimeManager.RemoveTime(TimeReductionAmount);
                CanReduceTime = false;
            }

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            if (CanPop)
            {
                IsTriggered = true;
                PopUp();
            }
        }
    }

    protected override void PopUp()
    {
        m_PopAnimationWeight = Mathf.Lerp(m_PopAnimationWeight, 1.0f, 15 * Time.deltaTime);
        PopupAnimator.SetLayerWeight(m_PopUpLayerLayerIndex, m_PopAnimationWeight);
        //close enough to 1 to say its popped up
        if (m_PopAnimationWeight >= 0.9f)
        {
            CanPop = false;
        }
    }

    protected override void GoDown()
    {
        //stop it from coming back up once shot
        IsTriggered = false;
        m_PopAnimationWeight = Mathf.Lerp(m_PopAnimationWeight, 0.0f, 60 * Time.deltaTime);
        PopupAnimator.SetLayerWeight(m_PopUpLayerLayerIndex, m_PopAnimationWeight);
    }

    protected override void OnSoftReset()
    {
        if (!CanReduceTime)
            return;
        base.OnSoftReset();
    }
}
