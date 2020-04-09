using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StateInAir logic switch rules:
///  - If on ground                     : StateOnGround
///  - If near wall and conditions met  : StateWallRunning
/// </summary>
/// 
[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Player/InAir", order = 3)]
public class StateInAir : StateBase
{
    private Transform ObjTransform;
    private PlayerController PlayerControls;
    CharacterController characterController;


    //private float CheckForGroundRadius = 0.5f;
    //private float MinAllowedSurfaceAngle = 15.0f;

    [HideInInspector]
    public GameObject BaseLoc;

    //Wallrunning distance/velocity variables
    private float DistanceFromWallRequiredToMount = 3.0f;
    private float MaxSpeedRequiredToWallRunSquared = 20.0f;
    private string TagToCompareForWallRunning = "WallRunnable";

    //Cooldown/conditions for wallrunning
    bool IsOnCooldown = false;
    Timer CooldownTimer;
    bool IsHoldingJumpDown = false;
    public float DefaultWallRunningCooldown { get; private set; }

    Timer WallRunAllowedTimer;
    float WallRunAllowDelay = 0.25f;
    bool IsAllowedToWallRun = false;

    [HideInInspector]
    public Vector3 m_LastObjectWallNormal;
    private PlayerMovementComponent m_WallRunComponent;

    StateWallRunning WallRunning;

    ~StateInAir()
    {
        EventManager.StopListening("OnPlayerJumpReleased", OnJumpReleased);
        EventManager.StopListening("OnPlayerJumpPressed", OnJumpPressed);

        TimerManager.QueueForRemoval(CooldownTimer);
        TimerManager.QueueForRemoval(WallRunAllowedTimer);
        CooldownTimer = null;
        WallRunAllowedTimer = null;
    }

    public override void Start()
    {
        base.Start();
        BaseLoc = SM.GetComponent<Character>().BaseLocationObj;
        ObjTransform = SM.transform;
        PlayerControls = SM.GetComponent<PlayerController>();
        characterController = SM.GetComponent<CharacterController>();


        WallRunning = SM.GetState<StateWallRunning>();

        m_WallRunComponent = SM.GetComponent<PlayerMovementComponent>();
        if (m_WallRunComponent)
        {
            DistanceFromWallRequiredToMount = m_WallRunComponent.DistanceFromWallRequiredToMount;
            MaxSpeedRequiredToWallRunSquared = m_WallRunComponent.MaxSpeedRequiredToWallRunSquared;
            TagToCompareForWallRunning = m_WallRunComponent.TagToCompareForWallRunning;
            DefaultWallRunningCooldown = m_WallRunComponent.WallRunningCooldownOnSamelWall;
        }

        CooldownTimer = TimerManager.MakeTimer(DefaultWallRunningCooldown, CooldownFinished);
        WallRunAllowedTimer = TimerManager.MakeTimer(WallRunAllowDelay, EnableWallRunning);

        EventManager.StartListening("OnPlayerJumpReleased", OnJumpReleased);
        EventManager.StartListening("OnPlayerJumpPressed", OnJumpPressed);

        //We'll use the speed from basic movement if its available to set speed constraints
        PlayerMovementComponent bmc = SM.GetComponent<PlayerMovementComponent>();
        if (bmc != null)
        {
            MaxSpeedRequiredToWallRunSquared = bmc.Speed * bmc.Speed;
        }
        else
        {
            MaxSpeedRequiredToWallRunSquared *= MaxSpeedRequiredToWallRunSquared;
        }
    }

    public override void ActivateState()
    {
        base.ActivateState();
        IsAllowedToWallRun = false;
    }

    public override void LateUpdate()
    {
        //Check grounded second as this is #2 priority.
        if (IsTouchingGround())
        {
            return;
        }

        //Check WallRunning last as this is #3 priority.
        if (CheckForWallCollisions())
        {
            return;
        } 
    }

    /// <summary>
    /// Switch to OnGround if ground is found. Return true if state change, false otherwise.
    /// </summary>
    /// <returns></returns>
    private bool IsTouchingGround()
    {
        //RaycastHit groundHitInfo;
        //Vector3 raystart = SM.gameObject.transform.position;
        //Vector3 basePos = BaseLoc.transform.position;
        ////check to see if player died first
        //
        ////if after everything we didn't find ground we must be in the air (falling most likely, also after a jump)
        //if (MathUtils.CheckForGroundBelow(out groundHitInfo, raystart, basePos, CheckForGroundRadius, MinAllowedSurfaceAngle, SM.GroundCheckMask))
        //{
        //    if (groundHitInfo.transform.tag == "Projectile")
        //    {
        //        return false;
        //    }
        //
        //    SM.SetMovementState<StateOnGround>();
        //    return true;
        //}
        //
        //return false;
        if (characterController.isGrounded == true)
        {
            SM.SetMovementState<StateOnGround>();
            //Debug.Log("swap to ground");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check outwards for walls to wallrun on if conditions are met.
    /// </summary>
    /// <returns></returns>
    private bool CheckForWallCollisions()
    {
        //we can ignore wall collisions if the wallrunning state doesnt exist in this state machine.
        if (WallRunning == null)
        {
            return false;
        }

        //Small buffer room before you're allowed to wallrun == to WallRunAllowDelay
        if (IsAllowedToWallRun == false)
        {
            return false;
        }

        //Can only wallrun if we're holding jump down.
        if (IsHoldingJumpDown == false)
        {
            return false;
        }

        //check magnitude of X,Z speed to see if we're traveling fast enough to consider wallrunning.
        Vector3 currentVelocity = PlayerControls.Velocity;
        float magSq = (currentVelocity.x * currentVelocity.x) + (currentVelocity.z * currentVelocity.z);

        //if we arent fast enough then get out.
        if (magSq >= MaxSpeedRequiredToWallRunSquared - MathUtils.CompareEpsilon == false)
        {
            return false;
        }

        RaycastHit OurWall;
        bool IsRight;
           if (MathUtils.CheckPlayerWallCollisions(ObjTransform, DistanceFromWallRequiredToMount, out OurWall, out IsRight, m_LastObjectWallNormal, IsOnCooldown, TagToCompareForWallRunning) == true)
        {
            WallRunning.IsRunningOnRightSide = IsRight;
            WallRunning.AttachedWallObject = OurWall;
            SM.SetMovementState<StateWallRunning>();
            return true;
        }

        return false;
    }

    void CooldownFinished()
    {
        IsOnCooldown = false;
    }

    public void StartWallrunningCooldown()
    {
        IsOnCooldown = true;
        CooldownTimer.Reset();
        CooldownTimer.StartTimer();
    }

    void OnJumpPressed()
    {
        IsHoldingJumpDown = true;
        IsAllowedToWallRun = false;
        WallRunAllowedTimer.Reset();
        WallRunAllowedTimer.StartTimer();
    }

    void OnJumpReleased()
    {
        IsHoldingJumpDown = false;
    }

    void EnableWallRunning()
    {
        IsAllowedToWallRun = true;
    }
}
