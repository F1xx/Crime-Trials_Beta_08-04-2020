using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FlyingTurretStateMachine))]
public class FlyingTurret : BasicTurret
{
    // Start is called before the first frame update
    [Header("Flying Movement")]
    [Tooltip("How fast the turret will move")]
    public float Speed = 50.0f;
    [Tooltip("How close the object has to be to be considered at the next waypoint")]
    public float MinDist = 0.3f;
    [Tooltip("How far the turret will chase the player before going back to patrolling")]
    public float ChaseRange = 30.0f;
    [Tooltip("The waypoints that the Flying Turret will patrol.")]
    public List<GameObject> Waypoints = new List<GameObject>();
    [Tooltip("How long the turret will pause once it loses track of the player")]
    public float PauseTime = 3.0f;

    [Header("AI Pursuit Logic")]
    [Tooltip("How close the turret can be to the player")]
    public float MinDistanceToPlayer = 5.0f;
    [System.NonSerialized]
    public GameObject WayPointToMoveTo;

    Rigidbody TurretRB = null;

    [HideInInspector] public AudioChannel m_Channel { get; protected set; }

    StateBase CurrentState;

    public override void Start()
    {
        //aim
        base.Start();
        TurretRange = 15.0f;
        WayPointToMoveTo = GetClosestWayPoint();
        m_InputComponent = GetComponent<AIInputComponent>();
        TurretLookRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);


        m_Channel = GetComponent<AudioChannel>();

        TurretRB = GetComponent<Rigidbody>();

        //listen for if a cutscene starts or finishes 
        Listen("CutsceneStart", DisableDuringCutscene);
        Listen("CutsceneFinsished", EnableAfterCutscene);
        Listen("FlyingTurretDeath", FlyingTurretDeath);
        
        //m_Channel.PlayAudio();
    }

    public void FixedUpdate()
    {
        CurrentState = TurretStateMachine.CurrentState;

        //We only care about updating things if the turret isnt disabled.
        if (CurrentState.Is<StateDisabled>() != true)
        {
            TurretLookRotation = Quaternion.LookRotation(m_InputComponent.GetControlRotation());
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, TurretLookRotation, Time.fixedDeltaTime * TurretRotateSpeed);

            //if (WayPointToMoveTo == null && TurretStateMachine.CurrentState.Is<StatePatrollingFlying>())
            //{

            //}
            //else
            this.gameObject.transform.position = this.gameObject.transform.position + m_InputComponent.GetMoveInput() * Speed * Time.fixedDeltaTime;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        CurrentState = TurretStateMachine.CurrentState;

        //We only care about updating things if the turret isnt disabled.
        if (CurrentState.Is<StateDisabled>() != true)
        {
            //Draw the line renderer based on raycast hit.
            if (AimingLaser && Physics.Raycast(this.gameObject.transform.position, this.gameObject.transform.forward, out RaycastHit LaserRayHit))
            {
                //sets end point of line renderer to whatever the raycast hit by adding the distance from what it hits to the start of the line
                AimingLaser.SetPosition(1, AimingLaser.GetPosition(0) + new Vector3(0.0f, 0.0f, Vector3.Distance(LaserRayHit.transform.position, this.gameObject.transform.position)));

                //only flying turrets deal damage with the "aiming laser"
                if (LaserRayHit.transform.CompareTag("Player"))
                {
                    PlayerHealthComp.OnTakeDPS(LaserDamage * Time.deltaTime, this.gameObject);
                }
            }
        }

    }


    public GameObject GetClosestWayPoint()
    {
        GameObject closestObject = null;
        float distToBeat = Mathf.Infinity;

        foreach(GameObject waypoint in Waypoints)
        {
            float dist = Vector3.Distance(gameObject.transform.position, waypoint.transform.position);
            if(dist < distToBeat)
            {
                closestObject = waypoint;
                distToBeat = dist;
            }
        }

        return closestObject;    
    }

    public override void Respawn(bool forced = false)
    {
        base.Respawn(forced);
        TurretStateMachine.enabled = true;
        TurretRB.useGravity = false;
        TurretRB.isKinematic = true;
        PlayerFinder PF = GetComponent<PlayerFinder>();
        PF.enabled = false;

    }

    public void FlyingTurretDeath()
    {
        CurrentState = TurretStateMachine.CurrentState;

        //only do death stuff if turret is disabled 
        if (CurrentState.Is<StateDisabled>() == true)
        {
            TurretRB.useGravity = true;
            TurretRB.isKinematic = false;
            PlayerFinder PF = GetComponent<PlayerFinder>();
            PF.enabled = false;
            TurretStateMachine.enabled = false;
            //zappy particles here pls
        }
    }

    public void RecieveStateChangeEvent(StateChangeStruct stateEvent)
    {
        CurrentState = stateEvent.CurrentState;

        //Disable/enable the laser
        if (CurrentState.Is<StateAggressiveFlying>())
        {
            if (AimingLaser)
                AimingLaser.enabled = true;
        }
        else
        {
            if (AimingLaser)
                AimingLaser.enabled = false;
        }

        //stop hover audio on death
        if(CurrentState.Is<StateDisabled>())
        {
            m_Channel.StopAudio();
        }

        //start the hover audio on respawn
        if(CurrentState.Is<StatePatrollingFlying>())
        {
            m_Channel.PlayAudio();
        }
    }

    void DisableDuringCutscene()
    {
        //disables during cutscenes
        this.gameObject.SetActive(false);
    }

    void EnableAfterCutscene()
    {
        //enables sound when cutscene is skipped or finishes
        this.gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(SpawnPoint, 1.0f);

        Gizmos.DrawRay(gameObject.transform.position, this.gameObject.transform.forward * 3);
    }
}
