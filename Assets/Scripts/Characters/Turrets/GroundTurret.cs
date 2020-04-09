using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TurretStateMachine))]
[RequireComponent(typeof(ShootComponent))]
public class GroundTurret : BasicTurret
{
    [Header("Turret Rotation")]
    [Tooltip("Half rotation amount of turrets when patrolling in idle state")]
    public float TurretPatrolHalfRotateAmount = 45.0f;
    [Tooltip("How fast the turret rotates in idle state")]
    public float TurretIdleRotateSpeed = 1.0f;
    [Tooltip("How fast the turret rotates in disabled state ( how fast it lowers its head)")]
    public float TurretDisabledRotateSpeed = 0.25f;
    //number to keep track if turret should get "dizzy" or not
    public int NumberOfContinuosRotations = 0;

    StateBase CurrentState;
    ShootComponent TurretShootComponent;

    GameObject LRTT = null;

    float LRTTY = 0.0f;

    protected override void Awake()
    {
        base.Awake();
        TurretShootComponent = GetComponent<ShootComponent>();
        Listen("GroundTurretDeath", GroundTurretDeath);

        //either left or right target ( 1 or 2)
        LRTT = this.transform.GetChild(1).gameObject;

        LRTTY = LRTT.transform.position.y;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        m_InputComponent = GetComponent<AIInputComponent>();
    }

    public void FixedUpdate()
    {
        

        if (TurretStateMachine.CurrentState.Is<StateDisabled>() != true)
        {
            Vector3 desiredDir = m_InputComponent.GetControlRotation();

            if (desiredDir == Vector3.zero)
                TurretLookRotation = Quaternion.identity;
            else
                TurretLookRotation = Quaternion.LookRotation(desiredDir);

            Vector3 desiredRot = Quaternion.RotateTowards(gameObject.transform.rotation, TurretLookRotation, Time.fixedDeltaTime * m_InputComponent.GetRotationSpeed()).eulerAngles;
            //gameObject.transform.rotation = Quaternion.Euler(0.0f, desiredRot.y, 0.0f);
            gameObject.transform.rotation = Quaternion.Euler(desiredRot.x, desiredRot.y, 0.0f);
        }
        else
        {
            Vector3 desiredDir = m_InputComponent.GetControlRotation();
            
            if (desiredDir == Vector3.zero)
                TurretLookRotation = Quaternion.identity;
            else
                TurretLookRotation = Quaternion.LookRotation(desiredDir);
            
            Vector3 desiredRot = Quaternion.RotateTowards(gameObject.transform.rotation, TurretLookRotation, Time.fixedDeltaTime * m_InputComponent.GetRotationSpeed()).eulerAngles;
            gameObject.transform.rotation = Quaternion.Euler(desiredRot.x, gameObject.transform.rotation.eulerAngles.y, 0.0f);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        //Tween toTween = TweenManager.CreateTween(DesiredRotation, Vector3.zero, 2.0f, eTweenFunc.LinearToTarget, null);
        //toTween.StopTweening(Tween.eExitMode.IncompleteTweening);
        GroundTurretDeath();
        base.Update();

        StateBase state = TurretStateMachine.CurrentState;

        //We only care about updating things if the turret isnt disabled.
        //if (state.Is<StateDisabled>() != true)
        //{
            //Draw the line renderer based on raycast hit.
            if (AimingLaser && Physics.Raycast(this.gameObject.transform.position, this.gameObject.transform.forward, out RaycastHit LaserRayHit))
            {
                //sets end point of line renderer to whatever the raycast hit by adding the distance from what it hits to the start of the line
                AimingLaser.SetPosition(1, AimingLaser.GetPosition(0) + new Vector3(0.0f, 0.0f, Vector3.Distance(LaserRayHit.transform.position, this.gameObject.transform.position)));
            }

            //Shoot if able to. Only GroundTurrets shoot.
            if (m_InputComponent.IsFiring())
            {
                Shoot();
            }
        //}
    }

    public override void Respawn(bool forced = false)
    {
        base.Respawn(forced);

    }

    public void GroundTurretDeath()
    {
        //StateBase state = TurretStateMachine.CurrentState;
        //
        ////only do death stuff if turret is disabled 
        //if (state.Is<StateDisabled>() == true)
        //{
        //    
        //    gameObject.transform.rotation = Quaternion.Euler(Mathf.Lerp(this.gameObject.transform.rotation.x, 45.0f, 1), this.gameObject.transform.rotation.y, this.gameObject.transform.rotation.z );
        //
        //    //zappy particles here
        //}

        
    }

    //spawns and fires the turret projectile 
    public override void Shoot()
    {
        base.Shoot();
        TurretShootComponent.Shoot();
    }

    public void RecieveStateChangeEvent(StateChangeStruct stateEvent)
    {
        CurrentState = stateEvent.CurrentState;
        if (CurrentState.Is<StateAggressiveGround>())
        {
            if (AimingLaser)
                AimingLaser.enabled = true;
        }
        else
        {
            if (AimingLaser)
                AimingLaser.enabled = false;
        }
        if(CurrentState.Is<StateDisabled>())
        {
            //GroundTurretRotateSound rotateSound = GetComponentInChildren<GroundTurretRotateSound>();
            //rotateSound.StopRotateAudio();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(gameObject.transform.position, TurretRange);
        //Gizmos.DrawLine(this.gameObject.transform.position, this.gameObject.transform.forward * 10);
    }
}
