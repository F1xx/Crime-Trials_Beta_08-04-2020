/*
    - This script is the main driver for all camera behaviours
    - Responsible for creating all camera behaviours and switching them when necessary
    - Also passes the needed information to each behaviour from the CameraSettings script
    - Inherits from BaseObject and is the only thing that the inspector interacts with
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class CameraDriver : BaseObject
{
    // Holds all our camera settings; pass to CameraBehaviour
    [SerializeField]
    private CameraSettings CameraSettings = null;

    // Current Behaviour
    private CameraBehaviour m_CurrentBehaviour = null;

    private LiftGammaGain m_Brightness = null;
    private MotionBlur m_MotionBlur = null;
    private Vignette m_Vignette = null;

    // Neon Material
    private Material m_NeonMaterial = null;

    public void SetFacingDirection(Vector3 facingDirection)
    {
        m_CurrentBehaviour.SetFacingDirection(facingDirection);
    }

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoad;

        // Set starting camera to FirstPersonBehaviour for now (no other camera types)
        if (m_CurrentBehaviour == null)
        {
            m_CurrentBehaviour = new FirstPersonBehaviour();
        }

        // Setup events to check to see if player is changing settings
        Listen("OnFOVChange", OnFOVChange);
        Listen("OnSensitivityChange", OnSensitivityChange);
        Listen("OnToggleInvertedX", OnToggleInvertedX);
        Listen("OnToggleInvertedY", OnToggleInvertedY);

        // Post Effects
        Listen("OnBrightnessChange", OnBrightnessChange);
        Listen("OnMotionBlurChange", OnMotionBlurChange);
        Listen("OnToggleSpeedVision", OnToggleSpeedVision);

        // Neon Boy
        Listen("OnToggleNeonShader", OnToggleNeonShader);

        // Player takes damage event for red vignetting 
        Listen("OnPlayerDamaged", OnPlayerDamagedEvent);
    }

    private void Start()
    {
        // Load in the neon shader material
        m_NeonMaterial = Resources.Load<Material>("NeonShader");

        // Make sure to initialize the current behaviour before activating it
        // TargetToFollow has to be a character, otherwise input component will = null
        if (CameraSettings != null)
        {
            SetFacingDirection(CameraSettings.StartFacingDirection);

            LoadFromSettings();

            m_CurrentBehaviour.Init(this, CameraSettings);
            m_CurrentBehaviour.Activate();

            if (CameraSettings.PostEffectProfile != null)
            {
                m_Brightness = ScriptableObject.CreateInstance<LiftGammaGain>();
                CameraSettings.PostEffectProfile.TryGet(out m_Brightness);
                m_MotionBlur = ScriptableObject.CreateInstance<MotionBlur>();
                CameraSettings.PostEffectProfile.TryGet(out m_MotionBlur);
                m_Vignette = ScriptableObject.CreateInstance<Vignette>();
                CameraSettings.PostEffectProfile.TryGet(out m_Vignette);
            }
        }
    }

    private void LateUpdate()
    {
        if (m_CurrentBehaviour != null)
        {
            m_CurrentBehaviour.UpdateCamera();

            if (CameraSettings.PostEffectProfile != null)
            {
                m_Brightness.gain.value = Vector4.one * CameraSettings.Brightness;
                m_MotionBlur.intensity.value = CameraSettings.MotionBlur;
                m_Vignette.intensity.value = CameraSettings.Vignette;
                m_Vignette.color.Interp(Color.black, CameraSettings.VignetteColor, Time.deltaTime * 20.0f);
            }
        }
    }

    /// <summary>
    ///     Loads player's preferred camera settings
    /// </summary>
    private void LoadFromSettings()
    {
        if (PlayerPrefs.HasKey("StartFOV"))
        {
            OnFOVChange();
        }
        if (PlayerPrefs.HasKey("OnSensitivityChange"))
        {
            OnSensitivityChange();
        }
        if (PlayerPrefs.HasKey("OnToggleInvertedX"))
        {
            OnToggleInvertedX();
        }
        if (PlayerPrefs.HasKey("OnToggleInvertedY"))
        {
            OnToggleInvertedY();
        }
        if (PlayerPrefs.HasKey("OnMotionBlurChange"))
        {
            OnMotionBlurChange();
        }
        if (PlayerPrefs.HasKey("OnToggleSpeedVision"))
        {
            OnToggleSpeedVision();
        }
        if (PlayerPrefs.HasKey("OnBrightnessChange"))
        {
            OnBrightnessChange();
        }
        if (PlayerPrefs.HasKey("OnToggleNeonShader"))
        {
            OnToggleNeonShader();
        }
    }

    /// <summary>
    ///      Sets camera behaviour only if passed in behaviour is new behaviour
    /// </summary>
    public void SetCameraBehaviour(CameraBehaviour behaviour)
    {
        if (m_CurrentBehaviour == behaviour)
        {
            return;
        }

        if (m_CurrentBehaviour != null)
        {
            m_CurrentBehaviour.Deactivate();
        }

        m_CurrentBehaviour = behaviour;

        if (m_CurrentBehaviour != null)
        {
            m_CurrentBehaviour.Activate();
        }
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        var players = scene.GetRootGameObjects().Where(x => x.transform.root.name == "Player").ToList();

        if (players.Any())
        {
            CameraSettings.TargetToFollow = players[0];
            //CameraSettings.ArmToRotate = GameObject.Find("Armgun_UpperArm").transform;
            CameraSettings.ArmToRotate = GameObject.Find("MarineRibcage").transform;

            CameraSettings.CameraPivotPoint = GameObject.Find("ThisWhereCameraGo").transform;
            //CameraSettings.CameraPivotPoint = GameObject.Find("MarineRibcage").transform;
            

            // Post effect profile loads OnScene to make sure its attached properly
            GameObject postEffectStack = GameObject.Find("PostEffectStack");

            if(postEffectStack != null)
            {
                CameraSettings.PostEffectProfile = postEffectStack.GetComponent<Volume>().profile;
            }
        }
    }

    #region -- Camera Settings Event Changes -- 

    private void OnFOVChange()
    {
        CameraSettings.StartFOV = PlayerPrefs.GetFloat("OnFOVChange");
    }
    private void OnSensitivityChange()
    {
        CameraSettings.Sensitivity = PlayerPrefs.GetFloat("OnSensitivityChange");

        if (m_CurrentBehaviour as PlayerControlledBehaviour != null)
        {
            ((PlayerControlledBehaviour)m_CurrentBehaviour).SetSensitivity();
        }
    }
    private void OnToggleInvertedX()
    {
        CameraSettings.IsInvertedX = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleInvertedX"));
    }
    private void OnToggleInvertedY()
    {
        CameraSettings.IsInvertedY = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleInvertedY"));
    }
    private void OnMotionBlurChange()
    {
        CameraSettings.MotionBlur = PlayerPrefs.GetFloat("OnMotionBlurChange");
    }
    private void OnToggleSpeedVision()
    {
        CameraSettings.SpeedVision = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleSpeedVision"));
    }
    private void OnBrightnessChange()
    {
        CameraSettings.Brightness = PlayerPrefs.GetFloat("OnBrightnessChange");
    }
    private void OnToggleNeonShader()
    {
        if(m_NeonMaterial != null)
        {
            int neonValue = PlayerPrefs.GetInt("OnToggleNeonShader");
            m_NeonMaterial.SetInt("_NeonOn", neonValue);
        }
    }
    private void OnPlayerDamagedEvent(EventParam param)
    {
        m_CurrentBehaviour.OnPlayerDamage();
    }

    #endregion
}