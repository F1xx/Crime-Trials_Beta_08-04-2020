using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerInputComponent))]
[RequireComponent(typeof(PlayerStateMachine))]
public class JumpComponent : BaseObject
{
    [Header("Jumping"), Tooltip("Raw jumping power applied as a force.")]
    public float JumpStrength = 25.0f;
    [Tooltip("Same as jumping power but specific only to double-jump.")]
    public float DoubleJumpStrength = 20.0f;

    [Tooltip("How much time the player has to jump after leaving the ground")]
    public float TimeCanJumpAfterLeavingGround = 0.25f;

    [Range(0.0f, 1.0f), Tooltip("How much the jump strength will be applied in the vertical direction")]
    public float VertJumpWeight = 1.0f;
    [Range(0.0f, 1.0f), Tooltip("How much the jump strength will be applied in the horizontal movement direction (recommend Low)")]
    public float HorizJumpWeight = 0.2f;

    public eTweenFunc JumpForceMode = eTweenFunc.LinearToTarget;
    [Tooltip("How long should the impulse be applied to the player. Increasing this means it will add the jump more and over longer. This results in a higher and more float-y jump.")]
    public float DurationOfJumpImpulse = 0.5f;

    [Header("Jump Sound")]
    public ScriptableAudio JumpAudio = null;
    public ScriptableAudio DoubleJumpAudio = null;

    bool m_HasJump = true;
    bool m_HasDoubleJump = true;
    bool m_InAirStillJump = true; //used for jumping if you just barely left a platform
    bool m_IsJumpQueued = false;

    // Start is called before the first frame update
    PlayerInputComponent m_InputComponent = null;
    PlayerController m_Controller = null;
    PlayerStateMachine PSM = null;
    AudioChannel m_SoundChannel = null;
    StateBase m_CurrentState = null;
    DebuffSystem m_DebuffSystem = null;

    Timer m_TimeSinceLeftGround = null;

    Animator m_PlayerAnimator = null;
    int m_IdleLayerIndex = 0;
    int m_DoubleJumpLayerIndex = 0;
    int m_JumpLayerIndex = 0;
    int m_LandingLayerIndex = 0;
    int m_UncoruchLayerIndex = 0;
    int m_FallingLayerIndex = 0;
    int m_CrouchLayerIndex = 0;
    float m_IdleLayerWeight = 0.0f;
    float m_DoubleJumpLayerWeight = 0.0f;
    float m_JumpLayerWeight = 0.0f;
    float m_LandingLayerWeight;
    float m_UncrouchLayerWeight = 0.0f;
    float m_CrouchLayerWEight = 0.0f;

    private void Start()
    {
        m_InputComponent = GetComponent<PlayerInputComponent>();
        m_Controller = GetComponentInChildren<PlayerController>();
        PSM = GetComponent<PlayerStateMachine>();
        m_SoundChannel = GetComponent<AudioChannel>();
        m_DebuffSystem = GetComponent<DebuffSystem>();

        m_PlayerAnimator = GetComponent<Animator>();
        if (m_PlayerAnimator != null)
        {
            m_IdleLayerIndex = m_PlayerAnimator.GetLayerIndex("Idle");
            m_DoubleJumpLayerIndex = m_PlayerAnimator.GetLayerIndex("Double_Jump");
            m_JumpLayerIndex = m_PlayerAnimator.GetLayerIndex("Jump");
            m_LandingLayerIndex = m_PlayerAnimator.GetLayerIndex("Landing");
            m_UncoruchLayerIndex = m_PlayerAnimator.GetLayerIndex("Uncrouch");
            m_CrouchLayerIndex = m_PlayerAnimator.GetLayerIndex("Crouch");
            m_FallingLayerIndex = m_PlayerAnimator.GetLayerIndex("Falling");
        }

        Listen("OnPlayerJumpReleased", OnJumpRelease);

        m_TimeSinceLeftGround = CreateTimer(TimeCanJumpAfterLeavingGround, InAirTooLongCallback);
    }

    private void FixedUpdate()
    {
        HandleJump();
    }

    private void HandleJump()
    {
        if (m_InputComponent.IsJumping() || m_IsJumpQueued)
        {
            Jump();   
        }
    }

    //simple cleanup function for readability
    private bool CanJump()
    {
        //no matter what if they're on the ground they can jump
        //reset the ground variables just in case. Either way we're on the ground so they're fine.
        if(m_Controller.CharCont.isGrounded)
        {
            ResetOnGround();
            return true;
        }

        return IsInJumpableState() && (m_HasJump || m_InAirStillJump);
    }

    private bool CanDoubleJump()
    {
        return m_HasDoubleJump && IsInJDoubleumpableState();
    }

