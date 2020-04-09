using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The movement component for the player.
/// Directly interacts with the PlayerInputComponent, PlayerStateMachine, and PlayerController to direct movements.
/// </summary>
[RequireComponent(typeof(PlayerInputComponent))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerController))]
public class PlayerMovementComponent : BaseObject
{
    PlayerInputComponent m_InputComponent = null;
    PlayerStateMachine PSM = null;
    PlayerController m_Controller = null;
    private AudioChannel m_SoundChannel = null;
    DebuffSystem m_DebuffSystem = null;

    [Header("Animations")]
    //animator shenanigans///////////////////
    Animator m_PlayerAnimator = null;
    int m_IdleLayerIndex = 0;
    int m_CrouchingLayerIndex = 0;
    int m_UncrouchLayerIndex = 0;
    int m_SlidingLayerIndex = 0;
    int m_CrouchWalkingLayerIndex = 0;
    int m_FallingLayerIndex = 0;
    int m_RunningLayerIndex = 0;
    int m_WallRunningRightLayerIndex = 0;
    int m_WallRunningLeftLayerIndex = 0;
    int m_StopRunningLayerIndex = 0;
    int m_Slide2CrouchLayerIndex = 0;
    int m_Slide2RunLayerIndex = 0;
    int m_InspectLayerIndex = 0;
    float m_IdleLayerWeight = 0.0f;
    float m_CrouchingLayerWeight = 0.0f;
    float m_UncrouchLayerWeight = 0.0f;
    float m_SlidingLayerWeight = 0.0f;
    float m_CrouchWalkingLayerWeight = 0.0f;
    float m_FallingLayerWeight = 0.0f;
    float m_RunningLayerWeight = 0.0f;
    float m_WallRunningRightLayerWeight = 0.0f;
    float m_WallRunningLeftLayerWeight = 0.0f;
    float m_StopRunningLayerWeight = 0.0f;
    float m_Slide2CrouchLayerWeight = 0.0f;
    float m_Slide2RunLayerWeight = 0.0f;
    float m_InspectLayerWeight = 0.0f;


    public float TimeBetweenSpecialIdleAnimations = 5.0f;
    [HideInInspector]
    public Timer TimeUntilSpecialIdleAnimation;
    private int SpecialIdleType = 0;


    [Header("Ground")]
    public float Speed = 10.0f;
    [SerializeField]
    private float m_CurrentSpeed = 10.0f;
    [SerializeField]
    private float MinAngleToSlowSpeed = 25.0f;
    private Vector3 m_Slopedir = Vector3.zero;
    private bool DidPlayerStop = false;

    [Header("Sprint")]
    public float SprintMult = 1.8f;
    public float m_CurrentSprintMult = 1.0f;
    [SerializeField]
    private float TimeTillMaxSprintSpeed = 2.0f;


    [Header("Air")]
    public float InAirMagnitudeCapMult = 1.2f;
    [Tooltip("Effectually calculated as (Airspeed / Speed) * CurrentSpeed")]
    public float AirSpeed = 1.0f;

    [Header("Wall-Run")]
    public float DistanceFromWallRequiredToMount = 2.0f;
    public float MaxSpeedRequiredToWallRunSquared = 20.0f;
    public string TagToCompareForWallRunning = "WallRunnable";
    public float WallRunningCooldownOnSamelWall = 1.5f;

    //Gravity scale variables that activate whenever you enter wallrunning.
    public float GravityScaleTweenDuration = 700.0f;
    private TweenableFloat CurrentGravityScale = new TweenableFloat(0.0f);
    private Tween GravityScaleTween;
    private float ControllerGravityScaleAccessor { get { return m_Controller.GravityScale; } set { m_Controller.GravityScale = value; } }

    //Step up variables that activate whenever you enter wallrunning.
    public Vector3 StepOntoWallVector = new Vector3(0.0f, 2.0f, 0.0f);
    public float StepOntoWallTweenDuration = 2.0f;
    private TweenableVector3 CurrentStepUpVector = new TweenableVector3(new Vector3(0.0f, 0.0f, 0.0f));
    private Tween StepUpTween;

    //Input command vector
    public Vector3 LeftAndRightMovement = new Vector3(0.0f, 2.50f, 0.0f);

    [Header("Crouch/Slide"),SerializeField]
    private float DefaultCrouchMovementSpeedMultiplayer = 0.5f;
    [SerializeField]
    private float DefaultSlideMovementSpeedMultiplier = 3.5f;
    [SerializeField]
    private float TimeToDecaySlideMovementSpeed = 3.0f;
    [Tooltip("How frequently the slide move can be used.")]
    public float SlideCooldown = 2.0f;
    [SerializeField]
    private float MinimumSpeedRequiredToSlide;
    private Timer SlideCooldownTimer;

    public ScriptableAudio SlideAudio = null;

    public bool UseImpulseSlide = true;
    public float ImpulseSlideScale = 15.0f;
    public float ImpulseSlideDuration = 0.4f;
    private Timer SlideEndTimer;

