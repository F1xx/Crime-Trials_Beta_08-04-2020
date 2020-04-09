/*
    - Contains logic pertaining to an FPS camera controlled specifically by a player (from input component)
*/

using UnityEngine;
using UnityEngine.Events;

public class FirstPersonBehaviour : PlayerControlledBehaviour
{
    #region -- Speed Modifiers/VFX --

    private float m_LocalSpeedX = 0.0f;
    private float m_LocalSpeedZ = 0.0f;

    private float m_Speed = 0.0f;

    private bool m_PlayerDamaged = false;

    private Material m_RadialBlurMat = null;

    private GameObject m_SpeedLineMaxParticle = null;
    private GameObject m_SpeedLineCrazyParticle = null;

    private TweenableFloat m_RadialBlur = new TweenableFloat();
    private TweenableFloat m_Vignette = new TweenableFloat();

    #endregion

    #region -- Input Variables --

    // Axes declared as tweenable floats for the tween functions
    // C# cannot store references to variables outside of local context
    private float m_RawPitch = 0.0f;
    private TweenableFloat m_ClampedPitch = new TweenableFloat(0.0f);
    private float m_RawYaw = 0.0f;
    private TweenableFloat m_ClampedYaw = new TweenableFloat(0.0f);
    private TweenableFloat m_ClampedRoll = new TweenableFloat(0.0f); // never getting raw, unclamped, input for roll

    #endregion

    #region -- FOV Tween Variables -- 

    // Camera FOV tween object
    private Tween m_CameraFOVTween = null;

    private eTweenFunc m_FOVTweenType = eTweenFunc.LinearToTarget;

    // Value that actually stores the tween
    private TweenableFloat m_FOV = new TweenableFloat(0.0f);
    private PlayerController m_TargetToFollowController = null; // using this to get velocity of player for tweening    

    //private Vector3 m_LastWallHit = Vector3.zero;

    private StateBase m_CurrentState = null;

    #endregion

    #region -- Wallrunning Variables --

    // Wallrunning Tweens and variables
    private Tween m_WallRunEnterTween = null;
    private Tween m_WallRunExitTween = null;
    private Tween m_WallRunClampTween = null;

    public eTweenFunc m_WallRunClampTweenType = eTweenFunc.LinearToTarget;
    public eTweenFunc m_RollClampTweenType = eTweenFunc.LinearToTarget;

    // Start and offsets for clamping while wallrunning
    private float m_AngleLockStart = 0.0f;

    // Keep track of what space we're currently in
    private bool m_In_0_360_Space = true;

    #endregion

    public override void Activate()
    {
        base.Activate();

        // Initialize all variables
        if (m_CameraSettings != null)
        {
            Init();
        }

        SubscribeToTargetState(); // subscribe to event changes 
    }

    public override void Deactivate()
    {
        UnsubscribeToTargetState(); // unsub from event changes
    }

    /// <summary>
    ///     Set facing direction as passed in Vector3
    /// </summary>
    public override void SetFacingDirection(Vector3 facingDirection)
    {
        m_ClampedPitch.Value = facingDirection.x;
        m_ClampedYaw.Value = facingDirection.y;
        m_ClampedRoll.Value = facingDirection.z;
    }

    public override void OnPlayerDamage()
    {
        m_PlayerDamaged = true;
    }

    protected override void SubscribeToTargetState()
    {
        if (m_StateMachine != null)
        {
            m_StateMachine.SubscribeToStateChange(TargetStateChanges);
        }
    }

    protected override void UnsubscribeToTargetState()
    {
        if (m_StateMachine != null)
        {
            m_StateMachine.UnsubscribeToStateChange(TargetStateChanges);
        }
    }

