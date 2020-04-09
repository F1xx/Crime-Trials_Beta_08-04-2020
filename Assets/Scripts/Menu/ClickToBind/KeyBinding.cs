using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[System.Serializable]
//[RequireComponent(typeof(SaveKeyBinding))] //used to save data to database
public class KeyBinding : BaseObject, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Keybind Settings"), Tooltip("Set this to the type of control this binding is for: Primary = 1st Keyboard and Mouse button, alt = 2nd. Gamepad = 1st gamepad and gamepadalt = 2nd gamepad.")]
    public KeyBindType keyBindType = KeyBindType.primary;

    [Tooltip("What Action does this key binding map to")]
    public KeyAction keyAction;
	
    [Tooltip("What is the default button that should be used for this action and bind type? Note: None is an option.")]
	public KeyCode keyCode = KeyCode.None;
    //default that is privately stored
    private KeyCode m_DefaultKeyCode = KeyCode.None;

    protected KeyBindButtonVisuals m_VisualsUpdater = null;
    //Text to display keycode for user feedback
    public TMPro.TMP_Text keyDisplay = null;
    //Used for color changing during key binding
    public Button button = null;

    [Tooltip("Is mouse an acceptable input type for this binding?")]
    public bool AllowMouseButtons = true;
    [SerializeField]
    bool ShouldListenToReset = true;

	//Internal variables
    bool isHovering = false; //used for mouse controls

    //Initializes
    protected override void Awake()
    {
        base.Awake();

        //basically just loads the value that is set in the editor
        m_DefaultKeyCode = keyCode;

        button.onClick.AddListener(OnClick);

        m_VisualsUpdater = GetComponent<KeyBindButtonVisuals>();

        Listen("OnChangeKeybind", OnChangeKeybind);

        if (ShouldListenToReset)
        {
            Listen("OnResetDefaultKeybinds", OnResetDefaultKeybinds);
        }
    }

    //Loads KeyCodes
    void OnEnable()
    {
        keyCode = KeyBindingManager.GetKeyDictionary(keyAction, keyBindType);
        m_VisualsUpdater.UpdateVisuals(keyCode, keyBindType);
    }

    //called when the button is pressed
    public void OnClick()
    {
        if(button.interactable == false || KeyBindingManager.IsReassigning)
        {
            return;
        }

        //find what key pressed us and ignore it until it is released
        KeyCode code = FindSubmitKey();

        if (code != KeyCode.None)
        {
            StartCoroutine(WaitUntilKeyReleasedThenSetTrue(code));
        }
    }

    /// <summary>
    /// if we are assigning then this will abandon that and return unpressed state
    /// </summary>
    public void ExitSelection(bool select = true)
    {
        if (button.interactable)
        {
            return;
        }

        button.interactable = true;

        if (select)
        {
            button.Select();
        }
    }

    //Changes in button behavior should be made here
    void OnGUI()
	{
        //if we can interact with the button then it hasn't been clicked so we don't care
        if (button.interactable)
        {
            return;
        }

        Event curEvent = Event.current;

        //cancels binding if not hovering and mouse clicked
        if (curEvent.isMouse && !isHovering)
        {
            ExitSelection();
        }

        if (keyBindType == KeyBindType.primary || keyBindType == KeyBindType.alternate)
        {
            HandleKBMInput(curEvent);
        }
        else if (keyBindType == KeyBindType.gamepad || keyBindType == KeyBindType.gamepadalternate)
        {
            HandleGamepadInput();
        }
	}

    void HandleKBMInput(Event curEvent)
    {
        //Checks if key is pressed and if button has been pressed indicating wanting to re - assign
        if (curEvent.isKey && curEvent.keyCode != KeyCode.None)
        {
            //ignore the input if escape or start were pressed
            if (curEvent.keyCode == KeyCode.Escape)
            {
                ExitSelection();
                return;
            }
            else if (curEvent.keyCode == KeyCode.Backspace)
            {
                SetNewKeybind(KeyCode.None);
                m_VisualsUpdater.UpdateVisuals(keyCode, keyBindType);
                return;
            }

            SetNewKeybind(curEvent.keyCode);
        }
        else if (Input.GetKey(KeyCode.LeftShift)) //deals with some oddity in how Unity deals with shifts keys
        {
            SetNewKeybind(KeyCode.LeftShift);
        }
        else if (Input.GetKey(KeyCode.RightShift)) //deals with some oddity in how Unity deals with shifts keys
        {
            SetNewKeybind(KeyCode.RightShift);

        }
        //checks if mouse is pressed and assigns appropriate keycode
        else if (curEvent.isMouse && isHovering && AllowMouseButtons)
        {
           // StartCoroutine(WaitUntilKeyReleasedThenSetTrue(KeyCode.Mouse0));//prevents "over clicking"
                                   //converts mouse button to keycode - see Keycode defintion for why 323 is added.
            int _int = curEvent.button + 323;
            KeyCode mouseKeyCode = (KeyCode)_int;

            SetNewKeybind(mouseKeyCode);
        }
    }

    void HandleGamepadInput()
    {
        //gamepad
        if (CheckLastInputType.GetLastInputType() == eLastInputType.Gamepad)
        {
            KeyCode pressed = CheckLastInputType.GetLastGamepadButtonDown();

            if (pressed != KeyCode.None)
            {
                if (pressed == KeyCode.JoystickButton7)
                {
                    ExitSelection();
                    return;
                }
                else if (pressed == KeyCode.JoystickButton6)
                {
                    SetNewKeybind(KeyCode.None);
                    m_VisualsUpdater.UpdateVisuals(keyCode, keyBindType);
                    return;
                }

                SetNewKeybind(pressed);
            }
        }
    }

    void SetNewKeybind(KeyCode code)
    {
        keyCode = code;
        KeyBindingManager.UpdateDictionary(this);

        ExitSelection();

        StartCoroutine(WaitFrameAndReset());
    }

    KeyCode FindSubmitKey()
    {
        if(Input.GetKey(KeyCode.JoystickButton0))
        {
            return KeyCode.JoystickButton0;
        }
        if (Input.GetKey(KeyCode.KeypadEnter))
        {
            return KeyCode.KeypadEnter;
        }
        if (Input.GetKey(KeyCode.Return))
        {
            return KeyCode.Return;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            return KeyCode.Space;
        }
        if (Input.GetKey(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            return KeyCode.Mouse0;
        }

        return KeyCode.None;
    }

    void OnChangeKeybind(EventParam param)
    {
        OnChangeKeybindParam ckp = (OnChangeKeybindParam)param;

        //either if we don't share the key then we can ignore
        if (ckp.Code != keyCode)
        {
            return;
        }

        //if we share everything then we should update
        if (ckp.Code == keyCode && ckp.ActionType == keyAction && ckp.Type == keyBindType)
        {
            m_VisualsUpdater.UpdateVisuals(keyCode, keyBindType);
            return;
        }

        //otherwise we share the code and nothing else so we have to reset
        keyCode = KeyCode.None;
        KeyBindingManager.UpdateDictionary(this);
        m_VisualsUpdater.UpdateVisuals(keyCode, keyBindType);
    }

    void OnResetDefaultKeybinds()
    {
        keyCode = m_DefaultKeyCode;
        KeyBindingManager.UpdateDictionary(this, true);//we don't want to call events when we're just resetting
        m_VisualsUpdater.UpdateVisuals(keyCode, keyBindType);
    }

    #region IEnumerators

    /// <summary>
    /// waits until the button passed in is released and then says that we have started assigning
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    IEnumerator WaitUntilKeyReleasedThenSetTrue(KeyCode code)
    {
        yield return new WaitUntil(() => !Input.GetKey(code));

        KeyBindingManager.SetAssigning(true, this);
        button.interactable = false;
    }

    /// <summary>
    /// we need to wait 1 frame so that the menu classes don't receive any back screen or collapse screen inputs
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitFrameAndReset()
    {
        yield return 0;
        KeyBindingManager.SetAssigning(false, this);
    }
    #endregion

    #region InputHandlers
    public void OnPointerEnter(PointerEventData data)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        isHovering = false;
    }
    #endregion
}