    private Timer SlideLenienceTimer;
    [Tooltip("How long the player can slide after leaving the acceptable range of speed required to slide. This allows you to slide in place so set with caution.")]
    public float SlideLenienceDuration = 0.25f;
    [SerializeField]
    private bool HasHadLenience = false;

    [Header("Parkour Combo"), SerializeField, Tooltip("This is the value that gets multiplied by the combo multiplier to boos movement speed. movespeed = movespeed * (1 + (ComboMultValue * ComboMultiplier))")]
    private float ComboMultValue = 0.1f;

    [Header("Audio")]
    public ScriptableAudio FootstepAudio = null;
    public float StepLengthRequiredForAudioPlayback = 5.0f;
    private float CurrentStepLength = 0.0f;
    private Vector3 PreviousPosition;

    private AudioChannel TwoDAudioChannel = null;
    [Tooltip("The minimum speed the player needs to be before air-like sounds will be heard. If left at -1, will use player speed as the low end.")]
    public float MinimumSpeedToHearWindAudio = -1;
    [Tooltip("The maximum speed the player needs to be before air-like sounds are at maximum volume potential. If left at -1, will use the max player speed as the high end.")]
    public float MaximumSpeedToHearMaxVolumeWindAudio = -1;
    [Tooltip("Scale how intense the wind sound is."), Range(0.0f, 1.0f)]
    public float WindSoundScale = 0.25f;


    //idle checker
    Timer FireIdleEventTimer = null;
    public float IdleTimeRequiredForEvent = 20.0f;


    private void Start()
    {
        m_InputComponent = GetComponent<PlayerInputComponent>();
        PSM = GetComponent<PlayerStateMachine>();
        m_Controller = GetComponentInChildren<PlayerController>();
        m_SoundChannel = GetComponent<AudioChannel>();
        m_DebuffSystem = GetComponent<DebuffSystem>();

        Transform obj = transform.Find("2D Wind Audio Channel Object");
        if (obj)
        {
            TwoDAudioChannel = obj.gameObject.GetComponent<AudioChannel>();
        }

        //define using variables if they were not set
        if (MinimumSpeedToHearWindAudio == -1)
        {
            MinimumSpeedToHearWindAudio = Speed;
            MaximumSpeedToHearMaxVolumeWindAudio = Speed * SprintMult;
        }

        m_PlayerAnimator = GetComponent<Animator>();
        if (m_PlayerAnimator != null)
        {
            m_IdleLayerIndex = m_PlayerAnimator.GetLayerIndex("Idle");
            m_CrouchingLayerIndex = m_PlayerAnimator.GetLayerIndex("Crouching");
            m_UncrouchLayerIndex = m_PlayerAnimator.GetLayerIndex("Uncrouch");
            m_SlidingLayerIndex = m_PlayerAnimator.GetLayerIndex("Sliding");
            m_CrouchWalkingLayerIndex = m_PlayerAnimator.GetLayerIndex("Crouch_Walking");
            m_FallingLayerIndex = m_PlayerAnimator.GetLayerIndex("Falling");
            m_RunningLayerIndex = m_PlayerAnimator.GetLayerIndex("Running");
            m_WallRunningRightLayerIndex = m_PlayerAnimator.GetLayerIndex("WallRunning_Right");
            m_WallRunningLeftLayerIndex = m_PlayerAnimator.GetLayerIndex("WallRunning_Left");
            m_StopRunningLayerIndex = m_PlayerAnimator.GetLayerIndex("Stop_Running");
            m_Slide2CrouchLayerIndex = m_PlayerAnimator.GetLayerIndex("Slide2Crouch");
            m_Slide2RunLayerIndex = m_PlayerAnimator.GetLayerIndex("Slide2Run");
            m_InspectLayerIndex = m_PlayerAnimator.GetLayerIndex("Inspect");
        }

        MinimumSpeedRequiredToSlide = (SprintMult * Speed) * 0.75f;
        PreviousPosition = transform.position;
        SlideCooldownTimer = CreateTimer(SlideCooldown, null, true);
        TimeUntilSpecialIdleAnimation = CreateTimer(TimeBetweenSpecialIdleAnimations);
        SlideEndTimer = CreateTimer(ImpulseSlideDuration, UnslideAnimPt1);
        SlideLenienceTimer = CreateTimer(SlideLenienceDuration, SetLenience, false, false, true);
        FireIdleEventTimer = CreateTimer(IdleTimeRequiredForEvent, () => EventManager.TriggerEvent("OnPlayerIdleEvent"), false, false, true);
    }

    void Update()
    {
        UpdateCrouchState();
        UpdateWindAudio();
        if(m_InputComponent.IsFiring())
        {
            CancelIdleAnim();
        }
    }

    private void FixedUpdate()
    {
        HandleCurrentSpeed();
        UpdateMovementBasedOnActiveState();
    }

