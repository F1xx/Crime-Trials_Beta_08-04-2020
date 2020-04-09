/*
    - Base class for all player contorlled camera behaviours
    - Includes override functions and variables needed for anything 
*/

using UnityEngine;

public abstract class PlayerControlledBehaviour : CameraBehaviour
{
    protected StateMachineBase m_StateMachine;
    protected InputComponentBase m_TargetInput;

    protected float MouseSensitivity = 0.0f;

    protected abstract void SubscribeToTargetState();
    protected abstract void UnsubscribeToTargetState();
    protected abstract void TargetStateChanges(StateChangeStruct stateStruct);

    public override void Activate()
    {
        MouseSensitivity = m_CameraSettings.Sensitivity;

        SetStateMachine(m_TargetToFollow);
        SetInputComponent();  
    }

    /// <summary>
    /// sets the behavior's sensitivity to the sensitivity in CameraSettings
    /// </summary>
    public virtual void SetSensitivity()
    {
        if (m_CameraSettings)
        {
            MouseSensitivity = m_CameraSettings.Sensitivity;
        }
    }

    protected virtual void SetStateMachine(GameObject targetToFollow)
    {
        // If target has a state machine base
        if (targetToFollow.GetComponent<StateMachineBase>() != null)
        {
            m_StateMachine = targetToFollow.GetComponent<StateMachineBase>();
        }
    }

    /// <summary>
    /// Sets InputComponent based on the current m_TargetToFollow
    /// </summary>
    protected void SetInputComponent()
    {
        // If target is valid && target has a character component
        if (m_TargetToFollow != null && m_TargetToFollow.GetComponent<Character>() != null)
        {
            // Store character input component
            m_TargetInput = m_TargetToFollow.GetComponent<Character>().m_InputComponent;
        }
        else
        {
            m_TargetInput = null;
        }

        if (m_TargetInput != null)
        {
            m_Camera.transform.position = m_PivotPoint.transform.position;
        }
    }

    /// <summary>
    ///     Update input for player controlled cameras
    /// </summary>
    protected void UpdateInput(ref float x, ref float y)
    {
        if (m_TargetInput != null)
        {
            x = (m_TargetInput.GetLookInput().x * MouseSensitivity);
            y = (m_TargetInput.GetLookInput().y * MouseSensitivity);
        }
    }

    /// <summary>
    ///     Gets m_TargetToFollow's current move eulerAngles.rotation.y based on move direction
    /// </summary>
    protected float GetPlayerMoveDirection(StateBase currentState)
    {
        if (currentState.Is<StateWallRunning>())
        {
            StateWallRunning state = (StateWallRunning)currentState;

            // Grab hit object
            RaycastHit WallObjectHit = state.AttachedWallObject;

            // Create our direction vector by crossing our collision normal with the UP axis.
            Vector3 moveDirection = Vector3.Cross(Vector3.up, WallObjectHit.normal);

            // If running on right side return negative
            moveDirection = state.IsRunningOnRightSide ? moveDirection : -moveDirection;

            // Set our angle to start from
            return Quaternion.LookRotation(moveDirection).eulerAngles.y;
        }
        
        return 0.0f;        
    }
}
