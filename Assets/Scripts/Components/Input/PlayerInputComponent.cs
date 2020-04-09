using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XInputDotNetPure;

public class PlayerInputComponent : InputComponentBase
{
    bool m_EnableMouseControl;

    //Settings
    [Header("Input Toggles")]
    public bool UseToggleSprint = false;
    public bool UseToggleCrouch = false;

    private bool m_SprintToggle = false;
    private bool m_CrouchToggle = false;
    private bool m_IsJumpQueued = false;

    private const float m_QueueTimeLength = 0.2f;
    private float m_QueueTimeout = m_QueueTimeLength;

    eLastInputType m_LastInputType;

    public override void Init(Character character)
    {
        //setup the ControlCamera variable
    }

    protected override void Awake()
    {
        base.Awake();

        Listen("OnToggleCrouch", OnToggleCrouch);
        Listen("OnToggleSprint", OnToggleSprint);

        LoadFromSettings();
    }

    /// <summary>
    /// Meant to load the settings the player has made from playerprefs
    /// </summary>
    private void LoadFromSettings()
    {
        if (PlayerPrefs.HasKey("OnToggleCrouch"))
            OnToggleCrouch();

        if (PlayerPrefs.HasKey("OnToggleSprint"))
            OnToggleSprint();
    }

    public void OnToggleCrouch()
    {
        UseToggleCrouch = Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleCrouch"));
    }

    public void OnToggleSprint()
    {
        UseToggleSprint = Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleSprint"));
    }

    private void Start()
    {
        LockToViewport();
    }

    //check for toggles and whatnot
    private void Update()
    {
        UpdateMouseControlToggle();
        UpdateInputType();

        CheckCrouchToggle();
        CheckSprintToggle();
        HandleJumpQueue();

        CheckJumpPress();
        CheckJumpRelease();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote)) //same button
        {
            DevConsole.ToggleConsole();
        }

        if(Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            WorldTimeManager.AddTime(10.0f);
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            WorldTimeManager.RemoveTime(10.0f);
        }
