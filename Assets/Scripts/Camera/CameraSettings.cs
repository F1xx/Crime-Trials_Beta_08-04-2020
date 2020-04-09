/*
    - Holds all settings for any camera
     
       ______________        _____________________________________
      |             #|      X                                     X
      |           # #|      |                                     |
      |          # ##|      |  Vhy hallow, I hope dis code help!  |    
      |        ## ###|      |                                     |
   ----------------------   X_____________________________________X
      |              |          
      |   X   \  x   |
      |       |      |
      |       _]     |      
      |  --v-----V-  |      
      \______________/    
*/
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable, CreateAssetMenu(fileName = "CameraSettings", menuName = "ScriptableObjects/Camera/Settings", order = 1)]
public class CameraSettings : ScriptableObject
{
    [HideInInspector]
    public GameObject TargetToFollow = null;
    [HideInInspector]
    public Transform ArmToRotate;
    [HideInInspector]
    public Transform CameraPivotPoint = null;
    [HideInInspector]
    public VolumeProfile PostEffectProfile = null;

    // Only use this if you want to rotate the player at certain angles on start
    public Vector3 StartFacingDirection = new Vector3(0.0f, 0.0f, 0.0f);

    #region -- First-Person Camera Behaviour Variables --

    public float Sensitivity = 6.0f;

    public bool IsInvertedX = false;
    public bool IsInvertedY = false;

    public float Brightness = 1.0f;
    public float MotionBlur = 0.2f;
    public float Vignette = 0.0f;
    public Color VignetteColor = Color.black;
    public bool SpeedVision = true;

    #region -- FOV Tween Variables -- 

    // Determine what FOV to tween to at what speed
    public float FOVMaxSpeedOffset = 15.0f;
    public float FOVMidSpeedOffset = 10.0f;
    public float StartFOV = 80.0f;

    // The thresholds where the tween starts
    public float FOVMaxSpeedThreshold = 28.0f;
    public float FOVMidSpeedThreshold = 6.0f;

    // How fast the tween happens for each threshold
    public float FOVMaxSpeedTweenDuration = 0.8f;
    public float FOVMidSpeedTweenDuration = 0.5f;
    public float FOVMinSpeedTweenDuration = 0.1f;

    #endregion

    #region -- Wallrunning Variables -- 

    // Duration for both types of wallrunning tweens
    public float WallRunningClampTweenDuration = 0.4f;
    public float RollTweenDuration = 0.2f;

    // How much do we roll when wallrunning?
    public float WallRunningRollAngle = 15.0f;

    // Viewing range while wallrunning
    public float AngleLockOffsetTowardsWall = 10.0f;
    public float AngleLockOffsetAwayWall = 90.0f;

    #endregion

    #endregion
}

// If we're using the unity editor, use custom inspector for the Camera Settings
#if UNITY_EDITOR

// This class will affect how the CameraSettigns script is seen in the inspector
[CustomEditor(typeof(CameraSettings))]
public class CameraSettingsEditor : Editor
{
    // CameraSettings object we're accessing
    CameraSettings csObj;

    // For foldout groups
    static bool showFOVSettings = false;
    static bool showWallrunSettings = false;

    // Set our settings object to our script's target object - will always be CameraSettings
    void OnEnable()
    {
        csObj = (CameraSettings)target;
    }

    public override void OnInspectorGUI()
    {
        // Camera settings header
        EditorGUILayout.Space();
        GUILayout.Label(" -- Camera Settings -- ", new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 18
        });
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.Space();
        #region -- Camera Setup -- 

