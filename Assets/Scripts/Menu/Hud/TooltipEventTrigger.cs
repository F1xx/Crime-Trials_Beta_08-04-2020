using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TooltipEventTrigger : BaseObject, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Custom Event Trigger Settings"), SerializeField]
    string TextOnEnter = "";
    [SerializeField]
    bool ClearTextOnExit = true;
    [SerializeField]
    TMPro.TMP_Text ToolTipField = null;

    [SerializeField]
    float TimeUntilTooltipShows = 0.6f;
    Timer ShowToolTipTimer = null;

    bool IsThisToolTipActive = false;

    [SerializeField, Tooltip("FadeIn will NOT work during a pause menu. They just will not show up due to time being stopped.")]
    bool UseFadeIn = false;

    protected override void Awake()
    {
        base.Awake();

        ShowToolTipTimer = CreateTimer(TimeUntilTooltipShows, OnTimerFinish);
    }

    private void Update()
    {
        if (ToolTipField != null)
        {
            //Have it fade in quickly but it adds some niceness to it
            if (IsThisToolTipActive && ToolTipField.alpha != 1.0f)
            {
                float tempalpha = ToolTipField.alpha;

                tempalpha = Mathf.Clamp(tempalpha + Time.deltaTime, 0.0f, 1.0f);

                ToolTipField.alpha = tempalpha;
            }

            //if (IsThisToolTipActive)
            //{
            //    Vector3 offset = new Vector3(70.0f, 30.0f, 0.0f);
            //    ToolTipField.gameObject.transform.position = gameObject.transform.position + offset;
            //}
        }
    }

    void OnTimerFinish()
    {
        SetText();
    }

    #region Events Region
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (UseFadeIn)
        {
            ShowToolTipTimer.Restart();
        }
        else
        {
            SetText();
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (UseFadeIn)
        {
            ShowToolTipTimer.Restart();
        }
        else
        {
            SetText();
        }

        EventManager.TriggerEvent("OnMenuHover");
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //if (ClearTextOnExit)
        //{
        //    ClearText();
        //}
    }

    private void OnDisable()
    {
        OnDeselect(null);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (ClearTextOnExit)
        {
            ClearText();
        }
    }

    void SetText()
    {
        if (ToolTipField == null)
        {
            return;
        }

        ToolTipField.text = TextOnEnter;
        IsThisToolTipActive = true;

        if (UseFadeIn)
        {
            ToolTipField.alpha = 0.0f;
        }
    }

    void ClearText()
    {
        if (ToolTipField == null)
        {
            return;
        }

        if(IsThisToolTipActive == false && ShowToolTipTimer.IsRunning)
        {
            ShowToolTipTimer.Reset();
            return;
        }

        ToolTipField.text = "";
        IsThisToolTipActive = false;
    }
    #endregion
}
