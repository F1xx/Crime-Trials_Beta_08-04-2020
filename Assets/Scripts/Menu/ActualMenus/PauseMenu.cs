using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MenuBase
{
    [Header("PauseMenu")]
    //public GameObject BackgroundObject = null;
    protected Button DisabledButton = null;

    [SerializeField]
    protected GameObject[] SubMenuBases = null;

    [SerializeField]
    protected GameObject[] SubMenuContainers = null;

    protected override void Awake()
    {
        foreach(GameObject sub in SubMenuContainers)
        {
            DisableContainerContents(sub);
        }

        base.Awake();
    }

    /// <summary>
    /// Used to disable all screens in the SubMenuContainers
    /// </summary>
    /// <param name="container"></param>
    void DisableContainerContents(GameObject container)
    {
        List<GameObject> screens = new List<GameObject>();

        for (int i = 0; i < container.transform.childCount; ++i)
        {
            screens.Add(container.transform.GetChild(i).gameObject);
        }

        foreach (GameObject c in screens)
        {
            c.SetActive(false);
        }
    }

    protected override void Start()
    {
        base.Start();

        //hide the pause menu at start
        DisableCurrentScreen();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetButtonDown("Pause"))
        {
            ToggleMenu();
        }
    }

    public void Resume()
    {
        OnClick();
        ToggleMenu();
    }

    private void ToggleMenu()
    {
        if(DevConsole.Instance().IsActive || PlayerHUDLeaderboard.IsShowing || DeathFade.IsActive || LeaderboardSubmitPanel.IsActive)
        {
            return;
        }

        if(PauseManager.IsPaused())
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    protected override void OpenMenu()
    {
        SetNewDisabledButton(null);
        base.OpenMenu();

        PauseManager.Pause();
    }

    protected override void CloseMenu()
    {
        SetNewDisabledButton(null);
        base.CloseMenu();

        PauseManager.Unpause();
    }

    /// <summary>
    /// "closes" current panel and opens the one behind it. If we are the base this closes the menu.
    /// </summary>
    public override void BackScreen()
    {
        SetNewDisabledButton(null);
        base.BackScreen();
    }

    /// <summary>
    /// This Opens whatever panel you need from the root, ie. it will collapse the stack until you're at the base and open it from there.
    /// Note the Button MUST have a PanelToOpen Script
    /// </summary>
    /// <param name="btn">The Button Containing the BPanelToOpen script and which will be disabled.</param>
    public void OpenPanelFromRoot(Button btn)
    {
        HandlePanel(btn, base.OpenPanelFromRoot);
    }

    public void OpenPanel(Button btn)
    {
        CollapseToSubMenu();

        HandlePanel(btn, base.OpenPanel);
    }

    /// <summary>
    /// Collapses the stack to the topmost sub menu. IE. if you're in a sub menu in a sub menu in a sub menu
    /// it will bring you to the 3rd one, not back to the beginning. That wasn't super clear but whatever.
    /// </summary>
    void CollapseToSubMenu()
    {
        //if we're on a sub menu already then we're collapsed
        if(CheckIfScreenIsSubMenu(m_Stack.Peek().Panel))
        {
            return;
        }

        //Fine the closest submenu on the stack
        int indexInStack = -1;
        for (int i = 0; i < m_Stack.Count; i++)
        {
            GameObject obj = m_Stack.ToArray()[i].Panel;

            if (CheckIfScreenIsSubMenu(obj))
            {
                indexInStack = m_Stack.Count - i;
                break;
            }
        }

        //if we found a submenu pop screens off the stack until we get to it
        if (indexInStack >= 0)
        {
            while(m_Stack.Count > indexInStack)
            {
                PopScreen();   
            }
        }
    }

    /// <summary>
    /// Checks if the object passed in is one of the objects registered with this script as a submenu
    /// </summary>
    /// <param name="toCheck">The object to find out about</param>
    /// <returns>true if the passed in GameObject is one of our registered submenus</returns>
    bool CheckIfScreenIsSubMenu(GameObject toCheck)
    {
        foreach (GameObject sub in SubMenuBases)
        {
            if (toCheck == sub)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// This will get the right Panel from the button passed, disable the button and based on the function open the panel.
    /// </summary>
    /// <param name="btn">The button to disable and get the panel from</param>
    /// <param name="func">The function to use to open (Just open or from base)</param>
    void HandlePanel(Button btn, System.Action<GameObject> func)
    {
        GameObject obj = GetGameObjectFromButton(btn);

        if (obj)
        {
            SetNewDisabledButton(btn);
            func(obj);
        }
    }

    /// <summary>
    /// Just used to extract the PanelToOpen script and it's contained gameobject from the button
    /// </summary>
    /// <param name="btn"></param>
    /// <returns>the object representing the Panel that needs to be opened</returns>
    GameObject GetGameObjectFromButton(Button btn)
    {
        GameObject obj = null;

        PanelToOpen panel = btn.GetComponent<PanelToOpen>();

        if (panel)
        {
            obj = panel.OpenPanel;

            if (!obj)
            {
                Debug.LogError("Button: " + btn.name + "'s PanelToOpen needs to have a Panel set to it.");
            }
        }
        else
        {
            Debug.LogError("Button: " + btn.name + " needs a PanelToOpen Script to be attached to use this function.");
        }

        return obj;
    }

    /// <summary>
    /// When opening a menu, since the buttons remain visible we will disable whatever one we are currently on.
    /// </summary>
    /// <param name="btn"></param>
    void SetNewDisabledButton(Button btn)
    {
        //if(DisabledButton)
        //{
        //    DisabledButton.interactable = true;
        //}

        //DisabledButton = btn;

        //if (btn)
        //{
        //    DisabledButton.interactable = false;
        //}
    }
}
