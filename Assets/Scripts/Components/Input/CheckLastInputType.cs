using UnityEngine;
using XInputDotNetPure;
using static XInputDotNetPure.GamePadThumbSticks;

//used to determine if the last input type was kbm or controller
public static class CheckLastInputType
{
    public static GamePadState ControllerState { get; private set; }

    static PlayerIndex m_ControllerIndex = PlayerIndex.One;
    static eLastInputType m_LastInputType = eLastInputType.KBM;

    static KeyCode LastControllerButtonPressed = KeyCode.None;

    /// <summary>
    /// Call this in update to remain accurate. Returns the most recent state.
    /// </summary>
    /// <returns>Whether the player last used KB+M or Gamepad</returns>
    public static eLastInputType GetLastInputType()
    {
        UpdateState();
        return m_LastInputType;
    }

    static void UpdateState()
    {
        switch (m_LastInputType)
        {
            case eLastInputType.KBM:
                if (ControlerAnyKey())
                {
                    m_LastInputType = eLastInputType.Gamepad;
                }
                break;
            case eLastInputType.Gamepad:
                if (isMouseKeyboard())
                {
                    m_LastInputType = eLastInputType.KBM;
                }
                break;
        }
    }

    private static bool isMouseKeyboard()
    {
        // mouse & keyboard buttons
        if (Input.anyKeyDown && ControlerAnyKey() == false)
        {
            return true;
        }

        // mouse movement
        if (Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f)
        {
            return true;
        }

        return false;
    }

    //kill me
    //returns true if anything is touched, false otherwise
    private static bool ControlerAnyKey()
    {
        ControllerState = GamePad.GetState(m_ControllerIndex);

        //if (ControllerState.Buttons.A == ButtonState.Pressed)
        if (Input.GetKey(KeyCode.JoystickButton0))
        {
            return true;
        }
        //else if (ControllerState.Buttons.B == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton1))
        {
            return true;
        }
        //else if (ControllerState.Buttons.X == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton2))
        {
            return true;
        }
        //else if (ControllerState.Buttons.Y == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton3))
        {
            return true;
        }
        //else if (ControllerState.Buttons.Start == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton7))
        {
            return true;
        }
        //else if (ControllerState.Buttons.Back == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton6))
        {
            return true;
        }
        else if (ControllerState.Buttons.Guide == ButtonState.Pressed)
        {
            return true;
        }
        //else if (ControllerState.Buttons.LeftShoulder == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton4))
        {
            return true;
        }
        //else if (ControllerState.Buttons.RightShoulder == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton5))
        {
            return true;
        }
        //else if (ControllerState.Buttons.RightStick == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton9))
        {
            return true;
        }
        //else if (ControllerState.Buttons.LeftStick == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton8))
        {
            return true;
        }
        //dpad
        else if (ControllerState.DPad.Up == ButtonState.Pressed)
        {
            return true;
        }
        else if (ControllerState.DPad.Down == ButtonState.Pressed)
        {
            return true;
        }
        else if (ControllerState.DPad.Left == ButtonState.Pressed)
        {
            return true;
        }
        else if (ControllerState.DPad.Right == ButtonState.Pressed)
        {
            return true;
        }

        //TODO: make this less precise than !=

        //triggers
        else if (ControllerState.Triggers.Left != 0.0f)
        {
            return true;
        }
        else if (ControllerState.Triggers.Right != 0.0f)
        {
            return true;
        }
        //Sticks
        //left
        else if (ControllerState.ThumbSticks.Left.X != 0.0f)
        {
            return true;
        }
        else if (ControllerState.ThumbSticks.Left.Y != 0.0f)
        {
            return true;
        }
        //right
        else if (ControllerState.ThumbSticks.Right.X != 0.0f)
        {
            return true;
        }
        else if (ControllerState.ThumbSticks.Right.Y != 0.0f)
        {
            return true;
        }

        return false;
    }

    public static KeyCode GetLastGamepadButtonDown()
    {
        LastControllerButtonPressed = KeyCode.None;

        //if (ControllerState.Buttons.A == ButtonState.Pressed)
        if (Input.GetKey(KeyCode.JoystickButton0))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton0;
        }
        //else if (ControllerState.Buttons.B == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton1))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton1;
        }
        //else if (ControllerState.Buttons.X == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton2))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton2;
        }
        //else if (ControllerState.Buttons.Y == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton3))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton3;
        }
        //else if (ControllerState.Buttons.Start == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton7))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton7;
        }
        //else if (ControllerState.Buttons.Back == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton6))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton6;
        }
        //else if (ControllerState.Buttons.LeftShoulder == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton4))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton4;
        }
        //else if (ControllerState.Buttons.RightShoulder == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton5))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton5;
        }
        //else if (ControllerState.Buttons.RightStick == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton9))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton9;
        }
        //else if (ControllerState.Buttons.LeftStick == ButtonState.Pressed)
        else if (Input.GetKey(KeyCode.JoystickButton8))
        {
            LastControllerButtonPressed = KeyCode.JoystickButton8;
        }

        return LastControllerButtonPressed;
    }
}

public enum eLastInputType
{
    None,
    KBM,
    Gamepad
}