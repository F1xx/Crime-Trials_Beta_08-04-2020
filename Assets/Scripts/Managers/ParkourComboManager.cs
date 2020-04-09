using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourComboManager : Singleton<ParkourComboManager>
{
    [Header("Base Settings")]
    [Range(0,1), SerializeField, Tooltip("There are multiple versions of this system built. This switches between which is active. 0 is cooldown based, 1 is constant combo decrement.")]
    private int _ParkourVersion = 0;
    public static int ParkourVersion { get { return Instance()._ParkourVersion; } }

    [Space(4)]
    public float MaxCombo = 10.0f;
    public bool UseComboSystem = true;

    public static float ComboMult { get { return Instance()._ComboMult; } private set { Instance().SetComboMult(value); } }
    [SerializeField]
    private float _ComboMult = 0.0f;

    //Timers
    private Timer m_ComboTimer = null;
    public Timer m_DoubleJumpCooldown = null;
    public Timer m_ShootCooldown = null;
    public Timer m_SlideCooldown = null;
    public Timer m_WallrunCooldown = null;
    private Timer m_StartDecrement = null;

    [Header("Cooldowns"), SerializeField]
    float ComboTimerInitialLength = 5.0f;
    [SerializeField]
    float ComboTimerMinimumLength = 1.0f;
    float DoubleJumpCooldown = 6.0f;
    float ShootCooldown = 3.0f;
    float SlideCooldown = 6.0f;
    float WallrunCooldown = 1.0f;

    [Header("Values"), SerializeField]
    float DoubleJumpValue = 1.0f;
    [SerializeField]
    float ShootpValue = 1.0f;
    [SerializeField]
    float SlideValue = 1.0f;
    [SerializeField]
    float WallrunValue = 1.0f;
    float MinPossibleValue = float.MaxValue;

    [Header("Values - Version 1"), SerializeField]
    int TimesDoubleJumped = 0;
    [SerializeField]
    int TimesSlided = 0;
    [SerializeField]
    int TimesWallran = 0;
    [SerializeField]
    int TimesShot = 0;

    [Header("Decrements - Version 1"), Range(0.0f, 0.9f), SerializeField, Tooltip("These are percentages of the actual value (so if slide's value is 10 then 0.1 decrement will dock 10%, ie 1")]
    float DoubleJumpDecrement = 0.1f;
    [Range(0.0f, 0.9f), SerializeField]
    float SlideDecrement = 0.1f;
    [Range(0.0f, 0.9f), SerializeField]
    float ShootDecrement = 0.1f;
    [Range(0.0f, 0.9f), SerializeField]
    float WallrunDecrement = 0.1f;
    bool m_StartDecreasing = false;


    protected override void OnAwake()
    {
        Listen("OnPlayerDoubleJump", OnPlayerDoubleJump);
        Listen("OnPlayerSlide", OnPlayerSlide);
        Listen("OnShootableShot", OnShootableShot);
        Listen("OnStartWallRunning", OnPlayerStartWallrun);
        Listen("OnLeaveWallRunning", OnPlayerStopWallrun);

        m_ComboTimer = CreateTimer(ComboTimerInitialLength, OnComboTimerEnd);

        m_DoubleJumpCooldown = CreateTimer(DoubleJumpCooldown, OnDoubleJumpTimerEnd);
        m_ShootCooldown = CreateTimer(ShootCooldown, OnShootTimerEnd);
        m_SlideCooldown = CreateTimer(SlideCooldown, OnSlideTimerEnd);
        m_WallrunCooldown = CreateTimer(WallrunCooldown, OnWallrunTimerEnd);
        m_StartDecrement = CreateTimer(1.0f, OnDecrementEnd);

        DeterminMinValue();
    }

    void DeterminMinValue()
    {
        MinPossibleValue = float.MaxValue;

        if(MinPossibleValue > DoubleJumpValue)
        {
            MinPossibleValue = DoubleJumpValue;
        }
        if(MinPossibleValue > ShootpValue)
        {
            MinPossibleValue = ShootpValue;
        }
        if(MinPossibleValue > SlideValue)
        {
            MinPossibleValue = SlideValue;
        }
        if(MinPossibleValue > WallrunValue)
        {
            MinPossibleValue = WallrunValue;
        }
    }

//ONVALIDATE
    private void Update()
    {
        if(ParkourVersion == 1 && ComboMult > 0.0f && m_StartDecreasing)
        {
            DecreseComboOverTime();
        }

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.F11))
        {
            AddComboScore(1.0f, null);
        }