#endif

        if (IsResetting() && !PauseManager.IsPaused())
        {
            //SQLManager.ResetAllLeaderboards();

            EventManager.TriggerEvent("OnSoftReset");

            //string name = string.Format("{0}{1}{2}", 
            //    Convert.ToChar(UnityEngine.Random.Range(65, 91)),
            //    Convert.ToChar(UnityEngine.Random.Range(65, 91)),
            //    Convert.ToChar(UnityEngine.Random.Range(65, 91)));

            //FirebaseManager.SaveHighScore(name, UnityEngine.Random.Range(0.0f, 1000.0f), UnityEngine.Random.Range(1, 3));
            //FirebaseManager.GetLeaderboardAsync().Wait();
        }
    }

    public override void UpdateControls()
    {
    }

    private void UpdateInputType()
    {
        m_LastInputType = CheckLastInputType.GetLastInputType();
    }

    public override void SetFacingDirection(Vector3 direction)
    {
    }

    public override Vector3 GetControlRotation()
    {
        if (m_Disabled)
        {
            return Vector3.zero;
        }

        return Vector3.zero;
    }

    //returns a vector with values -1 to 1 based on input. 
    //If you want the vector relevant to the object's forward use GetRelativeMoveInput()
    public override Vector3 GetMoveInput()
    {
        if (m_Disabled)
        {
            return Vector3.zero;
        }

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        input = Vector3.ClampMagnitude(input, 1.0f);

        return input;
    }

    public float GetHorizantalMovement()
    {
        return Input.GetAxis("Horizontal");
    }

    public float GetVerticalMovement()
    {
        return Input.GetAxis("Vertical");
    }

    //Same as GetMoveInput but gives you the result relevant to local space
    public override Vector3 GetRelativeMoveInput()
    {
        if (m_Disabled)
        {
            return Vector3.zero;
        }

        Vector3 inputVector = GetMoveInput();

        Vector3 moveDirection = transform.forward * inputVector.z + transform.right * inputVector.x;

        return moveDirection;
    }

    //returns a vector with values -1 to 1 based on input
    public override Vector3 GetLookInput()
    {
        if (m_Disabled || !m_EnableMouseControl)
        {
            return Vector3.zero;
        }

        return new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0.0f);
    }

    //eventually ray cast to get whatever object we're aiming at
    public override Vector3 GetAimTarget()
    {
        return Vector3.zero;
    }

    private void HandleJumpQueue()
    {
        if (m_Disabled)
        {
            return;
        }

        //if (Input.GetButtonDown("Jump"))
        if (KeyBindingManager.GetButtonDown(KeyAction.jump))
        {
            m_IsJumpQueued = true;
        }

        //run a timer so we don't have horribly stored input
        if (m_IsJumpQueued)
        {
            m_QueueTimeout -= Time.deltaTime;

            if (m_QueueTimeout <= 0.0f)
            {
                m_IsJumpQueued = false;
                m_QueueTimeout = m_QueueTimeLength;
            }
        }
        else
        {
            m_QueueTimeout = m_QueueTimeLength;
        }
    }

    private void CheckCrouchToggle()
    {
        if (UseToggleCrouch)
        {
            //if (Input.GetButtonDown("Crouch"))
            if (KeyBindingManager.GetButtonDown(KeyAction.crouch))
            {
                m_CrouchToggle = !m_CrouchToggle;
            }
        }
    }

    private void CheckSprintToggle()
    {
        if (UseToggleSprint)
        {
            //if (Input.GetButtonDown("Sprint"))
            if (KeyBindingManager.GetButtonDown(KeyAction.sprint))
            {
                m_SprintToggle = !m_SprintToggle;
            }
        }
    }

    public override bool IsJumping()
    {
        if (m_Disabled)
        {
            return false;
        }

        //I would just return IsJumpQueued but I need to turn it back off first
        if (m_IsJumpQueued)
        {
            m_IsJumpQueued = false;
            return true;
        }

        return false;
    }

    public void CheckJumpRelease()
    {
        //if (Input.GetButtonUp("Jump"))
        if (KeyBindingManager.GetButtonUp(KeyAction.jump))
        {
            EventManager.TriggerEvent("OnPlayerJumpReleased");
        }
    }

    public void CheckJumpPress()
    {
        //if (Input.GetButtonDown("Jump"))
        if (KeyBindingManager.GetButtonDown(KeyAction.jump))
        {
            EventManager.TriggerEvent("OnPlayerJumpPressed");
        }
    }

    public override bool IsFiring()
    {
        if (m_Disabled)
        {
            return false;
        }

        if (!m_EnableMouseControl)
        {
            return false;
        }

        //if controller return controller's fire
        if (m_LastInputType == eLastInputType.Gamepad)
        {
            //Keybindingmanager has no way to check the trigger so we still have to rely on Input.GetAxis
            return Input.GetAxis("Fire") > 0.0f || KeyBindingManager.GetKey(KeyAction.shoot);
        }

        //else return the keyboard's fire
        //return Input.GetButton("Fire");
        return KeyBindingManager.GetKey(KeyAction.shoot);
    }

    //returns the state of the toggle if toggle is on, otherwise returns the state of the sprint button(held)
    public bool IsSprinting()
    {
        if (m_Disabled)
        {
            return false;
        }

        if (UseToggleSprint)
        {
            return m_SprintToggle;
        }

        //return Input.GetButton("Sprint");
        return KeyBindingManager.GetKey(KeyAction.sprint);
    }

    public override bool IsAiming()
    {
        if (m_Disabled)
        {
            return false;
        }

        return false;
    }

    public override bool IsCrouching()
    {
        if (m_Disabled)
        {
            return false;
        }

        if (UseToggleCrouch)
        {
            return m_CrouchToggle;
        }

        //return Input.GetButton("Crouch");
        return KeyBindingManager.GetKey(KeyAction.crouch);
    }

    public bool IsInspecting()
    {
        return Input.GetButton("Inspect");
    }

    void UpdateMouseControlToggle()
    {
        if(m_Disabled)
        {
            return;
        }

        //Check for a mouse click to lock and enable mouse control
        //GUIUtility.hotControl will be non-zero if a UI element was clicked.  If this is the case ignore the input.
        if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        //Set enable mouse control here.  This can change outside of our control so we'll check it every frame.
        m_EnableMouseControl = Cursor.lockState == CursorLockMode.Locked;

        Cursor.visible = !m_EnableMouseControl;
    }

    public override void SetEnabled()
    {
        base.SetEnabled();

        LockToViewport();
    }

    public override void ToggleInput()
    {
        base.ToggleInput();

        if(!m_Disabled)
        {
            LockToViewport();
        }
    }

    private void LockToViewport()
    {
        m_LastInputType = CheckLastInputType.GetLastInputType();

        Cursor.lockState = CursorLockMode.Locked;

        //Set enable mouse control here.  This can change outside of our control so we'll check it every frame.
        m_EnableMouseControl = Cursor.lockState == CursorLockMode.Locked;

        Cursor.visible = !m_EnableMouseControl;
    }

    public bool IsResetting()
    {
        //return Input.GetButtonDown("Reset");
        return KeyBindingManager.GetButtonDown(KeyAction.reset);
    }

    protected override void OnSoftReset()
    {
        m_CrouchToggle = false;
        m_SprintToggle = false;
        m_IsJumpQueued = false;
    }
}