        // Camera setup header
        GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 14
        });
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Object fields
        csObj.TargetToFollow = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target To Follow", "Target Camera is attached to"), csObj.TargetToFollow, typeof(GameObject), true);
        csObj.CameraPivotPoint = (Transform)EditorGUILayout.ObjectField(new GUIContent("Camera Pivot Point", "Pivot Point of the Camera"), csObj.CameraPivotPoint, typeof(Transform), true);
        csObj.ArmToRotate = (Transform)EditorGUILayout.ObjectField(new GUIContent("Arm To Rotate", "Arm that appears in First-Person"), csObj.ArmToRotate, typeof(Transform), true);
        csObj.PostEffectProfile = (VolumeProfile)EditorGUILayout.ObjectField(new GUIContent("Post Effect Profile", "Holds post effect stack for the current level"), csObj.PostEffectProfile, typeof(VolumeProfile), true);
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Vector3 field
        csObj.StartFacingDirection = EditorGUILayout.Vector3Field(new GUIContent("Set Facing Direction", "0,0,0 will be default world space rotation"), csObj.StartFacingDirection);
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        #endregion

        EditorGUILayout.Space();
        #region -- FOV Tween Variables --

        showFOVSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showFOVSettings, new GUIContent("FOV Tween Modifiers", "Stat modifiers that will apply when player is sprinting."));
        if (showFOVSettings)
        {
            // FOV tween header
            GUILayout.Label("FOV Tween Settings", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            });
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // FOV numbers
            csObj.FOVMaxSpeedOffset = EditorGUILayout.Slider(new GUIContent("FOV + Offset (Max Speed)"), csObj.FOVMaxSpeedOffset, 15.0f, 35.0f);
            csObj.FOVMidSpeedOffset = EditorGUILayout.Slider(new GUIContent("FOV + Offset (Mid Speed)"), csObj.FOVMidSpeedOffset, 5.0f, 25.0f);
            csObj.StartFOV = EditorGUILayout.Slider(new GUIContent("Starting FOV"), csObj.StartFOV, 50.0f, 100.0f);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Tween thresholds 
            csObj.FOVMaxSpeedThreshold = EditorGUILayout.Slider(new GUIContent("Max FOV Threshold", "Speed at which max FOV starts"), csObj.FOVMaxSpeedThreshold, 10.0f, 30.0f);
            csObj.FOVMidSpeedThreshold = EditorGUILayout.Slider(new GUIContent("Mid FOV Threshold", "Speed at which mid FOV starts"), csObj.FOVMidSpeedThreshold, 5.0f, 25.0f);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Tween durations
            csObj.FOVMaxSpeedTweenDuration = EditorGUILayout.Slider(new GUIContent("Max Speed FOV Tween Duration", "Duration of tween to get to value of FOV at max speed"), csObj.FOVMaxSpeedTweenDuration, 0.1f, 2.0f);
            csObj.FOVMidSpeedTweenDuration = EditorGUILayout.Slider(new GUIContent("Mid Speed FOV Tween Duration", "Duration of tween to get to value of FOV at mid speed"), csObj.FOVMidSpeedTweenDuration, 0.1f, 2.0f);
            csObj.FOVMinSpeedTweenDuration = EditorGUILayout.Slider(new GUIContent("Min Speed FOV Tween Duration", "Duration of tween to get to value of FOV at min speed"), csObj.FOVMinSpeedTweenDuration, 0.1f, 2.0f);
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        #endregion

        EditorGUILayout.Space();
        #region -- Wallrunning Variables -- 

        showWallrunSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showWallrunSettings, new GUIContent("Wallrun Tween Modifiers", "Stat modifiers that will apply when player is wallrunning."));
        if (showWallrunSettings)
        {
            // Wallrun Tween settings header
            GUILayout.Label("Wallrun Tween Settings", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            });
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Tween types
            csObj.RollTweenDuration = EditorGUILayout.Slider(new GUIContent("Roll Tween Duration"), csObj.RollTweenDuration, 0.0f, 1.0f);
            csObj.WallRunningClampTweenDuration = EditorGUILayout.Slider(new GUIContent("Mouse Clamp Tween Duration"), csObj.WallRunningClampTweenDuration, 0.0f, 1.0f);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            csObj.WallRunningRollAngle = EditorGUILayout.Slider(new GUIContent("Wallrun Roll Angle"), csObj.WallRunningRollAngle, 5.0f, 45.0f);
            csObj.AngleLockOffsetTowardsWall = EditorGUILayout.Slider(new GUIContent("View Angle Towards Wall"), csObj.AngleLockOffsetTowardsWall, 0.0f, 45.0f);
            csObj.AngleLockOffsetAwayWall = EditorGUILayout.Slider(new GUIContent("View Angle Away From Wall"), csObj.AngleLockOffsetAwayWall, 45.0f, 90.0f);
            EditorGUILayout.Space();
        }

        #endregion
    }
}

#endif