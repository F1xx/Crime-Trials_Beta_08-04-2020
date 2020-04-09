using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTurretInputComponent : AIInputComponent
{
    [Header("AI Logic"), Tooltip("How long the player must be still before the turret considers firing.")]
    public float TimeUntilShoot = 2.0f;
    GroundTurret GroundTurretObj;
    GameObject Player;
    PlayerController PlayerController;
    float VelocityEpsilon = 4.0f;
    float ResetEpsilon = 4.0f;
    Timer ShootTimer;
    StateBase CurrentState = null;
    bool TimerDone = false;
    Vector3 TurretDirection;
    float TurretPatrolHalfRotateAmount;
    float TargetY;
    float NegTargetY;
    Vector3 TargetTurretDirection;

    bool GoingRight;
    bool GoingLeft;
    bool ResettingPatrolOrientation;

    
    public override void Init(Character character)
    {

    }

    protected override void Awake()
    {
        base.Awake();

        Player = GameObject.Find("Player");
        PlayerController = Player.GetComponent<PlayerController>();
        GroundTurretObj = GetComponent<GroundTurret>();
        ShootTimer = CreateTimer(TimeUntilShoot, OnTimerComplete);
        TurretDirection = transform.forward;

        GoingRight = true;
        GoingLeft = false;
        ResettingPatrolOrientation = false;
    }

    private void Start()
    {
        CurrentState = GroundTurretObj.GetStateMachine().GetActiveState();
        TurretPatrolHalfRotateAmount = GroundTurretObj.TurretPatrolHalfRotateAmount;
    }

    private void Update()
    {
        CheckIfTimerShouldRun();
        DidPlayerStopMoving();
        // if the turret is within 4 units of original rotation go back to patrolling
        if(ResettingPatrolOrientation && (gameObject.transform.rotation.eulerAngles.y >= GroundTurretObj.SpawnRot.eulerAngles.y + (360.0f - ResetEpsilon) 
                                        || (gameObject.transform.rotation.eulerAngles.y <= GroundTurretObj.SpawnRot.eulerAngles.y +  ResetEpsilon) ) )
        {
            ResettingPatrolOrientation = false;
        }
    }

    public override void UpdateControls()
    {
    }

    public override void SetFacingDirection(Vector3 direction)
    {
    }

    public override Vector3 GetControlRotation()
    {
        SetPatrolHalfRotationValues();
        
        if(CurrentState.Is<StateIdleGround>())
        {
            
                if (GoingRight)
                {
                    Transform target = gameObject.transform.GetChild(1);
                    //GameObject target = GameObject.Find("RightPatrolTarget");
                    TurretDirection = (target.transform.position - gameObject.transform.position).normalized;
                    //TurretDirection = new Vector3(TurretPatrolHalfRotateAmount, 0.0f, 0.0f);
                    if (gameObject.transform.rotation.eulerAngles.y >= (TargetY - 3.0f ) && gameObject.transform.rotation.eulerAngles.y <= (TargetY + 10.0f))
                    {
                        GoingRight = false;
                        GoingLeft = true;
                    }
                }
                
                if (GoingLeft)
                {
                    //GameObject target = GameObject.Find("LeftPatrolTarget");
                    Transform target = gameObject.transform.GetChild(2);
                    TurretDirection = (target.transform.position - gameObject.transform.position).normalized;
                    //TurretDirection = new Vector3(-TurretPatrolHalfRotateAmount, 0.0f, 0.0f);
                    if (gameObject.transform.rotation.eulerAngles.y <= (NegTargetY + 3.0f) && gameObject.transform.rotation.eulerAngles.y >= (NegTargetY - 10.0f))
                    {
                        GoingRight = true;
                        GoingLeft = false;
                    }
                }
        }

        if (CurrentState.Is<StateAggressiveGround>())
        {
            TurretDirection = (Player.transform.position - gameObject.transform.position).normalized;
            GoingRight = false;
            GoingLeft = true;
        }

        if(CurrentState.Is<StateDisabled>())
        {
            
            Transform target = gameObject.GetComponent<Character>().BaseLocationObj.transform;
            TurretDirection = (target.transform.position - gameObject.transform.position).normalized;

            GoingRight = false;
            GoingLeft = true;
        }

        return TurretDirection;
    }

    public override float GetRotationSpeed()
    {
        if(CurrentState.Is<StateIdleGround>())
        {
            return GroundTurretObj.TurretIdleRotateSpeed;
        }
        if(CurrentState.Is<StateDisabled>())
        {
            return GroundTurretObj.TurretDisabledRotateSpeed;
        }
        //if the turret is not idle rotate at its normal rotation speed
        return GroundTurretObj.TurretRotateSpeed;
    }
    public override Vector3 GetLookInput()
    {
        return Vector3.zero;
    }

    public override Vector3 GetAimTarget()
    {
        return Vector3.zero;
    }

    public override bool IsJumping()
    {
        return false;
    }

    public override bool IsFiring()
    {
        if(TimerDone)
        {
            TimerDone = false;
            return true;
        }
        return false;
    }

    public override bool IsAiming()
    {
        return false;
    }

    public override bool IsCrouching()
    {
        return false;
    }

    public override Vector3 GetRelativeMoveInput()
    {
        return Vector3.zero;
    }

    public bool IsPlayerTooSlow()
    {
        if(PlayerController.Velocity.magnitude < VelocityEpsilon)
        {
            //print("PLayer too slow");
            return true;

        }
        return false;
    }

    public void OnTimerComplete()
    {
        TimerDone = true;
        ShootTimer.StopTimer();
        ShootTimer.Reset();
    }

    private void CheckIfTimerShouldRun()
    {
        if(CurrentState.Is<StateAggressiveGround>() && IsPlayerTooSlow() && !ShootTimer.IsRunning)
        {
            //print("all true");
            ShootTimer.StartTimer();
        }
    }
   
    public void RecieveStateChangeEvent(StateChangeStruct stateEvent)
    {
        CurrentState = stateEvent.CurrentState;
        if(CurrentState.Is<StateAggressiveGround>())
        {
            //ShootTimer.Restart();
        }
        else
        {
            ShootTimer.StopTimer();
            ShootTimer.Reset();
        }
    }

    public void DidPlayerStopMoving()
    {
        if(!IsPlayerTooSlow())
        {
            ShootTimer.StopTimer();
            ShootTimer.Reset();
        }
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();

        ShootTimer.Reset();
    }

    private void SetPatrolHalfRotationValues()
    {
        // convert to radians now so you dont have to do it every time \\

        //if starting angle is between 0 and 315 targetY is the half rotation amount plus starting Y rotation
        if (GroundTurretObj.SpawnRot.eulerAngles.y <= 315.0f)
        {
            TargetY = TurretPatrolHalfRotateAmount + GroundTurretObj.SpawnRot.eulerAngles.y;
        }
        //if starting angle is between 315 and 360 targetY is half rotation amount plus starting Y rotation - 360 
        if (GroundTurretObj.SpawnRot.eulerAngles.y > 315 && GroundTurretObj.SpawnRot.eulerAngles.y <= 360.0f)
        {
            TargetY = (TurretPatrolHalfRotateAmount + GroundTurretObj.SpawnRot.eulerAngles.y) - 360.0f;
        }
        //if starting angle is between 0 and 45 negTargetY is 360 - half rotation amount
        if (GroundTurretObj.SpawnRot.eulerAngles.y <= 44.0f)
        {
            NegTargetY = 360.0f - (TurretPatrolHalfRotateAmount - GroundTurretObj.SpawnRot.eulerAngles.y);
        }
        //if angle is between 45 and 360 negTargetY is negative patrol half val - starting rot
        if (GroundTurretObj.SpawnRot.eulerAngles.y >= 45.0f && GroundTurretObj.SpawnRot.eulerAngles.y <= 360.0f)
        {
            NegTargetY = -(TurretPatrolHalfRotateAmount - GroundTurretObj.SpawnRot.eulerAngles.y);
        }
    }
}
