using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockPopup : BaseObject
{
    [SerializeField]
    Image UnlockImage = null;
    [SerializeField]
    TMPro.TMP_Text UnlockText = null;
    [SerializeField]
    float OpenDuration = 5.0f;

    Animator m_Animator = null;
    Timer ClosePanelTimer = null;

    bool m_IsAnimRunning = false;

    Queue<OnUnlockParam> m_UnlockQueue = new Queue<OnUnlockParam>();

    protected override void Awake()
    {
        base.Awake();

        m_Animator = GetComponent<Animator>();
        ClosePanelTimer = CreateTimer(OpenDuration, ClosePanel);
        ClosePanelTimer.UpdateUnscaled = true;
    }

    private void Start()
    {
        Listen("OnUnlockArm", AddPopupToQueue);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.O))
    //    {
    //        OnUnlockParam param = new OnUnlockParam(null, "You unlocked this by doing absolutely nothing. Its the default arm!", 0, "ARM");
    //        AddPopupToQueue(param);
    //    }

    //    if (Input.GetKeyDown(KeyCode.P))
    //    {
    //        OnUnlockParam param = new OnUnlockParam(null, "THIS IS A SECOND UNLOCK", 0, "ARM");
    //        AddPopupToQueue(param);
    //    }
    //}

    /// <summary>
    /// called by an animation event when the animation ends. Which means the panel is entirely visible
    /// so now we restart the timer and when it finishes we will close the panel via animation.
    /// </summary>
    public void FinishedAnimation()
    {
        ClosePanelTimer.Restart();
    }

    /// <summary>
    /// Called by the timer to start the closing animation
    /// </summary>
    void ClosePanel()
    {
        m_Animator.SetBool("Open", false);
    }

    /// <summary>
    /// this gets triggered at the start of the animation as well as the end since we reverse the animation.
    /// </summary>
    public void AnimationBeginOrEnd()
    {
        float direction = m_Animator.GetCurrentAnimatorStateInfo(0).speed;

        //we go backwards to close the anim so this means we just closed
        if (direction < 0) //closing
        {
            m_IsAnimRunning = false;

            //if the queue has more then continue with them
            if(m_UnlockQueue.Count > 0)
            {
                TriggerPopup();
            }
        }
    }

    /// <summary>
    /// sets the data of the popups and removes it from the queue
    /// </summary>
    void SetPopups()
    {
        if(m_UnlockQueue.Count == 0)
        {
            return;
        }

        UnlockImage.sprite = m_UnlockQueue.Peek().SpriteOfUnlock;
        UnlockText.text = m_UnlockQueue.Peek().UnlockText;

        //we got its data now we remove it
        m_UnlockQueue.Dequeue();
    }

    void AddPopupToQueue(EventParam param)
    {
        OnUnlockParam armParam = (OnUnlockParam)param;

        m_UnlockQueue.Enqueue(armParam);

        //only trigger the popup if no other popup is running
        if(m_IsAnimRunning == false)
        {
            TriggerPopup();
        }
    }

    void TriggerPopup()
    {
        SetPopups();
        m_Animator.SetBool("Open", true);
        m_IsAnimRunning = true;
    }
}