    /// <summary>
    ///     Handles what happens on StateChange
    /// </summary>
    protected override void TargetStateChanges(StateChangeStruct stateStruct) // a delegate is a function pointer
    {
        m_CurrentState = stateStruct.CurrentState;
        float tweenExitTime = m_CameraSettings.RollTweenDuration;

        // Clean the tweens once they aren't in use
        CleanTweens(stateStruct, ref tweenExitTime);

        #region -- Tweening and Clamping Camera While WallRunning --

        if (m_CurrentState.Is<StateWallRunning>())
        {
            StateWallRunning state = (StateWallRunning)m_CurrentState;

            // Set the starting lock angle to be the player's movement direction angle on a wall
            m_AngleLockStart = GetPlayerMoveDirection(m_CurrentState);

            // Avoid rollover from 0/360 by changing spaces
            if (m_AngleLockStart <= 90 || m_AngleLockStart >= 270)
            {
                m_In_0_360_Space = false;

                // Angles are now in -180/180 space
                m_AngleLockStart = WrapAngle(m_AngleLockStart);
                m_ClampedYaw.Value = WrapAngle(m_ClampedYaw.Value);
            }
            // Avoid rollover from -180/180 by changing spaces
            else
            {
                m_In_0_360_Space = true;

                // Angles are in 0/360 space
                m_AngleLockStart = UnwrapAngle(m_AngleLockStart);
                m_ClampedYaw.Value = UnwrapAngle(m_ClampedYaw.Value);
            }

            if (state.IsRunningOnRightSide)
            {
                // Create the roll tween
                CreateTween(ref m_WallRunEnterTween, m_ClampedRoll, m_CameraSettings.WallRunningRollAngle, m_CameraSettings.RollTweenDuration, m_RollClampTweenType);

                // Tween player's mouse to be in bounds
                if (m_ClampedYaw.Value > m_AngleLockStart + m_CameraSettings.AngleLockOffsetTowardsWall)
                {
                    CreateTween
                        (ref m_WallRunClampTween,
                        m_ClampedYaw,
                        (m_AngleLockStart - m_CameraSettings.AngleLockOffsetAwayWall),
                        m_CameraSettings.WallRunningClampTweenDuration,
                        m_WallRunClampTweenType);
                }
                else if (m_ClampedYaw.Value < m_AngleLockStart - m_CameraSettings.AngleLockOffsetAwayWall)
                {
                    CreateTween
                        (ref m_WallRunClampTween,
                        m_ClampedYaw,
                        (m_AngleLockStart + m_CameraSettings.AngleLockOffsetTowardsWall),
                        m_CameraSettings.WallRunningClampTweenDuration,
                        m_WallRunClampTweenType);
                }
            }
            else
            {
                // Create the roll tween
                CreateTween(ref m_WallRunEnterTween, m_ClampedRoll, -m_CameraSettings.WallRunningRollAngle, m_CameraSettings.RollTweenDuration, m_RollClampTweenType);

                // Tween player's mouse to be in bounds
                if (m_ClampedYaw.Value < m_AngleLockStart - m_CameraSettings.AngleLockOffsetTowardsWall)
                {
                    CreateTween
                        (ref m_WallRunClampTween,
                        m_ClampedYaw,
                        (m_AngleLockStart + m_CameraSettings.AngleLockOffsetAwayWall),
                        m_CameraSettings.WallRunningClampTweenDuration,
                        m_WallRunClampTweenType);
                }
                else if (m_ClampedYaw.Value > m_AngleLockStart + m_CameraSettings.AngleLockOffsetAwayWall)
                {
                    CreateTween
                        (ref m_WallRunClampTween,
                        m_ClampedYaw,
                        (m_AngleLockStart - m_CameraSettings.AngleLockOffsetTowardsWall),
                        m_CameraSettings.WallRunningClampTweenDuration,
                        m_WallRunClampTweenType);
                }
            }
        }

        #endregion

        // If previous state was wall running state tween back the roll
        if (stateStruct.PreviousState.Is<StateWallRunning>())
        {
            // If entering second wallrunning state then make sure to complete tween 2 if exists
            CreateTween(ref m_WallRunExitTween, m_ClampedRoll, 0.0f, tweenExitTime, m_RollClampTweenType);
        }
    }