#endif
    }

    public static float GetComboTimerPercentComplete()
    {
        return Instance().m_ComboTimer.GetPercentageComplete();
    }

    void DecreseComboOverTime()
    {
        ComboMult = ComboMult - (0.1f * Time.deltaTime);
    }

    /// <summary>
    /// The function used to set and clamp the value of the combo multiplier
    /// </summary>
    /// <param name="value">the value the combo will become</param>
    void SetComboMult(float value)
    {
        if (UseComboSystem == false)
        {
            return;
        }

        _ComboMult = Mathf.Clamp(value, 0, MaxCombo);
        EventManager.TriggerEvent("OnComboChange");
    }

    void OnPlayerDoubleJump()
    {
        if (ParkourVersion == 0 && m_DoubleJumpCooldown.IsRunning == false)
        {
            AddComboScore(DoubleJumpValue, m_DoubleJumpCooldown);
            EventManager.TriggerEvent("OnDoubleJumpCombo");
        }
        else if(ParkourVersion == 1)
        {
            EventManager.TriggerEvent("OnDoubleJumpCombo");
            HandleDecrement(ref TimesDoubleJumped, DoubleJumpDecrement, DoubleJumpValue, m_DoubleJumpCooldown);
        }
    }

    void OnPlayerSlide()
    {
        if (ParkourVersion == 0 && m_SlideCooldown.IsRunning == false)
        {
            EventManager.TriggerEvent("OnSlideCombo");
            AddComboScore(SlideValue, m_SlideCooldown);
        }
        else if (ParkourVersion == 1)
        {
            EventManager.TriggerEvent("OnSlideCombo");
            HandleDecrement(ref TimesSlided, SlideDecrement, SlideValue, m_SlideCooldown);
        }
    }

    void OnShootableShot()
    {
        if (ParkourVersion == 0 && m_ShootCooldown.IsRunning == false)
        {
            EventManager.TriggerEvent("OnShootCombo");
            AddComboScore(ShootpValue, m_ShootCooldown);
        }
        else if (ParkourVersion == 1)
        {
            EventManager.TriggerEvent("OnShootCombo");
            HandleDecrement(ref TimesShot, ShootDecrement, ShootpValue, m_ShootCooldown);
        }
    }

    void OnPlayerStartWallrun()
    {
        if (ParkourVersion == 0 && m_WallrunCooldown.IsRunning == false)
        {
            EventManager.TriggerEvent("OnWallRunCombo");
            m_WallrunCooldown.Reset();
            AddComboScore(WallrunValue, null);
        }
        else if (ParkourVersion == 1)
        {
            EventManager.TriggerEvent("OnWallRunCombo");
            HandleDecrement(ref TimesWallran, WallrunDecrement, WallrunValue, m_WallrunCooldown);
        }
    }

    /// <summary>
    /// Wallrunning cooldown only starts when you leave the wall
    /// </summary>
    void OnPlayerStopWallrun()
    {
        if (ParkourVersion == 0)// && m_WallrunCooldown.IsRunning == false)
        {
            m_WallrunCooldown.Restart();
        }
    }

    void HandleDecrement(ref int counter, float decrement, float baseValue, Timer cooldown)
    {
        float dec = 1 * ((float)counter * decrement);

        dec = Mathf.Clamp(dec, 0.0f, 0.9f);

        counter++;
        AddComboScore(baseValue - dec, cooldown);
    }

    /// <summary>
    /// this will add the given score and then restart the combo timer
    /// </summary>
    /// <param name="val">amount to add to combo</param>
    void AddComboScore(float val, Timer cooldown)
    {
        if(ComboMult == 0.0f)
        {
            m_StartDecrement.Restart();
        }

        ComboMult += val;
        RestartComboTimer();

        if (cooldown != null)
        {
            cooldown.Restart();
        }
    }

    /// <summary>
    /// if the player's combo ends it will be halved and rounded to the nearest int
    /// </summary>
    void OnComboTimerEnd()
    {
        float combo = ComboMult;

        //halve the multiplier
        combo *= 0.5f;

        if(combo < 1.0f)
        {
            ResetValues();
            combo = 0.0f;
        }
        else
        {
            combo = Mathf.RoundToInt(combo);

            if (ParkourVersion == 1)
            {
                HalveCounts();
            }
        }

        ComboMult = combo;

        RestartComboTimer();
    }

    /// <summary>
    /// will not restart if combo is at 0
    /// ie Add combo before restarting timer.
    /// </summary>
    void RestartComboTimer()
    {
        if(ComboMult == 0.0f)
        {
            return;
        }

        float duration = 1 + ComboTimerInitialLength - (((ComboMult - MinPossibleValue) / (MaxCombo - MinPossibleValue)) * (ComboTimerInitialLength - ComboTimerMinimumLength));

        //duration = Mathf.Clamp(duration, ComboTimerMinimumLength, ComboTimerInitialLength);
       
        //set a duration based on your combo mult
        m_ComboTimer.SetDuration(duration);
        m_ComboTimer.Restart();
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        ResetValues();
    }

    void HalveCounts()
    {
        TimesDoubleJumped = TimesDoubleJumped / 2;
        TimesSlided = TimesSlided / 2;
        TimesWallran = TimesWallran / 2;
        TimesShot = TimesShot / 2;
    }

    void ResetValues()
    {
        ComboMult = 0;

        m_ComboTimer.SetDuration(ComboTimerInitialLength);
        m_ComboTimer.Reset();

        m_DoubleJumpCooldown.Reset();
        m_ShootCooldown.Reset();
        m_SlideCooldown.Reset();
        m_WallrunCooldown.Reset();

        TimesDoubleJumped = 0;
        TimesSlided = 0;
        TimesWallran = 0;
        TimesShot = 0;

        m_StartDecreasing = false;
    }

    //Timer Resets
    void OnDoubleJumpTimerEnd()
    {
        if(ParkourVersion == 1)
        {
            TimesDoubleJumped = Mathf.Clamp(TimesDoubleJumped--, 0, 10);
        }
    }

    void OnSlideTimerEnd()
    {
        if (ParkourVersion == 1)
        {
            TimesSlided = Mathf.Clamp(TimesSlided--, 0, 10);
        }
    }

    void OnWallrunTimerEnd()
    {
        if (ParkourVersion == 1)
        {
            TimesWallran = Mathf.Clamp(TimesWallran--, 0, 10);
        }
    }

    void OnShootTimerEnd()
    {
        if (ParkourVersion == 1)
        {
            TimesShot = Mathf.Clamp(TimesShot--, 0, 10);
        }
    }

    void OnDecrementEnd()
    {
        m_StartDecreasing = true;
    }
}
