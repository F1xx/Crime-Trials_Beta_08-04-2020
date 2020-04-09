/*
    - Generic behaviour for every camera
    - Responsible for initializing the camera driver, target to follow, pivot point and anything else all cameras might need
    - Also contains generic functionality that all camera's can use
*/
using UnityEngine;

public abstract class CameraBehaviour
{
    protected CameraSettings m_CameraSettings = null; // the camera settings

    // The actual CameraDriver object
    protected CameraDriver m_Camera = null;

    // Objects to move and rotate around
    protected GameObject m_TargetToFollow = null; // we get the input component from this object
    protected Transform m_PivotPoint = null; // this is the actual object that the camera sets its position to, usually an empty gameobject

    public abstract void UpdateCamera();
    public abstract void Activate();
    public abstract void Deactivate();
    public abstract void SetFacingDirection(Vector3 facingDirection);
    public abstract void OnPlayerDamage();

    public virtual void Init(CameraDriver camera, CameraSettings cameraSettings)
    {
        m_Camera = camera;
        m_CameraSettings = cameraSettings;

        if (m_CameraSettings != null)
        {
            m_TargetToFollow = m_CameraSettings.TargetToFollow;
            m_PivotPoint = m_CameraSettings.CameraPivotPoint;
        }
    }

    /// <summary>
    ///     Set this angle to -180/180 space
    /// </summary>
    protected float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    /// <summary>
    ///     Set this angle to 0/360 space
    /// </summary>
    protected float UnwrapAngle(float angle)
    {
        if (angle >= 0)
            return angle;

        angle = -angle % 360;

        return 360 - angle;
    }

    /// <summary>
    ///     Unity's Axes in relation to pitch, yaw, and roll
    ///     X = Pitch, Y = Yaw, Z = Roll,
    ///      also clamps the given axis
    /// </summary>
    protected void ClampAxis(ref float ClampValue, ref float DirtyValue, float min, float max)
    {
        if(ClampValue < min)
        {
            ClampValue = min;
            DirtyValue = 0.0f;
        }
        else if(ClampValue > max)
        {
            ClampValue = max;
            DirtyValue = 0.0f;
        }
    }
}
