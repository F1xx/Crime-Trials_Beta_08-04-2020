using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HudTimer : BaseObject
{
    public TMPro.TMP_Text TimerText = null;

    protected override void Awake()
    {
        base.Awake();

        WorldTimeManager.Instance();
        Listen("WorldTimerManagerReset", SetText);
    }

    void Update()
    {
        if (WorldTimeManager.IsRunning)
        {
            SetText();
        }
    }

    void SetText()
    {
        TimerText.text = WorldTimeManager.GetTimeAsFormattedString();
    }
}
