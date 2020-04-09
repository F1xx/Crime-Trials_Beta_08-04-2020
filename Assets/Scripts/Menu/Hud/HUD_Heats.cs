using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_Heats : BaseObject
{
    [SerializeField]
    TMPro.TMP_Text HeatsText = null;

    [SerializeField]
    Color AheadColor = Color.green;
    [SerializeField]
    Color BehindColor = Color.red;


    [SerializeField]
    float TimeToDisplayHeat = 2.0f;
    [SerializeField]
    float FadeInTime = 0.8f;
    [SerializeField]
    float FadeOutTime = 1.5f;

    Timer DisplayTimer = null;
    Timer FadeInTimer = null;
    Timer FadeOutTimer = null;

    bool m_IsRunning = false;

    protected override void Awake()
    {
        base.Awake();

        DisplayTimer = CreateTimer(TimeToDisplayHeat, OnDisplayFinish);
        FadeInTimer = CreateTimer(FadeInTime, OnFadeInFinish);
        FadeOutTimer = CreateTimer(FadeOutTime, OnFadeOutFinish);

        Reset();

        //Listen to Checkpoints so we show heats
        Listen("OnCheckPointReached", OnCheckPointReached);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Comma))
        //{
        //    OnCheckpointReachedParam param = new OnCheckpointReachedParam(0, WorldTimeManager.TimePassed + 5.0236f);

        //    OnCheckPointReached(param);
        //}

        //if (Input.GetKeyDown(KeyCode.Period))
        //{
        //    OnCheckpointReachedParam param = new OnCheckpointReachedParam(0, WorldTimeManager.TimePassed - 5.0236f);

        //    OnCheckPointReached(param);
        //}

        if (m_IsRunning == false)
        {
            return;
        }

        if(FadeInTimer.IsRunning && FadeInTimer.GetPercentageComplete() != 1.0f)
        {
            HeatsText.alpha = FadeInTimer.GetPercentageComplete();
        }
        else if(FadeOutTimer.IsRunning)
        {
            HeatsText.alpha = 1.0f - FadeOutTimer.GetPercentageComplete();
        }
    }

    void OnCheckPointReached(EventParam param)
    {
        Reset();

        OnCheckpointReachedParam Param = (OnCheckpointReachedParam)param;
        float heatTime = SQLManager.GetRunCheckpointDataFor(Param.CheckpointIndex); //WorldTimeManager.TimePassed - 1.0f;

        if (heatTime < 0.0f)
        {
            return;
        }

        float difference = Param.TimeReached - heatTime;
        SetTextColor(difference);
        HeatsText.alpha = 0.0f;
        SetHeatsText(difference);

        m_IsRunning = true;
        FadeInTimer.Restart();
    }

    void SetHeatsText(float time)
    {
        float absTime = Mathf.Abs(time);

        int intTime = (int)absTime;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        float milliseconds = absTime * 1000;
        milliseconds = (milliseconds % 1000);

        if (time > 0.0f)
        {
            HeatsText.text = string.Format("+{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
        else
        {
            HeatsText.text = string.Format("-{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
    }

    void SetTextColor(float difference)
    {
        if(difference < 0.0f)
        {
            HeatsText.color = AheadColor;
        }
        else if(difference > 0.0f)
        {
            HeatsText.color = BehindColor;
        }
        else
        {
            HeatsText.color = Color.white;
        }
    }

    void OnFadeInFinish()
    {
        HeatsText.alpha = 1.0f;
        DisplayTimer.Restart();
    }

    void OnDisplayFinish()
    {
        FadeOutTimer.Restart();
    }

    void OnFadeOutFinish()
    {
        Reset();
    }

    void Reset()
    {
        HeatsText.color = Color.white;
        HeatsText.alpha = 0.0f;

        DisplayTimer.Reset();
        FadeInTimer.Reset();
        FadeOutTimer.Reset();

        m_IsRunning = false;
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        Reset();
    }
}
