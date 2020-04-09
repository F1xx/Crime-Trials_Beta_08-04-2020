using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTurretInputComponent : AIInputComponent
{
    FlyingTurret FlyingTurretObj;
    List<GameObject> WayPoints = null;
    GameObject Player;

    Vector3 TurretDirection;
    [System.NonSerialized]
    public GameObject WayPointToMoveTo;

    //StateBase CurrentState = null;

    //Timer QuickPauseTimer;

    public override void Init(Character character)
    {

    }

    protected override void Awake()
    {
        base.Awake();

        Player = GameObject.Find("Player");
        FlyingTurretObj = this.gameObject.GetComponent<FlyingTurret>();
        TurretDirection = this.gameObject.transform.forward;
        WayPointToMoveTo = FlyingTurretObj.GetClosestWayPoint();
        //QuickPauseTimer = CreateTimer(FlyingTurretObj.PauseTime);

        Listen(gameObject, "OnDeathEvent", OnDeathEvent);
        Listen(gameObject, "OnRespawnEvent", OnRespawnEvent);
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    public override void UpdateControls()
    {
    }

    public override void SetFacingDirection(Vector3 direction)
    {
        TurretDirection = direction.normalized;
    }

    public override Vector3 GetControlRotation()
    {
        StateBase state = FlyingTurretObj.GetStateMachine().GetActiveState();
        WayPoints = FlyingTurretObj.Waypoints;
        float MinDist = FlyingTurretObj.MinDist;

        if (state.Is<StatePatrollingFlying>())
        {

            //if(QuickPauseTimer.IsRunning)
            //{
            //    TurretDirection.y -= 0.01f;
            //    return TurretDirection;
            //}


            //only go from point to point if they exist
            if (WayPoints.Count != 0)
            {

                //foreach (GameObject waypoint in WayPoints)
                //{
                //    if (waypoint == null)
                //    {
                //        Debug.LogWarning("Flying Turret: " +  gameObject.name + " does not have waypoints");
                //        return gameObject.transform.forward;
                //    }
                //}

                //Increment the waypoint if needed.
                if (Vector3.Distance(gameObject.transform.position, WayPointToMoveTo.transform.position) < MinDist)
                {
                    int indexOfNextWaypoint = WayPoints.IndexOf(WayPointToMoveTo) + 1;

                    //Increase by 1 or set back to 0.
                    if (indexOfNextWaypoint < WayPoints.Count)
                        WayPointToMoveTo = WayPoints[indexOfNextWaypoint];
                    else
                        WayPointToMoveTo = WayPoints[0];

                }

                TurretDirection = (WayPointToMoveTo.transform.position - gameObject.transform.position).normalized;
            }
        }
        else if (state.Is<StateAggressiveFlying>())
        {
            TurretDirection = (Player.transform.position - gameObject.transform.position).normalized;
        }

        return TurretDirection;
    }

    private void OnDeathEvent(EventParam param)
    {
        //QuickPauseTimer.Reset();
    }

    private void OnRespawnEvent()
    {

    }

    public override Vector3 GetMoveInput()
    {
        StateBase state = FlyingTurretObj.GetStateMachine().GetActiveState();

        if (state.Is<StateAggressiveFlying>() || state.Is<StateDisabled>())
        {
            //dont move if shooting or disabled
            return Vector3.zero;
        }
        //else if(QuickPauseTimer.IsRunning)
        //{
        //    //dont move if "paused"
        //    return Vector3.zero;
        //}

        return this.gameObject.transform.forward;
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
        return FlyingTurretObj.GetStateMachine().GetActiveState().Is<StateAggressiveFlying>();
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

    public void RecieveStateChangeEvent(StateChangeStruct stateEvent)
    {
        //CurrentState = stateEvent.CurrentState;
        //StateBase prevState = stateEvent.PreviousState;
        //if (CurrentState.Is<StatePatrollingFlying>() && !prevState.Is<StateDisabled>() )
        //{
        //    QuickPauseTimer.StartTimer();
        //}
    }

    protected override void OnSoftReset()
    {
        WayPointToMoveTo = FlyingTurretObj.GetClosestWayPoint();
    }
}