    public override void UpdateCamera()
    {
        if (m_Camera != null)
        {
            // Get the current speed on the X and Z of the target ot follow
            m_LocalSpeedX = Mathf.Abs(m_TargetToFollow.transform.InverseTransformDirection(m_TargetToFollowController.Velocity).x);
            m_LocalSpeedZ = Mathf.Abs(m_TargetToFollow.transform.InverseTransformDirection(m_TargetToFollowController.Velocity).z);

            m_Speed = m_LocalSpeedX + m_LocalSpeedZ;

            // Set the camera to the position of the PivotPoint aka the head
            m_Camera.transform.position = m_PivotPoint.transform.position;

            UpdateInput(ref m_RawPitch, ref m_RawYaw);

            m_ClampedPitch.Value += m_RawPitch;
            m_ClampedYaw.Value += m_RawYaw;

            ClampAxis(ref m_ClampedPitch.Value, ref m_RawPitch, -75.0f, 75.0f);

            if (m_CurrentState != null)
            {
                // Prevent sudden camera movement by staying in the same space we were in while wallrunning after exiting
                if (!m_CurrentState.Is<StateWallRunning>())
                {
                    if (m_In_0_360_Space == true)
                    {
                        if (m_ClampedYaw.Value > 360)
                        {
                            m_ClampedYaw.Value = 0;
                        }
                        else if (m_ClampedYaw.Value < 0)
                        {
                            m_ClampedYaw.Value = 360;
                        }
                    }
                    else
                    {
                        if (m_ClampedYaw.Value > 180)
                        {
                            m_ClampedYaw.Value = -180;
                        }
                        else if (m_ClampedYaw.Value < -180)
                        {
                            m_ClampedYaw.Value = 180;
                        }
                    }
                }
            }

            // This code is a little bit of a mess :^/
            HandleStates();

            // Update head and arm based on camera's new position
            UpdateArm();

            Vector3 rotation = Vector3.zero;
            if (m_CameraSettings.IsInvertedX)
            {
                rotation = new Vector3(m_ClampedPitch.Value, m_ClampedYaw.Value, m_ClampedRoll.Value);
            }
            else
            {
                rotation = new Vector3(-m_ClampedPitch.Value, m_ClampedYaw.Value, m_ClampedRoll.Value);
            }
            m_Camera.transform.rotation = Quaternion.Euler(rotation);
        }
    }

    /// <summary>
    ///     Update the position of the arm
    /// </summary>
    private void UpdateArm()
    {
        // Rotate the arm based on the camera
        Vector3 armRotation = m_CameraSettings.ArmToRotate.localEulerAngles;
        //Vector3 armRotation = m_Camera.transform.localEulerAngles;
        //armRotation.z += -90.0f;
        //armRotation.x = 0.0f;
        armRotation.y += -90.0f;

        // Set arm to camera rotation
        if (m_CameraSettings.ArmToRotate != null)
        {
           // Ray ray = Camera.main.ViewportPointToRay(PlayerCrosshair.AimLocation.pivot);
           // m_CameraSettings.ArmToRotate.LookAt(ray.GetPoint(5.0f), Vector3.up);
           // m_CameraSettings.ArmToRotate.rotation = Quaternion.Euler(armRotation);

            if (m_CameraSettings.IsInvertedX)
            {
                armRotation = new Vector3(0.0f, m_ClampedYaw.Value - 90.0f, -m_ClampedPitch.Value - 90.0f);
            }
            else
            {
                armRotation = new Vector3(0.0f, m_ClampedYaw.Value - 90.0f, m_ClampedPitch.Value - 90.0f);
            }
            m_CameraSettings.ArmToRotate.rotation = Quaternion.Euler(armRotation);
        }
        else
        {
            //m_CameraSettings.ArmToRotate = GameObject.Find("Armgun_UpperArm").transform;
            m_CameraSettings.ArmToRotate = GameObject.Find("MarineRibcage").transform;
            Ray ray = Camera.main.ViewportPointToRay(PlayerCrosshair.AimLocation.pivot);
            m_CameraSettings.ArmToRotate.LookAt(ray.GetPoint(5.0f), Vector3.up);
            m_CameraSettings.ArmToRotate.rotation = Quaternion.Euler(armRotation);

        }
    }

