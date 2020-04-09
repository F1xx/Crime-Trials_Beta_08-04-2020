using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : BaseObject
{
    [Header("Gravity")]
    public bool UseGravity = true;
    public float MaxTimeInAirBeforeGravCap = 2.0f;

    /// <summary>
    /// The current Velocity of the CharacterController
    /// </summary>
    public Vector3 Velocity { get { return m_Controller.velocity; } }
    /// <summary>
    /// Used to get the Character controller from the PlayerController
    /// </summary>
    public CharacterController CharCont { get { return m_Controller; } }
    float CharacterControllerAdjustment = 0.0f;

    public float CrouchModelScaleFactor = 0.5f;

    /// <summary>
    /// Gravity scale is used to alter how much gravity affects the character during runtime. 1.0 for full gravity or 0.0 for no gravity.
    /// </summary>
    public float GravityScale { get { return _GravityScale; } set { _GravityScale = Mathf.Clamp(value, 0.0f, 1.0f); } }
    private float _GravityScale = 1.0f;

    /// <summary>
    /// This value scales how much time factors the scaling of gravity when delta is applied during FixedUpdate
    /// </summary>
    [SerializeField, Range(1.0f, 10.0f)]
    private float GravityTimefactorMult = 1.0f;

    /// <summary>
    /// Default 'realistic' gravity
    /// </summary>
    private Vector3 Gravity = new Vector3(0.0f, -9.8f, 0.0f);

    private CharacterController m_Controller;
    private List<Vector3> m_MovementInputs = new List<Vector3>();
    private bool m_IsInAir = false;
    private float m_TimeInAir = 0.0f;

    [Header("Air Drag"), SerializeField]
    float AirDragX = 1.0f;
    [SerializeField]
    float AirDragZ = 1.0f;

    public float SlideFactorInAir = 1.0f;

    [Header("Impulses")]
    private HashSet<PlayerImpulse> m_ImpulsesToRemove = new HashSet<PlayerImpulse>();
    private HashSet<PlayerImpulse> m_ImpulsesToAdd = new HashSet<PlayerImpulse>();
    private List<PlayerImpulse> m_ActiveImpulses = new List<PlayerImpulse>();

    //private Vector3 m_Impulse = new Vector3(0.0f, 0.0f, 0.0f);
    //private Vector3 m_CurrentImpulse = new Vector3(0.0f, 0.0f, 0.0f);

    //[Header("Slopes")]
    //[SerializeField]
    //private float SlerpValHoriz = 25.0f;
    //[SerializeField]
    //private float SlerpValVert = 15.0f;
    protected override void Awake()
    {
        DevConsole.Instance();
        base.Awake();
        m_Controller = GetComponent<CharacterController>();
        CharacterControllerAdjustment = (CharCont.center.y - CharCont.height * 0.5f);
        Debug.Assert(m_Controller);
    }

    //sums up the inputs and applies movement to the controller
    private void FixedUpdate()
    {
        ApplyGravity();
        //HandleImpulse();
        HandleAllImpulses();

        Vector3 allVecs = SumList();

        if (m_IsInAir)
        {
            allVecs = HandleAirDrag(allVecs);
        }

        m_Controller.Move(allVecs);

        //print(allVecs);

        m_MovementInputs.Clear();

        //print(Velocity.magnitude);
    }

    Vector3 HandleAirDrag(Vector3 summedList)
    {
        summedList.x /= 1 + AirDragX * Time.fixedDeltaTime;
        summedList.z /= 1 + AirDragZ * Time.fixedDeltaTime;

        return summedList;
    }

    /// <summary>
    /// Clean the list of any outdated Impulses during LateUpdate.
    /// </summary>
    private void LateUpdate()
    {
        foreach (var impulse in m_ImpulsesToRemove)
        {
            RemoveImpulse(impulse);
        }
        m_ImpulsesToRemove.Clear();

        foreach (var impulse in m_ImpulsesToAdd)
        {
            m_ActiveImpulses.Add(impulse);
        }
        m_ImpulsesToAdd.Clear();
    }

    private Vector3 SumList()
    {
        Vector3 allVectors = Vector3.zero;

        foreach(Vector3 vec in m_MovementInputs)
        {
            allVectors += vec;
        }

        return allVectors;
    }

    private Vector3 SumList(List<Vector3> listToSum)
    {
        Vector3 allVectors = Vector3.zero;

        foreach (Vector3 vec in listToSum)
        {
            allVectors += vec;
        }

        return allVectors;
    }

    // Apply gravity. Gravity is multiplied by deltaTime twice. This is because gravity should be applied as an acceleration (ms^-2)
    private void ApplyGravity()
    {
        if(UseGravity)
        {
            if(m_IsInAir)
            {
                m_TimeInAir += Time.fixedDeltaTime;

                m_TimeInAir = Mathf.Clamp(m_TimeInAir, 0.0f, MaxTimeInAirBeforeGravCap);
            }

            Vector3 grav = Gravity * GravityScale;

            if (m_TimeInAir > 0.0f)
            {
                grav *= m_TimeInAir;
                grav *= m_TimeInAir;
            }
            if(m_TimeInAir == 0.0f)
            {
                grav *= Time.fixedDeltaTime;
                grav *= Time.fixedDeltaTime;
            }

            grav *= Time.fixedDeltaTime * GravityTimefactorMult;

            m_MovementInputs.Add(grav);
        }
    }

    /// <summary>
    /// Add a direction vector to this controller. All vectors are then summed and used on the next FixedUpdate
    /// </summary>
    /// <param name="input">a vector of input you wish to add. Typically a direction vector but it is not clamped.</param>
    public void AddMoveInput(Vector3 input)
    {
        m_MovementInputs.Add(input);
    }

    /// <summary>
    /// Resets the inair variable which is used to accelerate gravity.
    /// Useful for things like double jumping where you want to reset the acceleration.
    /// </summary>
    public void ResetInAir()
    {
        m_TimeInAir = 0.0f;
    }

    /// <summary>
    /// Scale the sliding impulse down if we enter air state.
    /// </summary>
    private void ScaleSlideImpulseInAir()
    {
        foreach (PlayerImpulse impulse in m_ActiveImpulses)
        {
            if (impulse.ImpulseType == PlayerImpulse.eImpulseType.Slide)
            {
                float duration = impulse.Duration;
                Vector3 impulseVal = impulse.Force * SlideFactorInAir;

                RemoveImpulse(impulse);
                AddImpulse(impulseVal, eTweenFunc.LinearToTarget, duration, PlayerImpulse.eImpulseType.Slide);
                break;
            }
        }
    }

    public void RecieveStateChangeEvent(StateChangeStruct stateEvent)
    {
        if (stateEvent.CurrentState.Is<StateInAir>())
        {
            ScaleSlideImpulseInAir();
        }

        //When ground is entered we use a default basic gravity time and set time in air back to 0.
        if (stateEvent.CurrentState.Is<StateOnGround>())
        {
            m_IsInAir = false;
            ResetInAir();
        }
        //entering either of these states is subject to gravity exponential scaling over time
        else if (stateEvent.CurrentState.Is<StateInAir>() || stateEvent.CurrentState.Is<StateWallRunning>())
        {
            m_IsInAir = true;
        }

        //Wallrunning is still considered 'InAir' but the duration of it needs to be reset so gravity doesn't instantly hammer you
        //when you leave or exit the state.
        if (stateEvent.PreviousState.Is<StateWallRunning>() || stateEvent.CurrentState.Is<StateWallRunning>())
        {
            ResetInAir();
        }
        //To stop the "Wall running Corpse" when we die while wall running
        if(stateEvent.CurrentState.Is<StateDisabled>())
        {
            m_IsInAir = false;
            GravityScale = 1.0f;
        }

        if(stateEvent.CurrentState.Is<StateCrouching>())
        {
            m_IsInAir = false;
            ScaleController(CharCont, CrouchModelScaleFactor);
        
        }
        else if(stateEvent.PreviousState.Is<StateCrouching>())
        {
            ScaleController(CharCont, 1 / CrouchModelScaleFactor);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //print("Collided with: " + hit.transform.root.name);

        if (hit.collider.CompareTag("ElectricWall"))
        {
            //ensure we go to the parent so we're not in some weird nested child
            Electric elec = hit.collider.transform.root.GetComponentInChildren<Electric>();

            if (elec)
            {
                elec.GetPushed(this, hit);
            }
            else
            {
                 Debug.LogError("Could not find Electric Script in " + hit.collider.transform.root.name + ". Specifically Collided with: " + hit.collider.gameObject);
            }
        }
        //else if (hit.collider.CompareTag("DangerLiquid"))
        //{
        //    //slow down?
        //}
        //else if (hit.collider.CompareTag("Spike"))
        //{
        //    //Something
        //}
    }

    void Reset()
    {
        ResetInAir();
        ClearAllImpulses();
        GravityScale = 1.0f;
        m_Controller.SimpleMove(Vector3.zero);
        m_Controller.velocity.Set(0.0f, 0.0f, 0.0f);
        m_MovementInputs.Clear();
    }

    protected override void OnSoftReset()
    {
        Reset();
    }

    protected override void OnHardReset()
    {
        Reset();
    }

    public void ScaleController(CharacterController target, float newScale)
    {
        target.center *= newScale;

        if (newScale < 1)
        {
            target.center = new Vector3(target.center.x, target.center.y + CharacterControllerAdjustment * newScale, target.center.z);
        }
        else if (newScale > 1)
        {
            target.center = new Vector3(target.center.x, target.center.y - CharacterControllerAdjustment, target.center.z);
        }

        target.height *= newScale;
    }

    #region Impulses
    void HandleAllImpulses()
    {
       // List<Vector3> impulses = new List<Vector3>();
        foreach (PlayerImpulse impulse in m_ActiveImpulses)
        {
            //impulses.Add(HandleImpulse(impulse));
            HandleImpulse(impulse);
        }
       // Vector3 allImpulses = SumList(impulses);
    }

    Vector3 HandleImpulse(PlayerImpulse impulse)
    {
        if (impulse.TweenedImpulse.IsPaused() == true)
        {
            QueueImpulseForRemoval(impulse);
            return Vector3.zero;
        }


        //If we are hitting our head we do not want to continue an upwards impulse, Nullify it
        ImpulseCheckCeiling(impulse);

        //add the current impulse as a movement input
        m_MovementInputs.Add(impulse.TweenableForce.Value);

        return impulse.TweenableForce.Value;
    }

    /// <summary>
    /// Zero out the impulse's Y if the player has bumped their head
    /// </summary>
    /// <param name="impulse">The impulse to check</param>
    void ImpulseCheckCeiling(PlayerImpulse impulse)
    {
        //only even check if the impulse is going up
        if (impulse.Force.y > 0.0f)
        {
            RaycastHit hit;
            Vector3 origin = transform.position;
            origin.y += 0.5f;
            float radius = 0.5f;
            float dist = 1.0f;

            if (Physics.SphereCast(origin, radius, Vector3.up, out hit, dist))
            {
                //did it hit a ceiling?
                if (hit.normal.y < 0.0f)
                {
                    impulse.ZeroOutY();
                }
            }
        }
    }

    /// <summary>
    /// Applies a force to the controller over a short time.
    /// </summary>
    /// <param name="impulse">direction and magnitude of the force</param>
    /// <param name="forceMode">direction and magnitude of the force</param>
    public void AddImpulse(Vector3 impulse, eTweenFunc forceMode = eTweenFunc.Linear, float duration = 0.5f, PlayerImpulse.eImpulseType type = PlayerImpulse.eImpulseType.Basic)
    {

        //check if the impulse being added is a slide. we can only have 1 slide at a time.
        if (type == PlayerImpulse.eImpulseType.Slide)
        {
            foreach (PlayerImpulse managedImpulses in m_ActiveImpulses)
            {
                if (managedImpulses.ImpulseType == type)
                {
                    return;
                }
            }

            foreach (PlayerImpulse managedImpulses in m_ImpulsesToAdd)
            {
                if (managedImpulses.ImpulseType == type)
                {
                    return;
                }
            }
        }

        Vector3 thisImpulse = impulse * Time.fixedDeltaTime;

        //add a new playerimpulse
        PlayerImpulse playerImpulse = new PlayerImpulse(thisImpulse, duration, forceMode, type);
        QueueImpulseForAddition(playerImpulse);

        //Forcefully apply this impulse manually this frame.
        m_MovementInputs.Add(thisImpulse);
    }


    void QueueImpulseForRemoval(PlayerImpulse impulse)
    {
        m_ImpulsesToRemove.Add(impulse);

    }

    void QueueImpulseForAddition(PlayerImpulse impulse)
    {
        m_ImpulsesToAdd.Add(impulse);
    }

    /// <summary>
    /// Checks if the impulse is active, if it is it will remove it from itself and call it's dispose method
    /// </summary>
    /// <param name="impulse">the impulse to remove</param>
    private void RemoveImpulse(PlayerImpulse impulse)
    {
        for (int i = 0; i < m_ActiveImpulses.Count; i++)
        {
            if (impulse == m_ActiveImpulses[i])
            {
                impulse.Dispose();
                m_ActiveImpulses.RemoveAt(i);
                return;
            }
        }

        Debug.LogWarning("Impulse that didn't exist in PlayerController attempted to be removed.");
    }

    private void ClearAllImpulses()
    {
        m_ImpulsesToRemove.Clear();

        foreach(PlayerImpulse impulse in m_ActiveImpulses)
        {
            m_ImpulsesToRemove.Add(impulse);
        }
    }
    #endregion
}