    private void Jump()
    {
        m_IsJumpQueued = false;
        //TODO jump anim here also double jump anim if we have it
        if (CanJump())
        {
            JumpAnim();
            m_Controller.AddImpulse(JumpVector(), JumpForceMode, DurationOfJumpImpulse);
            m_SoundChannel.PlayAudio(JumpAudio);
            m_HasJump = false;
            m_InAirStillJump = false;

            EventManager.TriggerEvent("OnPlayerJump");
        }
        else if (CanDoubleJump())
        {
            DoubleJumpAnim();
            m_Controller.ResetInAir();
            m_Controller.AddImpulse(DoubleJumpVector(), JumpForceMode, DurationOfJumpImpulse);
            m_SoundChannel.PlayAudio(DoubleJumpAudio);
            m_HasDoubleJump = false;

            EventManager.TriggerEvent("OnPlayerJump");
            EventManager.TriggerEvent("OnPlayerDoubleJump");
        }
    }

    private Vector3 JumpVector()
    {
        Vector3 vec = CalculateImpulse(JumpStrength);

        return vec;
    }

    private Vector3 DoubleJumpVector()
    {
        Vector3 vec = CalculateImpulse(DoubleJumpStrength);
        //mess with the vector in here if you want a more different double jump than just strength

        return vec;
    }

    //math to calculate the weighted jump
    private Vector3 CalculateImpulse(float power)
    {
        Vector3 movedir = m_InputComponent.GetRelativeMoveInput();
        Vector3 JumpWeight = new Vector3(HorizJumpWeight, VertJumpWeight, HorizJumpWeight);

        Vector3 jumpVecScaled = Vector3.zero;
        jumpVecScaled.x = movedir.x * JumpWeight.x;
        jumpVecScaled.y = movedir.y * JumpWeight.y;
        jumpVecScaled.z = movedir.z * JumpWeight.z;

        //always want the y to be VertJumpWeight
        jumpVecScaled.y = VertJumpWeight;

        power = SumJumpDebuffs(power);

        jumpVecScaled *= power;

        jumpVecScaled.x = Mathf.Clamp(jumpVecScaled.x, -power, power);
        jumpVecScaled.y = Mathf.Clamp(jumpVecScaled.y, -power, power);
        jumpVecScaled.z = Mathf.Clamp(jumpVecScaled.z, -power, power);

        //decrease height for horizontal
        //jumpVec = Vector3.ClampMagnitude(jumpVec, power);

        return jumpVecScaled;
    }

    //just a vertical impulse with 100% power into y up
    private Vector3 CalculateImpulseIgnoreWeight(float power)
    {
        Vector3 jumpVec = Vector3.zero;

        jumpVec.y = 1.0f;
        jumpVec *= power;

        jumpVec.y = Mathf.Clamp(jumpVec.y, 0.0f, power);

        return jumpVec;
    }

    /// <summary>
    /// Grabs all current related debuffs and returns a float of them all added together
    /// </summary>
    /// <returns></returns>
    float SumJumpDebuffs(float power)
    {
        List<JumpDebuff> debuffs = m_DebuffSystem.GetAllDebuffsOfType<JumpDebuff>();
        foreach (var debuff in debuffs)
        {
            power -= (debuff).JumpEffect;
        }

        return Mathf.Clamp(power, 0.2f, float.MaxValue); ;
    }

    //called when the timer expires and the player can no longer jump without expending double jump
    public void InAirTooLongCallback()
    {
        m_InAirStillJump = false;
    }

    private bool IsInJumpableState()
    {
        bool canjump = false;

        //StateBase state = PSM.GetActiveState();

        if (m_CurrentState.Is<StateOnGround>())
        {
            canjump = true;
        }
        else if (m_CurrentState.Is<StateCrouching>())
        {
            canjump = false;
        }
        else if (m_CurrentState.Is<StateWallRunning>())
        {
            canjump = true;
        }
        else if (m_CurrentState.Is<StateInAir>() && (m_InAirStillJump == true || m_HasDoubleJump))
        {
            canjump = true;
        }

        return canjump;
    }

    private bool IsInJDoubleumpableState()
    {
        bool canjump = false;

        //StateBase state = PSM.GetActiveState();

        if (m_CurrentState.Is<StateInAir>())
        {
            canjump = true;
        }

        return canjump;
    }