    /// <summary>
    ///     Handles how the camera acts during each state
    /// </summary>
    private void HandleStates()
    {
        if (m_CurrentState != null)
        {
            if (m_CurrentState.Is<StateWallRunning>())
            {
                StateWallRunning state = (StateWallRunning)m_CurrentState;

                // Grab hit object
                RaycastHit WallObjectHit = state.AttachedWallObject;

                // Create our direction vector by crossing our collision normal of wall with the up axis
                Vector3 moveDirection = Vector3.Cross(Vector3.up, WallObjectHit.normal);
                moveDirection = state.IsRunningOnRightSide ? moveDirection : -moveDirection;
                float yRotation = Quaternion.LookRotation(moveDirection).eulerAngles.y;
                m_AngleLockStart = yRotation;

                if (m_In_0_360_Space == false)
                {
                    // Angles to -180/180 space
                    m_AngleLockStart = WrapAngle(m_AngleLockStart);
                    m_ClampedYaw.Value = WrapAngle(m_ClampedYaw.Value);
                }
                else
                {
                    // Angles to 0/360 space
                    m_AngleLockStart = UnwrapAngle(m_AngleLockStart);
                    m_ClampedYaw.Value = UnwrapAngle(m_ClampedYaw.Value);
                }

                if (m_CameraSettings.IsInvertedY)
                {
                    m_ClampedYaw.Value = Mathf.Abs(m_ClampedYaw.Value);
                }

                // Stop tween if in bounds then clamp camera depending on which side we're running on
                if (state.IsRunningOnRightSide == true)
                {
                    if (m_WallRunClampTween != null && m_ClampedYaw.Value > m_AngleLockStart - m_CameraSettings.AngleLockOffsetAwayWall && m_ClampedYaw.Value < m_AngleLockStart + m_CameraSettings.AngleLockOffsetTowardsWall)
                    {
                        m_WallRunClampTween.StopTweening(Tween.eExitMode.IncompleteTweening);
                        m_WallRunClampTween = null;
                    }

                    if (m_WallRunClampTween == null)
                    {
                        ClampAxis
                            (ref m_ClampedYaw.Value,
                            ref m_RawYaw,
                            m_AngleLockStart - m_CameraSettings.AngleLockOffsetAwayWall,
                            m_AngleLockStart + m_CameraSettings.AngleLockOffsetTowardsWall);
                    }
                }
                else
                {
                    if (m_WallRunClampTween != null && m_ClampedYaw.Value < m_AngleLockStart + m_CameraSettings.AngleLockOffsetAwayWall && m_ClampedYaw.Value > m_AngleLockStart - m_CameraSettings.AngleLockOffsetTowardsWall)
                    {
                        m_WallRunClampTween.StopTweening(Tween.eExitMode.IncompleteTweening);
                        m_WallRunClampTween = null;
                    }

                    if (m_WallRunClampTween == null)
                    {
                        ClampAxis
                            (ref m_ClampedYaw.Value,
                            ref m_RawYaw,
                            m_AngleLockStart - m_CameraSettings.AngleLockOffsetTowardsWall,
                            m_AngleLockStart + m_CameraSettings.AngleLockOffsetAwayWall - 20);
                        // TODO: don't know why - 20 fixes bug, something with the yaw going over to 270 and it not liking it, something with the space conversion probably
                    }
                }
            }

            if (m_CameraSettings.SpeedVision == true)
            {
                SpeedVision();
            }

            Camera.main.fieldOfView = m_FOV.Value;

            // Tween back the camera when state isn't wallrunning
            if (!m_CurrentState.Is<StateWallRunning>())
            {
                if (m_WallRunClampTween != null)
                {
                    m_WallRunClampTween.StopTweening(Tween.eExitMode.IncompleteTweening);
                    m_WallRunClampTween = null;
                }

                if (m_CameraSettings.IsInvertedY)
                {
                    m_TargetToFollow.transform.rotation = Quaternion.Euler(new Vector3(0.0f, -m_ClampedYaw.Value, 0.0f));
                }
                else
                {
                    m_TargetToFollow.transform.rotation = Quaternion.Euler(new Vector3(0.0f, m_ClampedYaw.Value, 0.0f));
                }

                // Only allow the GameObject to rotate on the y (yaw) axis
                m_TargetToFollow.transform.rotation = Quaternion.Euler(new Vector3(0.0f, m_ClampedYaw.Value, 0.0f));
            }
        }
    }

