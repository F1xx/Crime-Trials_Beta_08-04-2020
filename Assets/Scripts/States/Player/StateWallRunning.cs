using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StateWallRunning switch rules:
///  - If on ground                     : StateOnGround
///  - No Walls near                    : StateInAir
///  - Jump Event                       : StateInAir
/// </summary>
/// 
[System.Serializable, CreateAssetMenu(fileName = "State", menuName = "ScriptableObjects/State/Player/WallRunning", order = 1)]
public class StateWallRunning : StateBase
{
    //private float CheckForGroundRadius = 0.5f;
    //private float MinAllowedSurfaceAngle = 15.0f;

    [HideInInspector]
    public GameObject BaseLoc;

    /// <summary>
    /// Creating an impossible normal here so that no matter what when you're wallrunning you do not break out due to similar wall normals since that isn't relevant
    /// in wallrunning context. So we're going to pass something that will never match whatever unity spits out.
    /// </summary>
    private readonly Vector3 ImpossibleNormal = new Vector3(10, 10, 10);

    /// <summary>
    /// If the character will be wallrunning from the right or not.
    /// </summary>
    [HideInInspector]
    public bool IsRunningOnRightSide = true;

    /// <summary>
    /// This is the object the character is following.
    /// </summary>
    [HideInInspector]
    public RaycastHit AttachedWallObject;

    /// <summary>
    /// Internal tracking for knowing the last normal successfully ran on to pass back to InAit when exiting state.
    /// </summary>
    
    private Vector3 MostRecentNormal = Vector3.zero;

    private float DistanceFromWallRequiredToMount = 3.0f;
    private string TagToCompareForWallRunning = "WallRunnable";

    private Transform m_Transform;
    private StateInAir m_StateInAir;

    private PlayerInputComponent m_PlayerInputComponent;
    CharacterController characterController;


    public override void Start()
    {
        base.Start();

        m_Transform = SM.transform;
        BaseLoc = SM.GetComponent<Character>().BaseLocationObj;
        m_StateInAir = SM.GetState<StateInAir>();
        characterController = SM.GetComponent<CharacterController>();


        PlayerMovementComponent wallComp = SM.GetComponent<PlayerMovementComponent>();
        //SM.GetComponent<JumpComponent>().SubscribeToJumpEvents(JumpEvent);
        if (wallComp)
        {
            DistanceFromWallRequiredToMount = wallComp.DistanceFromWallRequiredToMount;
            TagToCompareForWallRunning = wallComp.TagToCompareForWallRunning;
        }

        m_PlayerInputComponent = SM.GetComponent<PlayerInputComponent>();

        EventManager.StartListening("OnPlayerJump", OnJumpActivation);
    }

    public override void ActivateState()
    {
        base.ActivateState();
        //Debug.Log("Entered WallRunning.");
        EventManager.TriggerEvent("OnStartWallRunning");
    }

    public override void DeactivateState()
    {
        base.DeactivateState();
        //Debug.Log("Exited WallRunning.");
        EventManager.TriggerEvent("OnLeaveWallRunning");
    }

    public override void LateUpdate()
    {
        if (IsTouchingGround())
        {
            return;
        }
        if (IsForwardInputActive())
        {
            return;
        }
        if (CheckForWallCollisions())
        {
            return;
        }
        
    }

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
        //    ExitStateTo<StateOnGround>();
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
    /// Check for whether or not the character is still running along a wall each LateUpdate.
    /// If theres no wall anymore then the state changes back to InAir and returns true.
    /// Return false if no state change.
    /// </summary>
    /// <returns></returns>
    private bool CheckForWallCollisions()
    {
        //We pass an impossible normal here to ensure we will continue to wallrun regardless of the surface we are on since we only care about the normal if we're going from
        //InAir back to WallRunning in a short timespan.
        if (MathUtils.CheckPlayerWallCollisions(m_Transform, DistanceFromWallRequiredToMount, out AttachedWallObject, out IsRunningOnRightSide, ImpossibleNormal) == true)
        {
            MostRecentNormal = AttachedWallObject.normal;
            return false;
        }
        else
        {
            ExitStateTo<StateInAir>();
            return true;
        }
    }

    bool IsForwardInputActive()
    {
        if (m_PlayerInputComponent.GetVerticalMovement() <= 0.0f)
        {
            ExitStateTo<StateInAir>();
            return true;
        }

        return false;
    }

    void OnJumpActivation()
    {
        if (SM.GetActiveState().Is<StateWallRunning>())
        {
            ExitStateTo<StateInAir>();
        }
    }

    void ExitStateTo<T>() where T : StateBase
    {
        m_StateInAir.m_LastObjectWallNormal = MostRecentNormal;
        m_StateInAir.StartWallrunningCooldown();
        SM.SetMovementState<T>();
    }
}
