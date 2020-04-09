using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFade : BaseObject
{
    public static bool IsActive { get; private set; }

    public float TimeUntilBlackOut = 1.0f;
    float CurrentAlpha = 0.0f;

    public CanvasGroup BlackScreen = null;
    public CanvasGroup ResetText = null;

    protected override void Awake()
    {
        base.Awake();
        Listen("OnPlayerDeathEvent", OnPlayerDeathEvent);
        OnSoftReset();
        IsActive = false;
    }

    void Update()
    {
        if (IsActive)
        {
            CurrentAlpha += Time.deltaTime * TimeUntilBlackOut;
            CurrentAlpha = Mathf.Clamp(CurrentAlpha, 0.0f, 1.0f);
            BlackScreen.alpha = CurrentAlpha;

            if (BlackScreen.alpha >= 1.0f)
            {
                ResetText.alpha = 1.0f;
            }
        }
    }

    protected override void OnSoftReset()
    {
        BlackScreen.alpha = 0.0f;
        ResetText.alpha = 0.0f;
        CurrentAlpha = 0.0f;
        IsActive = false;
    }

    void OnPlayerDeathEvent(EventParam param)
    {
        IsActive = true;
        WorldTimeManager.StopTimer();
    }
}