    public void RecieveStateChangeEvent(StateChangeStruct stateEvent)
    {
        m_CurrentState = stateEvent.CurrentState;

        if (stateEvent.CurrentState.Is<StateOnGround>())
        {
            if(stateEvent.PreviousState.Is<StateCrouching>())
            {
                StartCoroutine(ResetOnGroundFromCrouch());
            }
            else
            {
                ResetOnGround();
            }
        }
        else if (stateEvent.CurrentState.Is<StateWallRunning>())
        {
            ResetOnGround();
        }
        else if (stateEvent.CurrentState.Is<StateInAir>())
        {
            m_HasJump = false;
            m_TimeSinceLeftGround.StartTimer();
        }
    }

    public void OnJumpRelease()
    {
        //if(m_CurrentState.Is<StateWallRunning>())
        //{
        //    m_IsJumpQueued = true;
        //}
    }

    public void JumpAnim()
    {
        if (m_PlayerAnimator != null)
        {
            CancelIdleAnim();
            m_JumpLayerWeight = Mathf.Lerp(m_JumpLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_JumpLayerIndex, m_JumpLayerWeight);
            m_PlayerAnimator.Play("Jump", m_JumpLayerIndex);
        }
    }

    public void DoubleJumpAnim()
    {
        if (m_PlayerAnimator != null)
        {
            //end jump animation 
            m_JumpLayerWeight = Mathf.Lerp(m_JumpLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_JumpLayerIndex, m_JumpLayerWeight);

            m_DoubleJumpLayerWeight = Mathf.Lerp(m_DoubleJumpLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_DoubleJumpLayerIndex, m_DoubleJumpLayerWeight);
            m_PlayerAnimator.Play("Double_Jump", m_DoubleJumpLayerIndex);

        }
    }

    public void LandingAnim()
    {
        if (m_PlayerAnimator != null)
        {
       
            if(m_PlayerAnimator.GetLayerWeight(m_FallingLayerIndex) > 0.0f)
            {
                m_PlayerAnimator.SetInteger("LandType", 1);
            }
            else if(m_PlayerAnimator.GetLayerWeight(m_JumpLayerIndex) > 0.0f)
            {
                m_PlayerAnimator.SetInteger("LandType", 2);
            }
            else if(m_PlayerAnimator.GetLayerWeight(m_DoubleJumpLayerIndex) > 0.0f)
            {
                m_PlayerAnimator.SetInteger("LandType", 3);
            }

            //end jump/double jump animation 
            m_JumpLayerWeight = Mathf.Lerp(m_JumpLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_JumpLayerIndex, m_JumpLayerWeight);
            m_DoubleJumpLayerWeight = Mathf.Lerp(m_DoubleJumpLayerWeight, 0.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_DoubleJumpLayerIndex, m_DoubleJumpLayerWeight);

            m_LandingLayerWeight = Mathf.Lerp(m_LandingLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_LandingLayerIndex, m_LandingLayerWeight);
            m_PlayerAnimator.Play("Landing", m_LandingLayerIndex);
        }
    }

    public void FinishLandingKeyFrame()
    {
        //end landing animation 
        m_LandingLayerWeight = Mathf.Lerp(m_LandingLayerWeight, 0.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_LandingLayerIndex, m_LandingLayerWeight);
        GoIntoIdleAnimations();
    }

    //used to reset variables that change when entering a grounded-type state
    public void ResetOnGround()
    {
        m_InAirStillJump = true;
        m_HasJump = true;
        m_HasDoubleJump = true;
        m_TimeSinceLeftGround.Reset();
        LandingAnim();
    }

    //stops landing anim after leaving crouched stated
    IEnumerator ResetOnGroundFromCrouch()
    {
        m_InAirStillJump = true;
        m_HasJump = true;
        m_HasDoubleJump = true;
        m_TimeSinceLeftGround.Reset();
        yield return new WaitForSeconds(1);
        //m_UncrouchLayerWeight already initialiazed to zero doesnt need to be reinitialized to zero
        m_PlayerAnimator.SetLayerWeight(m_UncoruchLayerIndex, m_UncrouchLayerWeight);
    }
    public void GoIntoIdleAnimations()
    {
        PlayerMovementComponent PMC = GetComponent<PlayerMovementComponent>();
        if (PMC != null)
        {
            PMC.TimeUntilSpecialIdleAnimation.Restart();
            m_IdleLayerWeight = Mathf.Lerp(m_IdleLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_IdleLayerIndex, m_IdleLayerWeight);
        }
    }
    public void CancelIdleAnim()
    {
        PlayerMovementComponent PMC = GetComponent<PlayerMovementComponent>();
        if (PMC != null)
        {
            PMC.TimeUntilSpecialIdleAnimation.StopTimer();
            m_PlayerAnimator.SetLayerWeight(m_IdleLayerIndex, 0.0f);
            PMC.CancelJump();
        }
        
    }
}
