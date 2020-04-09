using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanHazard : Hazard
{
    [Header("Fan Data"), SerializeField]
    PainVolume KillArea = null;
    GameObject ObjectToSpin = null;
    [SerializeField]
    MultipleParticleSystemHandler ParticlesToSpawnOnBreak = null;
    [SerializeField, Tooltip("This particle needs to loop")]
    ParticleSystem SmokeParticle = null;
    PooledObject SmokeParticleObject = null;
    Shootable ShootHighlight = null;

    [Header("Fan Destruction"), SerializeField]
    GameObject FanWorking = null;
    [SerializeField]
    GameObject LocationToSpawnExplosion = null;
    [SerializeField]
    float TimeToWaitBeforeSwappingToBrokenMesh = 0.5f;
    [SerializeField]
    GameObject FanDamaged = null;
    [SerializeField]
    GameObject FanBroken = null;
    GameObject CurrentFanModel = null;

    [SerializeField]
    GameObject ObjectToSpinWorking = null;
    [SerializeField]
    GameObject ObjectToSpinDamaged = null;


    [Header("Fan Settings"), SerializeField, Tooltip("I highly recommend not changing it from 0,1,0 but if you wanna break it, have fun")]
    Vector3 Axis = new Vector3(0.0f, 1.0f, 0.0f);
    [SerializeField, Tooltip("This will rotate 360 degrees per second with a value of 1, 720 with 2, etc, recommend  slower")]
    float FullRotationsPerSecond = 1.0f; 
    [SerializeField]
    bool IsOn = true;
    [SerializeField]
    bool ShouldFanSmokeOnDeath = true;

    /// <summary>
    /// when damaged this multiplier will decrease causing the fan's max speed to decrease
    /// </summary>
    float DamagedMultiplier = 1.0f;
    float StartingSpeed = 3.0f;
    float StoppingSpeed = 2.0f;

    [Header("Audio"), SerializeField, Tooltip("Sound to disable or enable when the fan is acting.")]
    AudioChannel FanAudioChannel = null;
    [SerializeField]
    ScriptableAudio StartupSound = null;
    [SerializeField]
    ScriptableAudio RunningSound = null;
    [SerializeField]
    ScriptableAudio RunningDamagedSound = null;
    [SerializeField]
    ScriptableAudio StoppingSound = null;

    //Tweens and states
    eFanState m_CurrentState = eFanState.ERROR;
    float m_CurrentFanSpeedMultiplier = 0.0f;
    TweenableFloat m_TweenedFanSpeedMultiplier = null;
    TweenFloat m_CurrentFanSpeedMultiplierTween = null;

    //Defaults
    bool m_OriginalState = true;
    Quaternion m_StartRotation;
    FanHealthComponent Health = null;

    protected override void Awake()
    {
        base.Awake();

        Health = GetComponent<FanHealthComponent>();
        ShootHighlight = GetComponentInChildren<Shootable>();

        SetModelInfoBasedOnHealth();

        m_TweenedFanSpeedMultiplier = new TweenableFloat(m_CurrentFanSpeedMultiplier);
        m_StartRotation = ObjectToSpin.transform.rotation;
        m_OriginalState = IsOn;

        StartingSpeed = StartupSound.GetPlayableAudio().AudioClip.length;
        StoppingSpeed = StoppingSound.GetPlayableAudio().AudioClip.length;

        //Listen(FanDependantAudio.gameObject, "OnChannelAudioStopped" + FanDependantAudio.GetEventSignature(), CALLBACK)
        Listen(gameObject, "OnDeathEvent", OnDeath);
        Listen(gameObject, "OnDamageEvent", OnDamage);
    }

    private void Start()
    {
        ToggleState(IsOn);
    }

    void SetModelInfoBasedOnHealth()
    {
        GameObject newModel = null;

        FanDamaged.SetActive(false);
        FanWorking.SetActive(false);
        FanBroken.SetActive(false);


        if (Health.NeedsHealing() == false)
        {
            ObjectToSpin = ObjectToSpinWorking;
            DamagedMultiplier = 1.0f;
            StopSmokeParticle();
            newModel = FanWorking;
        }
        else if(Health.IsAlive())
        {
            ObjectToSpin = ObjectToSpinDamaged;
            DamagedMultiplier = 0.7f;
            PlaySmokeParticle();
            newModel = FanDamaged;

            if (m_CurrentState == eFanState.FanRunning)
            {
                RunFan();//force it to go through this again so that the new fan sound plays.
            }
        }
        else
        {
            if (ShouldFanSmokeOnDeath == false)
            {
                StopSmokeParticle();
            }
            newModel = FanBroken;
        }

        if(CurrentFanModel != null)
        {
            CurrentFanModel.SetActive(false);
        }
        CurrentFanModel = newModel;
        CurrentFanModel.SetActive(true);
    }

    private void Update()
    {
        if (Health.IsAlive())
        {
            SpinBasedOnState();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Axis.x = Mathf.Clamp(Axis.x, 0.0f, 1.0f);
        Axis.y = Mathf.Clamp(Axis.y, 0.0f, 1.0f);
        Axis.z = Mathf.Clamp(Axis.z, 0.0f, 1.0f);
    }
#endif

    void SpinBasedOnState()
    {
        switch (m_CurrentState)
        {
            case eFanState.ERROR:
                Debug.LogError("Fan: " + gameObject.name + " State failed to be set");
                break;
            case eFanState.FanStarting:
            case eFanState.FanStopping:
            case eFanState.FanRunning:
                RotateFan();
                break;
            case eFanState.FanStopped:
                break;
        }
    }

    void RotateFan()
    {
        //updates the current tween
        if(m_CurrentFanSpeedMultiplierTween != null)
        {
            m_CurrentFanSpeedMultiplier = m_TweenedFanSpeedMultiplier.Value;
        }

        float amountToRotate = (360.0f * FullRotationsPerSecond) * m_CurrentFanSpeedMultiplier * DamagedMultiplier;

        amountToRotate *= Time.deltaTime;
        ObjectToSpin.transform.Rotate(Axis, amountToRotate, Space.Self);
    }

    void SetFanToOffPosition()
    {
        ObjectToSpin.transform.rotation = m_StartRotation;
    }

    void OnDeath(EventParam param)
    {
        //leave if we can't be shot anyway
        if (ShootHighlight.GetCanBeShot() == false)
        {
            return;
        }

        SpawnDamageParticle();
        ChangeState(eFanState.FanStopped);

        StartCoroutine(PauseForExplosion());
    }

    IEnumerator PauseForExplosion()
    {
        yield return new WaitForSeconds(TimeToWaitBeforeSwappingToBrokenMesh);
        SetModelInfoBasedOnHealth();
    }

    void OnDamage(EventParam param)
    {
        OnDamageEventParam dp = (OnDamageEventParam)param;

        //if we took damage and are not dead
        if(dp.CurrentHealth < dp.MaxHealth && dp.CurrentHealth > dp.MinHealth)
        {
            SetModelInfoBasedOnHealth();
        }
    }

    void PlaySmokeParticle()
    {
        SmokeParticle.Play();

        //get an instance of the particle from the pool
        SmokeParticleObject = ObjectPoolManager.Get(SmokeParticle.gameObject);
        SmokeParticleObject.gameObject.transform.position = ObjectToSpin.transform.position;

        ParticleSystem ps = SmokeParticleObject.gameObject.GetComponent<ParticleSystem>();
        //start playing
        ps.Play();
    }

    void StopSmokeParticle()
    {
        if(SmokeParticleObject != null)
        {
            ParticleSystem ps = SmokeParticleObject.gameObject.GetComponent<ParticleSystem>();
            ps.Stop();
            SmokeParticleObject.Deactivate();
        }
    }

    void SpawnDamageParticle()
    {
        ParticlesToSpawnOnBreak.Play(LocationToSpawnExplosion.transform.position);
    }

    #region State Handling

    /// <summary>
    /// Handles the changing of the Fan's state
    /// </summary>
    /// <param name="state">the state to change to</param>
    void ChangeState(eFanState state)
    {
        if (state == m_CurrentState)
        {
            return;
        }

        m_CurrentState = state;

        switch (m_CurrentState)
        {
            case eFanState.ERROR:
                Debug.LogError("Fan: " + gameObject.name + " State set to error");
                break;
            case eFanState.FanStarting:
                StartFan();
                break;
            case eFanState.FanStopping:
                StopFan();
                break;
            case eFanState.FanRunning:
                RunFan();
                break;
            case eFanState.FanStopped:
                EndFan();
                break;
        }
    }

    /// <summary>
    /// starts up the fan. From a stop it will have to take a small amount of time to get to full speed
    /// </summary>
    void StartFan()
    {
        //handle audio, kill box, start tween
        KillArea.TurnOnOff(true);

        float percent = m_CurrentFanSpeedMultiplier;
        float dur = StartingSpeed - (StartingSpeed * m_CurrentFanSpeedMultiplier);
        HandleTweenCreation(1.0f, dur, eFanState.FanRunning);

        ShootHighlight.SetCanBeShot(true);
        ChangeAudio(StartupSound, percent);
    }

    /// <summary>
    /// initiates the fan stopping. If it was running it will have to slow down before finishing
    /// </summary>
    void StopFan()
    {
        //handle audio, stop tween
        float percent = 1 - m_CurrentFanSpeedMultiplier;
        float dur = StoppingSpeed - (StoppingSpeed * (1 - m_CurrentFanSpeedMultiplier));
        HandleTweenCreation(0.0f, dur, eFanState.FanStopped);

        ChangeAudio(StoppingSound, percent);
    }

    /// <summary>
    /// Changes to the state where the fan is running at full speed
    /// </summary>
    void RunFan()
    {
        if(Health.NeedsHealing())
        {
            ChangeAudio(RunningDamagedSound, 0.0f);
        }
        else
        {
            ChangeAudio(RunningSound, 0.0f);
        }

        m_CurrentFanSpeedMultiplier = 1.0f;
        //handle audio
    }

    /// <summary>
    /// Called to entirely stop the fan, it is no longer moving
    /// </summary>
    void EndFan()
    {
        m_CurrentFanSpeedMultiplier = 0.0f;
        ShootHighlight.SetCanBeShot(false);
        //handle audio and kill box, position
        KillArea.TurnOnOff(false);
        SetFanToOffPosition();

        StopAllAudio();
    }

    /// <summary>
    /// sets the fan to the opposite state it is in. If its running or starting it will enter stopping, and vice versa entering starting
    /// </summary>
    public void ToggleState()
    {
        switch (m_CurrentState)
        {
            case eFanState.FanStarting:
            case eFanState.FanRunning:
                ChangeState(eFanState.FanStopping);
                break;

            case eFanState.FanStopping:
            case eFanState.FanStopped:
                ChangeState(eFanState.FanStarting);
                break;
        }
    }

    /// <summary>
    /// Changes the fan's state
    /// </summary>
    /// <param name="ison">Whether to turn it on or off</param>
    /// <param name="force">Whether the function should force the state change even if already in the state</param>
    public void ToggleState(bool ison, bool force = false)
    {
        //turn it on if it isn't already on and they want it on
        if ((ison && m_CurrentState != eFanState.FanStarting && m_CurrentState != eFanState.FanRunning) || force)
        {
            ChangeState(eFanState.FanStarting);
        }
        //turn it off if it isn't already
        else if ((ison == false && m_CurrentState != eFanState.FanStopped && m_CurrentState != eFanState.FanStopping) || force)
        {
            ChangeState(eFanState.FanStopping);
        }
    }

    enum eFanState
    {
        ERROR,
        FanStarting,
        FanStopping,
        FanRunning,
        FanStopped
    }
    #endregion

    #region Tween handling
    void HandleTweenCreation(float target, float duration, eFanState stateIfCloseEnough)
    {
        //if we're already close enough to running just start running instead
        if (MathUtils.FloatCloseEnough(m_CurrentFanSpeedMultiplier, target, 0.1f))
        {
            m_CurrentFanSpeedMultiplier = target;
            ChangeState(stateIfCloseEnough);
            return;
        }
        EndTweenIfItExists();

        //start the tween from where we are in the rotation
        m_TweenedFanSpeedMultiplier.Value = m_CurrentFanSpeedMultiplier;
        m_CurrentFanSpeedMultiplierTween = (TweenFloat)TweenManager.CreateTween(m_TweenedFanSpeedMultiplier, target, duration, eTweenFunc.LinearToTarget, TweenEnded);
    }

    void EndTweenIfItExists()
    {
        if (m_CurrentFanSpeedMultiplierTween != null)
        {
            m_CurrentFanSpeedMultiplierTween.StopTweening(Tween.eExitMode.IncompleteTweening, false);
        }
    }

    void TweenEnded()
    {
        //since we're done with that tween, remove it entirely.
        m_CurrentFanSpeedMultiplierTween = null;

        switch (m_CurrentState)
        {
            case eFanState.FanStarting:
                ChangeState(eFanState.FanRunning);
                break;
            case eFanState.FanStopping:
                ChangeState(eFanState.FanStopped);
                break;
        }
    }
    #endregion

    #region Audio Handling
    void StopAllAudio()
    {
        if (FanAudioChannel.IsPlaying())
        {
            FanAudioChannel.StopAudio();
        }
    }

    void ChangeAudio(ScriptableAudio newAudio, float percentToStartFrom)
    {
        FanAudioChannel.OwnedAudioObject = newAudio;
        FanAudioChannel.PlayAudio();
        FanAudioChannel.SetPlayTimeByPercentage(percentToStartFrom);
    }
    #endregion

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        ObjectToSpin.transform.rotation = m_StartRotation;
        m_CurrentFanSpeedMultiplier = 0.0f;
        ToggleState(m_OriginalState, true);
        ShootHighlight.SetCanBeShot(true);

        SetModelInfoBasedOnHealth();
    }
}