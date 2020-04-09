using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInputComponent))]
[RequireComponent(typeof(PlayerHealthComponent))]
public class Player : Character
{
    protected RespawnPoint m_RespawnPoint = null;

    ShootComponent m_Shootcomp = null;

    Animator m_PlayerAnimator = null;
    int m_ShootingLayerIndex = 0;
    int m_AimLayerIndex = 0;
    int m_UnaimLayerIndex = 0;
    int m_IdleLayerIndex = 0;
    int m_DeathLayerIndex = 0;
    float m_ShootingLayerWeight = 0.0f;
    float m_AimLayerWeight = 0.0f;
    float m_UnaimLayerWeight = 0.0f;
    float m_IdleLayerWeight = 0.0f;
   



    [SerializeField]
    Material[] ArmSkins = null;

    private bool InitialFireInput = false;

    protected override void Awake()
    {
        base.Awake();
        m_Shootcomp = GetComponentInChildren<ShootComponent>();

        if (GameObject.Find("LevelStartPoint") != null)
        {
            GameObject obj = GameObject.Find("LevelStartPoint");
            SpawnPoint = obj.transform.position;
            SpawnRot = obj.transform.rotation;
        }
        else
        {
            SpawnPoint = transform.position;
            SpawnRot = transform.rotation;
        }

        m_PlayerAnimator = GetComponent<Animator>();
        if(m_PlayerAnimator != null)
        {
            m_IdleLayerIndex = m_PlayerAnimator.GetLayerIndex("Idle");
            m_ShootingLayerIndex = m_PlayerAnimator.GetLayerIndex("Shooting");
            m_AimLayerIndex = m_PlayerAnimator.GetLayerIndex("Aim");
            m_UnaimLayerIndex = m_PlayerAnimator.GetLayerIndex("Unaim");
            m_DeathLayerIndex = m_PlayerAnimator.GetLayerIndex("Death");
        }

        LoadArmMaterial();

        //listen for if a Cutscene starts or finishes
        Listen("CutsceneStart", DisableDuringCutscene);
        Listen("CutsceneFinsished", EnableAfterCutscene);
        Listen("CurrentPlayerArm", LoadArmMaterial);
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

    }

    private void LoadArmMaterial()
    {
        GameObject obj = transform.Find("Powerplant_Model_Character_Player_1").gameObject;
        int val = PlayerPrefs.GetInt("CurrentPlayerArm", 0);

        if (obj)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

            if (renderer)
            {
                renderer.material = ArmSkins[val];
            }

            MeshRenderer[] objs = obj.GetComponentsInChildren<MeshRenderer>();

            foreach (var mesh in objs)
            {
                if(mesh.gameObject.name != "Armgun_Screen")
                    mesh.material = ArmSkins[val];
            }
        }

    }

    private void FixedUpdate()
    {
        if(InitialFireInput == false)
        {
            if (m_InputComponent.IsFiring() && m_Shootcomp)
            {
                
                AimAnim();
            }
        }
    }

    public override void Respawn(bool forced = false)
    {
        base.Respawn();

        InitialFireInput = false;

        if (m_HealthComp.IsAlive() == false)
        {
            EventManager.TriggerEvent("OnPlayerRespawnEvent");
        }

        //set death layer weight to zero
        m_PlayerAnimator.SetLayerWeight(m_DeathLayerIndex, 0.0f);

        transform.position = SpawnPoint;
        transform.rotation = SpawnRot;
        Camera.main.GetComponent<CameraDriver>().SetFacingDirection(SpawnRot.eulerAngles);

        PlayerStateMachine PSM = GetComponent<PlayerStateMachine>();

        if (PSM.CurrentState.Is<StateCrouching>())
        {
            PSM.SetMovementState<StateOnGround>();
            
        }
        //m_InputComponent.
          
    }

    public void SetRespawnPoint(RespawnPoint respawnPoint)
    {
        //if we already have a RespawnPoint, disable it so we can swap to the new one
        if (m_RespawnPoint)
        {
            m_RespawnPoint.SetInactive();
            m_RespawnPoint = null;
        }
        //set new RespawnPoint
        m_RespawnPoint = respawnPoint;
        //Set variables to new RespawnPoint
        respawnPoint.SetRespawnPoint(ref SpawnPoint, ref SpawnRot);

        //trigger a checkpoint event
        EventManager.TriggerEvent("OnCheckPointReached", new OnCheckpointReachedParam(respawnPoint.RespawnOrderInLevel, WorldTimeManager.TimePassed));
    }

    protected override void OnSoftReset()
    {
        Respawn(true);
    }

     
    public void AimAnim()
    {
        CancelIdleAnim();
        m_Shootcomp.Shoot();

        InitialFireInput = true;
       if (m_PlayerAnimator != null)
       {
            CancelIdleAnim();
            m_AimLayerWeight = Mathf.Lerp(m_AimLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_AimLayerIndex, m_AimLayerWeight);
            m_PlayerAnimator.Play("Aim", m_AimLayerIndex);
       }
    }
    
    public void FireAnim()
    {
        CancelIdleAnim();

        if (m_PlayerAnimator != null)
        {
            m_ShootingLayerWeight = Mathf.Lerp(m_ShootingLayerWeight, 1.0f, 1);
            m_PlayerAnimator.SetLayerWeight(m_ShootingLayerIndex, m_ShootingLayerWeight);
            m_PlayerAnimator.Play("Shooting", m_ShootingLayerIndex);
        }
    }
    
    IEnumerator TakeArmDown()
    {
        CancelIdleAnim();

        yield return new WaitForSeconds(0.5f);
        m_PlayerAnimator.SetLayerWeight(m_ShootingLayerIndex, 0.0f);
        m_PlayerAnimator.SetLayerWeight(m_AimLayerIndex, 0.0f);
        m_UnaimLayerWeight = Mathf.Lerp(m_UnaimLayerWeight, 1.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_UnaimLayerIndex, m_UnaimLayerWeight);
        m_PlayerAnimator.Play("Unaim", m_UnaimLayerIndex);
        yield return new WaitForSeconds(0.5f);
        m_UnaimLayerWeight = Mathf.Lerp(m_UnaimLayerWeight, 0.0f, 1);
        m_PlayerAnimator.SetLayerWeight(m_UnaimLayerIndex, m_UnaimLayerWeight);
        InitialFireInput = false;
        GoIntoIdleAnimations();
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
        }
    }

    //called by animation event in Shooting animation when bullet needs to be fired
    public void ShootKeyFrame()
    {
        m_Shootcomp.Shoot();
    }
    //called by animation event at the end of shooting animation to check if it needs to shoot again or not 
    public void CheckInputKeyFrame()
    {
        if(m_InputComponent.IsFiring() && m_Shootcomp)
        {
            FireAnim();
        }
        else
        {
            //take arm down anim here
            StartCoroutine(TakeArmDown());
        }
    }

    void DisableDuringCutscene()
    {
        //disables player duting cutscenes
        this.gameObject.SetActive(false);
    }

    void EnableAfterCutscene()
    {
        //enables player when cutscene is skipped or finishes
        this.gameObject.SetActive(true);
    }

}