    void UpdateMovementBasedOnActiveState()
    {
        StateBase state = PSM.GetActiveState();

        if (state.Is<StateOnGround>() || state.Is<StateCrouching>())
        {
            UpdateOnGround();
        }
        else if (state.Is<StateInAir>())
        {
            UpdateInAir();
        }
        else if (state.Is<StateWallRunning>())
        {
            ControllerGravityScaleAccessor = CurrentGravityScale.Value;
            UpdateWallRunning();
        }
    }

    void UpdateWindAudio()
    {
        if (TwoDAudioChannel != null)
        {
            float speedToCompare = m_CurrentSpeed * m_CurrentSprintMult;

            if (speedToCompare <= MinimumSpeedToHearWindAudio)
            {
                TwoDAudioChannel.SetSourceVolume(0.0f);
                return;
            }

            speedToCompare -= MinimumSpeedToHearWindAudio;
            float speedHighEnd = MaximumSpeedToHearMaxVolumeWindAudio - MinimumSpeedToHearWindAudio;

            //calculate final volume and scale based on the wind scale set by prefab
            float vol = speedToCompare / speedHighEnd * WindSoundScale;

            //clamp in case some bullshit wind scale was set somehow and we dont kill our players ears.
            vol = Mathf.Clamp(vol, 0.0f, 1.0f);

            TwoDAudioChannel.SetSourceVolume(vol);
        }
    }

    void HandleCurrentSpeed()
    {
        Vector3 moveDirection = m_InputComponent.GetRelativeMoveInput();
        float angle = MovementMath.AngleInDirection(out m_Slopedir, gameObject, moveDirection);
        //prep variables for use
        m_CurrentSpeed = MovementMath.DetermineSpeedFromTerrain(angle, Speed, m_Controller.CharCont.slopeLimit, MinAngleToSlowSpeed);
        HandleDebuffs();

        if (m_InputComponent.GetMoveInput() == Vector3.zero && FireIdleEventTimer.IsRunning == false)
        {
            FireIdleEventTimer.StartTimer();
        }
        else if (m_InputComponent.GetMoveInput() != Vector3.zero && FireIdleEventTimer.IsRunning == true)
        {
            FireIdleEventTimer.Reset();
        }
    }

