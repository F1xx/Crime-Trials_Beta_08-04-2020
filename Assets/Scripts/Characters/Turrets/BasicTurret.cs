using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTurret : Enemy
{
    [Header("Turret Visibility")]
    [Tooltip("How far the turret can see the player")]
    public float TurretRange = 10.0f;
    [Tooltip("How fast the turret rotates")]
    public float TurretRotateSpeed = 1.0f;
    [Tooltip("Turrets field of view half size")]
    public float FOV = 45.0f;

    //protected bool IsRespawning = false;

    protected StateMachineBase TurretStateMachine;
    protected Shootable TurretShootable = null;

    protected Quaternion TurretLookRotation;

    ParticleSystem[] ParticleSystem;
    protected LineRenderer AimingLaser;
    //Vector3 FirePointPos;

    protected HealthComponent PlayerHealthComp;

    [Header("Damage")]
    public float LaserDamage = 100.0f;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Listen(gameObject, "OnDeathEvent", OnDeath);
        Listen("GroundTurretDeath", OnDeath);

    }

    protected override void Awake()
    {
        base.Awake();

        GameObject player = GameObject.Find("Player");
        if (player)
        {
            PlayerHealthComp = player.GetComponent<HealthComponent>();
        }

        m_TurretRange = TurretRange;

        ParticleSystem = GetComponentsInChildren<ParticleSystem>();

        AimingLaser = GetComponent<LineRenderer>();
        if (AimingLaser)
        {
            AimingLaser.enabled = false;
            AimingLaser.receiveShadows = false;
        }
        TurretStateMachine = GetComponent<StateMachineBase>();

        SpawnPoint = this.gameObject.transform.position;
        SpawnRot = this.gameObject.transform.rotation;

        TurretShootable = gameObject.transform.root.gameObject.GetComponentInChildren<Shootable>();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    //spawns and fires the turret projectile 
    public virtual void Shoot()
    {
    }

    public float GetTurretRange()
    {
        return m_TurretRange;
    }

    public StateMachineBase GetStateMachine()
    {
        return TurretStateMachine;
    }

    protected void OnDeath(EventParam deathParams)
    {
        TurretShootable.SetCanBeShot(false);
        foreach (ParticleSystem sys in ParticleSystem)
        {
            if (sys.name == "SparkParticle_Slow")
            {
                sys.Play();
            }

            else if (sys)
            {
                sys.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        OnDeathEventParam param = (OnDeathEventParam)deathParams;

        if (param.Attacker.name.ToUpper() == "PLAYER")
        {
            TurretShootable.TriggerShotEvent();
        }
    }

    public override void Respawn(bool forced = false)
    {
        base.Respawn(forced);
        if (TurretShootable)
        {
            TurretShootable.SetCanBeShot(true);
        }

        m_InputComponent.SetFacingDirection(gameObject.transform.rotation.eulerAngles);

        foreach (ParticleSystem sys in ParticleSystem)
        {
            if (sys.name == "SparkParticle_Slow")
            {
                sys.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            else if (sys)
            {
                sys.Play();
            }
        }

    }

    float m_TurretRange;
}