    /// <summary>
    ///     Handles values for speed vision
    /// </summary>
    private void SpeedVision()
    {
        // Change VFX intensity based on speed
        if (m_Speed >= m_CameraSettings.FOVMaxSpeedThreshold)
        {
            // FOV tween
            CreateTween(ref m_CameraFOVTween, m_FOV, m_CameraSettings.StartFOV + m_CameraSettings.FOVMaxSpeedOffset, m_CameraSettings.FOVMaxSpeedTweenDuration, m_FOVTweenType);

            // Post Effects
            CreateTween(ref m_CameraFOVTween, m_Vignette, 0.15f, m_CameraSettings.FOVMaxSpeedTweenDuration, m_FOVTweenType);
            CreateTween(ref m_CameraFOVTween, m_RadialBlur, 0.65f, m_CameraSettings.FOVMaxSpeedTweenDuration, m_FOVTweenType);

            // Speed line particles
            m_SpeedLineCrazyParticle.SetActive(true);
            m_SpeedLineMaxParticle.SetActive(false);
        }
        else if (m_Speed >= m_CameraSettings.FOVMidSpeedThreshold)
        {
            // FOV tween
            CreateTween(ref m_CameraFOVTween, m_FOV, m_CameraSettings.StartFOV + m_CameraSettings.FOVMidSpeedOffset, m_CameraSettings.FOVMidSpeedTweenDuration, m_FOVTweenType);

            // Post Effects
            CreateTween(ref m_CameraFOVTween, m_Vignette, 0.1f, m_CameraSettings.FOVMidSpeedTweenDuration, m_FOVTweenType);
            CreateTween(ref m_CameraFOVTween, m_RadialBlur, 0.0f, m_CameraSettings.FOVMinSpeedTweenDuration, m_FOVTweenType);

            // Speed line particles
            m_SpeedLineCrazyParticle.SetActive(false);
            m_SpeedLineMaxParticle.SetActive(true);
        }
        else
        {
            // FOV Tween
            CreateTween(ref m_CameraFOVTween, m_FOV, m_CameraSettings.StartFOV, m_CameraSettings.FOVMinSpeedTweenDuration, m_FOVTweenType);

            // Post Effects
            CreateTween(ref m_CameraFOVTween, m_Vignette, 0.0f, m_CameraSettings.FOVMinSpeedTweenDuration, m_FOVTweenType);
            CreateTween(ref m_CameraFOVTween, m_RadialBlur, 0.0f, m_CameraSettings.FOVMinSpeedTweenDuration, m_FOVTweenType);

            m_SpeedLineCrazyParticle.SetActive(false);
            m_SpeedLineMaxParticle.SetActive(false);
        }
       
        if (m_RadialBlurMat != null)
        {
            m_RadialBlurMat.SetFloat("_BlurAmount", m_RadialBlur.Value);
        }

        // Red vignette changes when hurt
        if (m_PlayerDamaged)
        {
            m_CameraSettings.VignetteColor = Color.red;
            m_CameraSettings.Vignette = Mathf.Lerp(m_CameraSettings.Vignette, 0.4f, Time.deltaTime * 1.5f);

            if (m_CameraSettings.Vignette > 0.3f)
            {
                m_PlayerDamaged = false;
            }
        }
        else 
        {
            m_CameraSettings.VignetteColor = Color.black;
            m_CameraSettings.Vignette = m_Vignette.Value;
        }
    }

    /// <summary>
    ///     Cleans the wallrunning tweens when exiting
    /// </summary>
    private void CleanTweens(StateChangeStruct stateStruct, ref float tweenExitTime)
    {
        // If the previous state was wallrunning change
        if (m_WallRunEnterTween != null && stateStruct.PreviousState.Is<StateWallRunning>())
        {
            // Did the tween exit cleanly? if no, tween was incomplete
            if (m_WallRunEnterTween.HasTimeRemaining())
            {
                tweenExitTime = m_WallRunEnterTween.TotalTime;
                m_WallRunEnterTween.StopTweening(Tween.eExitMode.IncompleteTweening);
            }

            m_WallRunEnterTween = null;
        }

        if (m_WallRunExitTween != null && m_CurrentState.Is<StateWallRunning>())
        {
            // Complete tween back if it was interupted
            if (m_WallRunExitTween.HasTimeRemaining())
            {
                m_WallRunExitTween.StopTweening(Tween.eExitMode.CompleteTweening);
            }

            m_WallRunExitTween = null;
        }
    }

    /// <summary>
    ///     Function for creating tweens
    /// </summary>
    private void CreateTween(ref Tween tween, TweenableFloat startFloat, float endFloat, float duration, eTweenFunc tweenType)
    {
        tween = TweenManager.CreateTween(startFloat, endFloat, duration, tweenType, null);
    }

    /// <summary>
    ///     Initialize member variables
    /// </summary>
    private void Init()
    {
        // Check to make sure player exists before setting the arm and player controller
        if (m_TargetToFollow != null)
        {
            m_TargetToFollowController = m_TargetToFollow.GetComponent<PlayerController>();
        }

        m_FOV.Value = m_CameraSettings.StartFOV;

        m_RadialBlurMat = Resources.Load<Material>("Radial Blur");

        m_RadialBlur.Value = 0.0f;
        m_Vignette.Value = 0.0f;

        #region -- Speed Line Variables --

        // Particle system to use at max speed
        m_SpeedLineCrazyParticle = GameObject.Instantiate(PrefabManager.GetPrefab("SpeedLineCrazyParticle"), m_PivotPoint, false);
        m_SpeedLineCrazyParticle.SetActive(false);

        m_SpeedLineMaxParticle = GameObject.Instantiate(PrefabManager.GetPrefab("SpeedLineMaxParticle"), m_PivotPoint, false);
        m_SpeedLineMaxParticle.SetActive(false);

        #endregion
    }
}