    void HandleDebuffs()
    {
        List<MovementDebuff> debuffs = m_DebuffSystem.GetAllDebuffsOfType<MovementDebuff>();
        float speed = 0.0f;
        foreach (var debuff in debuffs)
        {
            speed -= (debuff).SpeedEffect;
        }

        m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed + speed, 0.0f, 50.0f);
    }

    void UpdateCrouchState()
    {
        StateBase st = PSM.GetActiveState();


        if (st.Is<StateOnGround>())
        {
            if (SlideLenienceTimer.IsRunning || HasHadLenience)
            {
                if (m_CurrentSpeed * m_CurrentSprintMult > MinimumSpeedRequiredToSlide)
                {
                    SlideLenienceTimer.Reset();
                    HasHadLenience = false;
                }
            }
            else if (HasHadLenience == false)
            {
                if (m_CurrentSpeed * m_CurrentSprintMult <= MinimumSpeedRequiredToSlide)
                {
                    SlideLenienceTimer.StartTimer();
                }
            }
        }
        

        //Get out if we're not crouching or on ground because input is irrelevant unless we're in a possible state to allow crouching.
        if (st.Is<StateOnGround>() == false && st.Is<StateCrouching>() == false)
        {
            return;
        }

        if (m_InputComponent.IsCrouching())
        //We're attempting to crouch. Either enter the crouch state or update our speed multiplier.
        {
            //This is where we handle entering the Crouch State.
            if (st.Is<StateOnGround>())
            {
                PSM.SetMovementStateForced<StateCrouching>();

                //If we're sprinting when we entered a crouch we're going to scale the multiplier up to simulate a speed boost into the slower crouch

                //If the slide timer is still on cooldown, crouch instead
                if (SlideCooldownTimer.IsRunning == false)
                {
                    //If they try to slide but not going fast enough, crouch instead if lenience is too long.
                    if (m_CurrentSpeed * m_CurrentSprintMult > MinimumSpeedRequiredToSlide || SlideLenienceTimer.IsRunning)
                    {
                        //Debug.Log("Entered Crouch while Sprinting. Initiating slide.");
                        m_SoundChannel.PlayAudio(SlideAudio);
                        EventManager.TriggerEvent("OnPlayerSlide");
                        SlideCooldownTimer.StartTimer();

                        SlideLenienceTimer.Reset();
                        HasHadLenience = false;

                        SlideAnim();

                        //using 2 different logic sets here for different sliding algorithms
                        if (UseImpulseSlide == true)
                        {
                            m_Controller.AddImpulse(CalculateSlideImpulse(), eTweenFunc.LinearToTarget, ImpulseSlideDuration, PlayerImpulse.eImpulseType.Slide);
                        }
                        else
                        {
                            m_CurrentSprintMult = DefaultSlideMovementSpeedMultiplier;
                        }
                    }
                    else
                    {
                        CrouchAnim();
                    }
                }
                else
                {
                    CrouchAnim();
                }
            }
            //This is where we handling someone who is already crouching.
            else if (st.Is<StateCrouching>())
            {
                //This decays the speed multiplier in 1 of the slide algorithms
                if (UseImpulseSlide == false)
                {
                    //Decay the speed multiplier if we were sliding into a crouch
                    if (m_CurrentSprintMult > DefaultCrouchMovementSpeedMultiplayer)
                    {
                        m_CurrentSprintMult = MathUtils.LerpTo(TimeToDecaySlideMovementSpeed, DefaultCrouchMovementSpeedMultiplayer, DefaultCrouchMovementSpeedMultiplayer, Time.deltaTime);
                    }
                }
            }
        }
        else
        //We arent inputting a crouch currently but we could be still crouching. We need to uncrouch if legally able to.
        {
            if (st.Is<StateCrouching>())
            {
                //Check if we can legally uncrouch (there's nothing above us).
                RaycastHit safetyCheck;
                Physics.BoxCast(transform.position, transform.localScale, Vector3.up, out safetyCheck, transform.rotation, 1.5f);

                if (safetyCheck.collider == null)
                {
                    PSM.SetMovementStateForced<StateOnGround>();
                    UncrouchAnim();
                }
            }
        }
    }

    void SetLenience()
    {
        HasHadLenience = true;
    }

    /// <summary>
    /// Automatically fired by the audio timers.
    /// </summary>
    void PlayStepAudio()
    {
        m_SoundChannel.PlayAudio();
    }

    /// <summary>
    /// Public call for updating Step Audio tracker.
    /// </summary>
    //public void UpdateStepAudio()
    //{
    //    //If we're attempting to move we need to update our audio step tracker.
    //    if (m_InputComponent.GetMoveInput() != Vector3.zero)
    //    {
    //        CurrentStepLength += (transform.position - PreviousPosition).magnitude;
    //
    //        if (CurrentStepLength >= StepLengthRequiredForAudioPlayback)
    //        {
    //            CurrentStepLength = 0.0f;
    //            PlayStepAudio();
    //        }
    //    }
    //    else
    //    {
    //        CurrentStepLength = 0.0f;
    //    }
    //}

    /// <summary>
    /// Sends the moveinput for this frame to the controller based on input, terrain, speed, sprinting, etc
    /// </summary>
    private void UpdateOnGround()
    {
        StopWallRunAnim();
        if(m_PlayerAnimator != null)
        {
            //if were on ground stop falling anim
            m_FallingLayerWeight = Mathf.Lerp(m_FallingLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_FallingLayerIndex, m_FallingLayerWeight);
        }

        if(m_Controller.Velocity.magnitude == 0)
        {
            if(m_InputComponent.IsInspecting() && !m_InputComponent.IsFiring())
            {
                StartCoroutine(InspectAnim());
            }
        }
        else
        {
            CancelInspectAnim();
        }

        //we're on the ground so we're not jumping yet
        //get the input relative to us so when we click forward we are going relative forward
        Vector3 moveDirection = m_InputComponent.GetRelativeMoveInput();

        //UpdateStepAudio();

        float angle = MovementMath.AngleInDirection(out m_Slopedir, gameObject, moveDirection);

        HandleSprintAndCrouch();

        //only apply the slope math if actually on a slope
        if(MovementMath.IsOnSlope(angle, MinAngleToSlowSpeed))
        {
            moveDirection = MovementMath.ApplySlopeToDirection(moveDirection, m_Slopedir);
        }
        StateBase state = PSM.GetActiveState();

        //get lccal velocity to actually tell which direction player is moving
        Vector3 LocalVel = transform.InverseTransformDirection(m_Controller.Velocity);
        //update speed in animator 
        m_PlayerAnimator.SetFloat("SpeedX", LocalVel.x);
        //y is z for blends trees 2D cartesion plain
        m_PlayerAnimator.SetFloat("SpeedY", LocalVel.z);
        //m_PlayerAnimator.SetBool("DidPlayerStop", DidPlayerStop);
        if (state.Is<StateOnGround>() && m_Controller.Velocity.magnitude != 0)
        {
            RunAnim();
            m_StopRunningLayerWeight = Mathf.Lerp(m_StopRunningLayerWeight, 0.0f, 1);
            DidPlayerStop = false;
        }
        //else if (state.Is<StateCrouching>() && m_Controller.Velocity.magnitude != 0)
        //{
        //    CrouchWalkAnim();
        //}
        else
        {
            if (!DidPlayerStop)
            {
                m_StopRunningLayerWeight = Mathf.Lerp(m_StopRunningLayerWeight, 1.0f, 1);
                if (m_StopRunningLayerWeight == 1.0f)
                {
                    DidPlayerStop = true;
                    StartCoroutine(StopRunAnim());
                }
            }
        }

        if(TimeUntilSpecialIdleAnimation.IsTimerComplete())
        {
            PlaySpecialIdleAnim();
        }

        //set up via the above functions
        ApplyComboMult(ref m_CurrentSpeed);
        moveDirection *= m_CurrentSpeed;
        moveDirection *= m_CurrentSprintMult;
        moveDirection.y -= 10;


        // Move the controller
        SendMoveInputToController(moveDirection);
        PreviousPosition = transform.position;
    }

    /// <summary>
    /// Sends the moveinput for this frame to the controller based input and air movement speed
    /// </summary>
    private void UpdateInAir()
    {
        CancelRunAnim();
        CancelIdleAnim();
        StopWallRunAnim();
        
        
        if(m_Controller.Velocity.y < -8.0f)
        {
            FallAnim();
        }

        //current velocity
        Vector3 velocity = m_Controller.Velocity;

        Vector3 moveDirection = m_InputComponent.GetRelativeMoveInput();

        //increase magnitude of movement based on how fast we want airspeed to be
        moveDirection *= (AirSpeed / Speed) * m_CurrentSpeed;
        float mag = velocity.magnitude;

        //add the movement change but keep the same magnitude
        velocity += moveDirection;
        velocity.Normalize();
        velocity.x *= mag;
        velocity.z *= mag;

        SendMoveInputToController(velocity);
    }

    /// <summary>
    /// Scales the speed multiplier based on crouch/sprint. If the player is crouching though, sprinting is ignored.
    /// </summary>
    public void HandleSprintAndCrouch()
    {
        if (m_InputComponent.IsCrouching())
        {
            //TODO crouch walk anim
            if (m_CurrentSprintMult > DefaultCrouchMovementSpeedMultiplayer)
            {
                m_CurrentSprintMult = MathUtils.LerpTo(TimeTillMaxSprintSpeed * 2, m_CurrentSprintMult, DefaultCrouchMovementSpeedMultiplayer, Time.fixedDeltaTime);
            }
        }
        else 
        {
            HandleSprint();
        }
    }

    /// <summary>
    /// Updates speed multiplier for sprint only.
    /// </summary>
    public void HandleSprint()
    {
        if (m_InputComponent.IsSprinting() && m_InputComponent.GetMoveInput().sqrMagnitude > 0.2f)
        {
            if (m_CurrentSprintMult < SprintMult)
            {
                m_CurrentSprintMult = MathUtils.LerpTo(TimeTillMaxSprintSpeed, m_CurrentSprintMult, SprintMult, Time.fixedDeltaTime);
            }
            if (MathUtils.FloatCloseEnough(m_CurrentSprintMult, SprintMult, 0.03f))
            {
                m_CurrentSprintMult = SprintMult;
            }
            
        }
        //if we are not sprinting
        else if (m_CurrentSprintMult > 1.0f)
        {
            //slow down faster than we speed up
            m_CurrentSprintMult = MathUtils.LerpTo(TimeTillMaxSprintSpeed * 3, m_CurrentSprintMult, 1.0f, Time.fixedDeltaTime);

            if (MathUtils.FloatCloseEnough(m_CurrentSprintMult, 1.0f, 0.03f))
            {
                m_CurrentSprintMult = 1.0f;
            }
        }
        else
        {
            m_CurrentSprintMult = MathUtils.LerpTo(TimeTillMaxSprintSpeed, m_CurrentSprintMult, 1.0f, Time.fixedDeltaTime);
            if (MathUtils.FloatCloseEnough(m_CurrentSprintMult, 1.0f, 0.03f))
            {
                m_CurrentSprintMult = 1.0f;
            }
        }
    }

    void UpdateWallRunning()
    {
        CancelRunAnim();
        HandleSprint();

        StateWallRunning state = PSM.GetState<StateWallRunning>();

        RaycastHit WallObjectHit = state.AttachedWallObject;
        bool IsRunningOnRightSide = state.IsRunningOnRightSide;

        if (WallObjectHit.collider != null)
        {
            //Create our direction vector by crossing our collision normal with the UP axis.
            Vector3 moveDirection = Vector3.Cross(Vector3.up, WallObjectHit.normal);

            moveDirection = IsRunningOnRightSide ? moveDirection : -moveDirection;

            //Rotate object to face along the wall you run across
            transform.rotation = Quaternion.LookRotation(moveDirection);

            Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + gameObject.transform.forward * 5.0f, Color.magenta);

            //Scale movement input based on whether or not the forward is held down but we dont allow backward movement.
            float forwardScale = m_InputComponent.GetVerticalMovement();
            forwardScale = Mathf.Clamp(forwardScale, 0.0f, 1.0f);

            //Apply movement speed, forward input scale, and current sprint scale
            moveDirection *= m_CurrentSpeed * forwardScale * m_CurrentSprintMult;

            //Add our step up vector.
            moveDirection += CurrentStepUpVector.Value;

            //Add left-right input vector.
            Vector3 HorizontalMovement = m_InputComponent.GetHorizantalMovement() * LeftAndRightMovement;
            if (IsRunningOnRightSide == false)
            {
                HorizontalMovement = -HorizontalMovement;
            }
            moveDirection += HorizontalMovement;

            SendMoveInputToController(moveDirection);
            
            //if IsRunningOnRightSide is true it will play right wallrunning animation if false it will play left side wallrunning animation
            WallRunAnim(IsRunningOnRightSide);
        }
    }

    /// <summary>
    /// The function that actually tells the controller to move. Isolated so any changes on the value before it is sent only need to be made once.
    /// </summary>
    /// <param name="input">The moveinput to be added BEFORE any deltatime multiplication. Just the Raw Value</param>
    void SendMoveInputToController(Vector3 input)
    {
        m_Controller.AddMoveInput(input * Time.fixedDeltaTime);
    }

    void ApplyComboMult(ref float speedToChange)
    {
        speedToChange = speedToChange * (1 + (ComboMultValue * ParkourComboManager.ComboMult));
    }

    /// <summary>
    /// called by state machine's state change function. Used to set variables based on state
    /// </summary>
    /// <param name="stateEvent"></param>
    public void RecieveStateChangeEvent(StateChangeStruct stateEvent)
    {
        //If we left Wallrunning we need to reset the gravity scale.
        if (stateEvent.PreviousState.Is<StateWallRunning>())
        {
            ExitWallRunningGravityTween();
            ExitStepUpVectorTween();
        }

        //We're switching to WallRunning. We restart the gravity scale.
        else if (stateEvent.CurrentState.Is<StateWallRunning>())
        {
            StartWallRunningGravityTween();
            StartStepUpVectorTween();
        }
    }

    void ExitWallRunningGravityTween()
    {
        //Clean tween
        if (GravityScaleTween != null)
        {
            if (GravityScaleTween.HasTimeRemaining())
            {
                GravityScaleTween.StopTweening(Tween.eExitMode.CompleteTweening);
                GravityScaleTween = null;
            }
        }

        CurrentGravityScale.Value = 1.0f;
        ControllerGravityScaleAccessor = CurrentGravityScale.Value;
    }

    void StartWallRunningGravityTween()
    {
        CurrentGravityScale.Value = 0.0f;
        ControllerGravityScaleAccessor = CurrentGravityScale.Value;
        GravityScaleTween = TweenManager.CreateTween(CurrentGravityScale, 1.0f, GravityScaleTweenDuration, eTweenFunc.LinearToTarget, null);
    }

    void StartStepUpVectorTween()
    {
        CurrentStepUpVector.Value = StepOntoWallVector;
        StepUpTween = TweenManager.CreateTween(CurrentStepUpVector, Vector3.zero, StepOntoWallTweenDuration, eTweenFunc.LinearToTarget, null);
    }

    void ExitStepUpVectorTween()
    {
        if (StepUpTween != null)
        {
            if (StepUpTween.HasTimeRemaining())
            {
                StepUpTween.StopTweening(Tween.eExitMode.CompleteTweening);
                StepUpTween = null;
            }
        }

        CurrentStepUpVector.Value = Vector3.zero;
    }

    /// <summary>
    /// Create a scaled Vector3 for sliding purposes.
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateSlideImpulse()
    {
        Vector3 movedir = m_InputComponent.GetRelativeMoveInput();

        movedir *= m_CurrentSpeed;
        movedir *= ImpulseSlideScale;
        movedir.y = -5.0f;

        return movedir;
    }

    public void UncrouchAnim()
    {
        if (m_PlayerAnimator != null)
        {
            m_CrouchingLayerWeight = Mathf.Lerp(m_CrouchingLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_CrouchingLayerIndex, m_CrouchingLayerWeight);
            m_UncrouchLayerWeight = Mathf.Lerp(m_UncrouchLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_UncrouchLayerIndex, m_UncrouchLayerWeight);
            //string needs to be crouching because its crouching anim reversed
            m_PlayerAnimator.Play("Crouching", m_UncrouchLayerIndex);
            GoIntoIdleAnimations();
        }
    }

    public void UnslideAnimPt1()
    {
        m_SlidingLayerWeight = Mathf.Lerp(m_SlidingLayerWeight, 0.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_SlidingLayerIndex, m_SlidingLayerWeight);
        StateBase st = PSM.GetActiveState();

        if (st.Is<StateCrouching>())
        {
            StartCoroutine(UnslideAnimPt2());
        }
        else if (st.Is<StateOnGround>() && m_Controller.Velocity.magnitude > 0.0f)
        {
            StartCoroutine(UnslideAnimPt3());
        }
    }

    IEnumerator UnslideAnimPt2()
    {
        m_Slide2CrouchLayerWeight = Mathf.Lerp(m_Slide2CrouchLayerWeight, 1.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_Slide2CrouchLayerIndex, m_Slide2CrouchLayerWeight);
        m_PlayerAnimator.Play("Sliding", m_Slide2CrouchLayerIndex);
        yield return new WaitForSeconds(0.5f);
        m_Slide2CrouchLayerWeight = Mathf.Lerp(m_Slide2CrouchLayerWeight, 0.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_Slide2CrouchLayerIndex, m_Slide2CrouchLayerWeight);
        ////go into crouching layer and skip to crouch idle
        m_CrouchingLayerWeight = 1.0f;
        m_PlayerAnimator.SetLayerWeight(m_CrouchingLayerIndex, m_CrouchingLayerWeight);
        m_PlayerAnimator.Play("Crouch_Idle", m_CrouchingLayerIndex);
    
    }

    IEnumerator UnslideAnimPt3()
    {
        m_Slide2RunLayerWeight = Mathf.Lerp(m_Slide2RunLayerWeight, 1.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_Slide2RunLayerIndex, m_Slide2RunLayerWeight);
        m_PlayerAnimator.Play("Sliding", m_Slide2RunLayerIndex);
        yield return new WaitForSeconds(0.5f);
        m_Slide2RunLayerWeight = Mathf.Lerp(m_Slide2RunLayerWeight, 0.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_Slide2RunLayerIndex, m_Slide2RunLayerWeight);

    }

    public void CrouchAnim()
    {
        if (m_PlayerAnimator != null)
        {
            CancelIdleAnim();
            m_UncrouchLayerWeight = Mathf.Lerp(m_UncrouchLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_UncrouchLayerIndex, m_UncrouchLayerWeight);
            m_CrouchingLayerWeight = Mathf.Lerp(m_CrouchingLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_CrouchingLayerIndex, m_CrouchingLayerWeight);
            m_PlayerAnimator.Play("Crouching", m_CrouchingLayerIndex);
        }
    }

    public void CrouchWalkAnim()
    {
        if (m_PlayerAnimator != null)
        {
            m_CrouchWalkingLayerWeight = Mathf.Lerp(m_CrouchWalkingLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_CrouchWalkingLayerIndex, m_CrouchWalkingLayerWeight);

        }
    }

    public void SlideAnim()
    {
        if (m_PlayerAnimator != null)
        {
            m_SlidingLayerWeight = Mathf.Lerp(m_SlidingLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_SlidingLayerIndex, m_SlidingLayerWeight);
            m_PlayerAnimator.Play("Sliding", m_SlidingLayerIndex);
            SlideEndTimer.Restart();
        }
    }

    public void RunAnim()
    {
        if(m_PlayerAnimator != null)
        {
            CancelIdleAnim();
            m_RunningLayerWeight = Mathf.Lerp(m_RunningLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_RunningLayerIndex, m_RunningLayerWeight);
            m_PlayerAnimator.Play("Run", m_RunningLayerIndex);

        }
    }

    IEnumerator StopRunAnim()
    {
        if (m_PlayerAnimator != null)
        {
            m_RunningLayerWeight = Mathf.Lerp(m_RunningLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_RunningLayerIndex, m_RunningLayerWeight);

            m_StopRunningLayerWeight = Mathf.Lerp(m_StopRunningLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_StopRunningLayerIndex, m_StopRunningLayerWeight);
            m_PlayerAnimator.Play("Stop_Running", m_StopRunningLayerIndex);

            DidPlayerStop = true;

            yield return new WaitForSeconds(0.5f);

            m_StopRunningLayerWeight = Mathf.Lerp(m_StopRunningLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_StopRunningLayerIndex, m_StopRunningLayerWeight);

            GoIntoIdleAnimations();
        }
    }
    //for when player stops running but doesnt need to play the stop running animation 
    public void CancelRunAnim()
    {
        if (m_PlayerAnimator != null)
        {
            m_RunningLayerWeight = Mathf.Lerp(m_RunningLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_RunningLayerIndex, m_RunningLayerWeight);
        }
    }

    public void FallAnim()
    {
        if (m_PlayerAnimator != null)
        {
            m_FallingLayerWeight = Mathf.Lerp(m_FallingLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_FallingLayerIndex, m_FallingLayerWeight);
            m_PlayerAnimator.Play("Falling", m_FallingLayerIndex);
        }
    }

    public void WallRunAnim(bool IsOnRight)
    {
        if(m_PlayerAnimator != null)
        {
            if(IsOnRight)
            {
                m_WallRunningRightLayerWeight = Mathf.Lerp(m_WallRunningRightLayerWeight, 1.0f, 1);
                m_PlayerAnimator.SetLayerWeight(m_WallRunningRightLayerIndex, m_WallRunningRightLayerWeight);
            }
            else
            {
                m_WallRunningLeftLayerWeight = Mathf.Lerp(m_WallRunningLeftLayerWeight, 1.0f, 1);
                m_PlayerAnimator.SetLayerWeight(m_WallRunningLeftLayerIndex, m_WallRunningLeftLayerWeight);
            }

        }
    }

    public void StopWallRunAnim()
    {
        if (m_PlayerAnimator != null)
        {
            m_WallRunningLeftLayerWeight = Mathf.Lerp(m_WallRunningLeftLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_WallRunningLeftLayerIndex, m_WallRunningLeftLayerWeight);
            m_WallRunningRightLayerWeight = Mathf.Lerp(m_WallRunningRightLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_WallRunningRightLayerIndex, m_WallRunningRightLayerWeight);
        }
    }

    public void GoIntoIdleAnimations()
    {
        TimeUntilSpecialIdleAnimation.Restart();
        m_IdleLayerWeight = Mathf.Lerp(m_IdleLayerWeight, 1.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_IdleLayerIndex, m_IdleLayerWeight);
        m_PlayerAnimator.Play("Idle", m_IdleLayerIndex);
    }


    public void PlaySpecialIdleAnim()
    {
        m_PlayerAnimator.SetBool("PlaySpecialIdle", true);
        //chooses number between 1 and 4
        SpecialIdleType = Random.Range(1, 5);
        if(SpecialIdleType ==1)
        {
            //ready
            m_PlayerAnimator.Play("Idle 1", m_IdleLayerIndex);
        }
        if (SpecialIdleType == 2)
        {
            //spin
            m_PlayerAnimator.Play("Idle 2", m_IdleLayerIndex);
        }
        if (SpecialIdleType == 3)
        {
            //lets go
            m_PlayerAnimator.Play("Idle 3", m_IdleLayerIndex);
        }

        if (SpecialIdleType == 4)
        {
            //lets go
            m_PlayerAnimator.Play("Idle 4", m_IdleLayerIndex);
        }

        SpecialIdleType = 0;
        m_PlayerAnimator.SetBool("PlaySpecialIdle", false);

        TimeUntilSpecialIdleAnimation.Restart();
    }

    public void CancelIdleAnim()
    {
        TimeUntilSpecialIdleAnimation.StopTimer();
        m_PlayerAnimator.SetLayerWeight(m_IdleLayerIndex, 0.0f);
    }

    public IEnumerator InspectAnim()
    {
        m_InspectLayerWeight = Mathf.Lerp(m_InspectLayerWeight, 1.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_InspectLayerIndex, m_InspectLayerWeight);
        m_PlayerAnimator.Play("Inspect", m_InspectLayerIndex);
        yield return new WaitForSeconds(7.0f);
        m_InspectLayerWeight = Mathf.Lerp(m_InspectLayerWeight, 0.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_InspectLayerIndex, m_InspectLayerWeight);
    }

    public void CancelInspectAnim()
    {
        m_InspectLayerWeight = Mathf.Lerp(m_InspectLayerWeight, 0.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_InspectLayerIndex, m_InspectLayerWeight);
    }

    public void CancelJump()
    {
        m_CrouchingLayerWeight = 0.0f;
        m_PlayerAnimator.SetLayerWeight(m_CrouchingLayerIndex, m_CrouchingLayerWeight);
    }

    protected override void OnSoftReset()
    {
        m_CrouchingLayerWeight = 0.0f;
        m_UncrouchLayerWeight = 0.0f;
        m_SlidingLayerWeight = 0.0f;
        m_CrouchWalkingLayerWeight = 0.0f;
        m_FallingLayerWeight = 0.0f;
        m_RunningLayerWeight = 0.0f;
        m_WallRunningRightLayerWeight = 0.0f;
        m_WallRunningLeftLayerWeight = 0.0f;
        m_StopRunningLayerWeight = 0.0f;
        m_Slide2CrouchLayerWeight = 0.0f;
        m_Slide2RunLayerWeight = 0.0f;
        m_InspectLayerWeight = 0.0f;


        m_PlayerAnimator.SetLayerWeight(m_CrouchingLayerIndex, m_CrouchingLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_UncrouchLayerIndex, m_UncrouchLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_SlidingLayerIndex, m_SlidingLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_CrouchWalkingLayerIndex, m_CrouchWalkingLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_FallingLayerIndex, m_FallingLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_RunningLayerIndex, m_RunningLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_WallRunningRightLayerIndex, m_WallRunningRightLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_WallRunningLeftLayerIndex, m_WallRunningLeftLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_StopRunningLayerIndex, m_StopRunningLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_Slide2CrouchLayerIndex, m_Slide2CrouchLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_Slide2RunLayerIndex, m_Slide2RunLayerWeight);
        m_PlayerAnimator.SetLayerWeight(m_InspectLayerIndex, m_InspectLayerWeight);


    }



#if UNITY_EDITOR
    //So changing timers on the fly works.
    private void OnValidate()
    {
        if (SlideLenienceTimer != null && SlideLenienceDuration != SlideLenienceTimer.Duration)
        {
            SlideLenienceTimer.SetDuration(SlideLenienceDuration);
        }

        if (SlideLenienceTimer != null && ImpulseSlideDuration != SlideEndTimer.Duration)
        {
            SlideEndTimer.SetDuration(ImpulseSlideDuration);
        }

        if (SlideLenienceTimer != null && SlideCooldown != SlideCooldownTimer.Duration)
        {
            SlideCooldownTimer.SetDuration(SlideCooldown);
        }
    }
#endif

}