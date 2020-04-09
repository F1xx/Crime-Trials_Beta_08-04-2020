using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(EventSystem))]
public class MenuBase : BaseObject
{
    [Header("Settings"), SerializeField]
    protected Canvas ContainerCanvas = null;
    [SerializeField]
    protected bool CloseBasePanelWhenOpeningSub = false;
    [SerializeField]
    protected bool DisablePanelsOnStart = true;

    [Header("Audio"), SerializeField]
    protected AudioWrapper AudioToPlayOnClick = null;
    [SerializeField]
    protected AudioWrapper AudioToPlayOnHover = null;
    [SerializeField]
    protected AudioWrapper AudioToPlayOnError = null;
    [SerializeField]
    AudioChannel Channel = null;
    [HideInInspector] public AudioChannel m_AudioSource { get { return Channel; } protected set { Channel = value; } }

    protected bool CanGoBackFromBaseMenu = true;

    protected Stack<MenuStackItem> m_Stack = new Stack<MenuStackItem>();
    [SerializeField]
    protected EventSystem m_System = null;

    protected eLastInputType LastInput = eLastInputType.KBM;

    public static bool SHOWHUD = true;

    protected override void Awake()
    {
        base.Awake();

        Listen("OnMenuHover", OnHover);
    }

    protected virtual void Start()
    {
        EventManager.Instance();
        TimerManager.Instance();
        FirebaseManager.Instance();
        SQLManager.Instance();
        AudioManager.Instance();
        SceneLoader.Instance();
        GraphicsManager.Instance();
        PrefabManager.Instance();
        KeyBindingManager.Instance();



        if (Channel == null)
        {
            m_AudioSource = GetComponent<AudioChannel>();
        }

        if (m_System == null)
        {
            m_System = GetComponent<EventSystem>();
        }

        List<GameObject> screens = new List<GameObject>();

        for (int i = 0; i < ContainerCanvas.transform.childCount; ++i)
        {
            screens.Add(ContainerCanvas.transform.GetChild(i).gameObject);
        }

        if (DisablePanelsOnStart)
        {
            foreach (GameObject c in screens)
            {
                c.SetActive(false);
            }
        }

        if (screens.Count > 0)
        {
            PushScreen(screens[0]);
            SetNewScreen();
        }
    }

    public virtual void LoadScene(string scene)
    {
        SceneLoader.GoToLoadingScreen(scene);
    }

    public void RestartLevel()
    {
        SceneLoader.GoToLoadingScreen(SceneManager.GetActiveScene().buildIndex);
        //SceneLoader.GoToLoadingScreen(SceneManager.GetActiveScene().name);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public virtual void LoadNextLevel()
    {
        OnClick();
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        currentScene++;

        if (currentScene >= sceneCount || currentScene < 1)
        {
            currentScene = 0;
        }
        //SceneManager.LoadScene(currentScene, LoadSceneMode.Single);
        SceneLoader.GoToLoadingScreen(currentScene);
    }

    protected virtual void Update()
    {
        if (m_Stack.Peek().Panel.activeInHierarchy)
        {
            LastInput = CheckLastInputType.GetLastInputType();

            switch (LastInput)
            {
                case eLastInputType.KBM:
                    Cursor.visible = true;
                    break;
                case eLastInputType.Gamepad:
                    Cursor.visible = false;
                    break;
                default:
                    break;
            }
        }

        if (Input.GetButtonDown("Cancel") && m_Stack.Peek().Panel.activeInHierarchy && KeyBindingManager.IsReassigning == false)
        {
            BackScreen();
        }

        if(Input.GetButtonDown("Pause") && m_Stack.Peek().Panel.activeInHierarchy && KeyBindingManager.IsReassigning == false)
        {
            CollapseStack();
            SetNewScreen();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            PlayerPrefs.SetInt("OnMatureSettingChange", 0);
            EventManager.TriggerEvent("OnMatureSettingChange");
        }
    }

    /// <summary>
    /// "closes" current panel and opens the one behind it. If we are the base this closes the menu.
    /// </summary>
    public virtual void BackScreen()
    {
        //if we are on the base screen and are not supposed to go any further back
        if(m_Stack.Count < 2 && CanGoBackFromBaseMenu == false)
        {
            //error sound
            OnClick(true);
            return;
        }


        OnClick();

        //if we are at the base of the stack, close.
        if(m_Stack.Count < 2)
        {
            CloseMenu();
            return;
        }

        PopScreen();
    }


    /// <summary>
    /// removed the current Panel from the stack, deactivating it. Sets the one under as active
    /// Only works if we aren't on the base panel
    /// </summary>
    /// <returns>true if suceeded</returns>
    protected virtual bool PopScreen()
    {
        Selectable sel = m_Stack.Peek().FormerSelected;

        bool val = PopScreenInactive();

        if(val)
        {
            if (m_Stack.Count > 0)
            {
                SetNewScreen(sel);
            }
            else
            {
                SetNewScreen();
            }
        }

        return val;
    }

    /// <summary>
    /// Same as PopScreen but does not set the new top of stack to active.
    /// </summary>
    /// <returns>true if successfully popping</returns>
    protected virtual bool PopScreenInactive()
    {
        if (m_Stack.Count > 1)
        {
            if (FindAndDeactivateAnimator() == false)
            {
                DisableCurrentScreen();
            }
            m_Stack.Pop();

            return true;
        }
        return false;
    }

    protected bool FindAndDeactivateAnimator()
    {
        Animator anim = m_Stack.Peek().Panel.GetComponent<Animator>();
        if(anim)
        {
            anim.SetBool("Open", false);
            return true;
        }

        return false;
    }

    public void SetDeactivated(GameObject panel)
    {
        panel.SetActive(false);
    }

    /// <summary>
    /// add the given panel to the top of the stack
    /// </summary>
    /// <param name="panel">The panel to add</param>
    protected virtual void PushScreen(GameObject panel)
    {
        if(CloseBasePanelWhenOpeningSub == false)
        {
            if(m_Stack.Count > 1)
            {
                DisableCurrentScreen();
            }
        }

        Selectable sel = null;
        if(m_System.currentSelectedGameObject)
        {
            sel = m_System.currentSelectedGameObject.GetComponent<Selectable>();
        }

        MenuStackItem item = new MenuStackItem(panel, sel);

        m_Stack.Push(item);
    }

    /// <summary>
    /// Whatever panel is at the top of the stack will get disabled
    /// </summary>
    protected virtual void DisableCurrentScreen()
    {
        if (m_Stack.Count > 0)
        {
            m_Stack.Peek().Panel.SetActive(false);
        }
    }

    /// <summary>
    /// Sets the top of the stack to active
    /// Tries to select the panel's first interactible
    /// </summary>
    protected virtual void SetNewScreen(Selectable target = null)
    {
        m_Stack.Peek().Panel.SetActive(true);

        if (target == null)
        {
            target = m_Stack.Peek().Panel.GetComponentInChildren<Selectable>();
        }

        if (target)
        {
            SelectObject(target);
            StartCoroutine("SelectContinueButtonLater", target);
            //m_System.SetSelectedGameObject(sel.gameObject);
        }

    }

    /// <summary>
    /// somehow this is actually required just to select a button
    /// </summary>
    /// <returns></returns>
    IEnumerator SelectContinueButtonLater(Selectable sel)
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(sel.gameObject);
        sel.Select();
    }

    /// <summary>
    /// Plays a click sound if it can
    /// Plays an error if error is true
    /// </summary>
    /// <param name="error">whether or not this click is an error click (like going back from a screen with no back</param>
    public void OnClick(bool error = false)
    {
        if (error == false && AudioToPlayOnClick)
        {
            m_AudioSource.SetChannelType(AudioManager.eChannelType.SoundEffects);
            m_AudioSource.PlayAudio(AudioToPlayOnClick);
        }
        else if(AudioToPlayOnError)
        {
            m_AudioSource.SetChannelType(AudioManager.eChannelType.SoundEffects);
            m_AudioSource.PlayAudio(AudioToPlayOnError);
        }
    }

    /// <summary>
    /// same as OnClick but plays a hover sound
    /// </summary>
    protected void OnHover()
    {
        //if (m_AudioSource.IsPlaying() == false && AudioToPlayOnHover)
        //{
        //    m_AudioSource.SetChannelType(AudioManager.eChannelType.SoundEffects);
        //    m_AudioSource.PlayAudio(AudioToPlayOnHover);
        //}
    }

    /// <summary>
    /// collapses the stack back to the base and sets the base screen disabled
    /// </summary>
    protected virtual void CloseMenu()
    {
        CollapseStack();
        DisableCurrentScreen();
    }

    /// <summary>
    /// When closing a menu the last screen should still remain on the stack but be disabled.
    /// This re-enables it
    /// </summary>
    protected virtual void OpenMenu()
    {
        if (m_Stack.Count > 0)
        {
            SetNewScreen();
        }
    }

    /// <summary>
    /// bring the stack down to just the base menu
    /// Then enables the base menu
    /// </summary>
    protected void CollapseStack()
    {
        int num = m_Stack.Count - 1;
        for (int i = 0; i < num; i++)
        {
            PopScreenInactive();
        }
    }

    /// <summary>
    /// Opens the panel from where we are on the stack, keeps adding.
    /// </summary>
    /// <param name="obj">Panel to open</param>
    public virtual void OpenPanel(GameObject obj)
    {
        OnClick();

        PushScreen(obj);
        SetNewScreen();
    }

    /// <summary>
    /// Opens the panel from where we are on the stack, keeps adding.
    /// </summary>
    /// <param name="obj">Panel to open</param>
    public virtual void OpenPanel(Animator obj)
    {
        OnClick();

        OpenPanel(obj.gameObject);
        obj.SetBool("Open", true);
    }

    /// <summary>
    /// collapses the stack to the base and opens the panel from there.
    /// </summary>
    /// <param name="obj">Panel to open</param>
    public virtual void OpenPanelFromRoot(GameObject obj)
    {
        CollapseStack();
        OpenPanel(obj);
    }

    /// <summary>
    /// just selects a selectable
    /// mostly used for menu stuff
    /// </summary>
    /// <param name="selectable">the selectable to be selected</param>
    public void SelectObject(Selectable selectable = null)
    {
        selectable.Select();
    }

    /// <summary>
    /// if in the editor stops playing the editor
    /// if in an application will fully quit the application
    /// </summary>
    public void QuitGame()
    {
        OnClick();
        EventManager.TriggerEvent("OnEditorQuit");

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
        return;
#else
        Application.Quit();
#endif
    }

    public struct MenuStackItem
    {
        public MenuStackItem(GameObject panel, Selectable selectableToSelectWhenClosing)
        {
            Panel = panel;
            FormerSelected = selectableToSelectWhenClosing;
        }

        public GameObject Panel { get; private set; }
        public Selectable FormerSelected { get; private set; }
    }
}
