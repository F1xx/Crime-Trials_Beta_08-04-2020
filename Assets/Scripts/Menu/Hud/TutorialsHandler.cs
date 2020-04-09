using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsHandler : BaseObject
{
    [SerializeField, Header("TutorialsHandler")]
    List<TutorialPopup> Popups = new List<TutorialPopup>();
    [SerializeField]
    CanvasGroup m_Group = null;

    protected override void Awake()
    {
        base.Awake();

        Listen("OnToggleTutorials", LoadSettings);

        foreach(TutorialPopup popup in Popups)
        {
            popup.Handler = this;
        }

    //    ClearAllPopups();
    }

    void LoadSettings()
    {
        SetCanvasVisible(System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleTutorials", 1)));
    }

    void SetCanvasVisible(bool visible)
    {
        if(visible)
        {
            m_Group.alpha = 1.0f;
        }
        else
        {
            m_Group.alpha = 0.0f;
        }
    }

    void ClearAllPopups()
    {
        foreach(var pop in Popups)
        {
            pop.Clear();
        }
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Keypad1))
        //{
        //    RemoveTutorial(Popups[0]);
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad2))
        //{
        //    RemoveTutorial(Popups[1]);
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad3))
        //{
        //    RemoveTutorial(Popups[2]);
        //}

        //if (Input.GetKeyDown(KeyCode.Keypad4))
        //{
        //    AddTutorial("Test1", "This is the first test", 5.0f);
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad5))
        //{
        //    AddTutorial("Test2", "This is the second test", 5.0f);
        //}
        //if (Input.GetKeyDown(KeyCode.Keypad6))
        //{
        //    AddTutorial("Test3", "This is the third test", 5.0f);
        //}
    }

    public TutorialPopup AddTutorial(TutorialPopup toAdd)
    {
        return AddTutorial(toAdd.Title, toAdd.Body, toAdd.BaseDuration);
    }

    public TutorialPopup AddTutorial(string title, string body, float duration)
    {
        TutorialPopup tut = FindBottomMostTutorial();
        tut.SetAndStart(title, body, duration);

        return tut;
    }

    public void RemoveTutorial(TutorialPopup target)
    {
        int index = GetIndexFromTutorial(target);

        if(index < 0)
        {
            return;
        }

        if(Popups[index].IsActive)
        {
            Popups[index].Clear();
        }

        if(CheckIfNeedToMoveUpTutorials(index))
        {
            for (int i = index; i < Popups.Count - 1; i++)
            {
                int next = i + 1;

                //set the tutorial with all the info of the one below it
                SetTutorialInfo(Popups[i], Popups[next]);
                //clear the one that was copied
                Popups[next].Clear();
            }
        }
    }


/// <summary>
/// Copy one tutorial's info to another. If the thing being copied from is running then the copier will continue running
/// </summary>
/// <param name="oldPopup">the one to override</param>
/// <param name="newPopup">the one to get the info from</param>
    void SetTutorialInfo(TutorialPopup oldPopup, TutorialPopup newPopup)
    {
        oldPopup.Set(newPopup);
    }

    int GetIndexFromTutorial(TutorialPopup target)
    {
        int index = -1;

        for(int i = 0; i < Popups.Count; i++)
        {
            if(Popups[i] == target)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    /// <summary>
    /// checks to see if there are any active pop-ups below the current one
    /// </summary>
    /// <param name="startingindex"></param>
    /// <returns>true if there are</returns>
    bool CheckIfNeedToMoveUpTutorials(int startingindex)
    {
        for (int i = startingindex + 1; i < Popups.Count; i++)
        {
            if(Popups[i].IsActive)
            {
                return true;
            }
        }
        return false;
    }

    TutorialPopup FindBottomMostTutorial()
    {
        for (int i = 0; i < Popups.Count; i++)
        {
            if (Popups[i].IsActive == false)
            {
                return Popups[i];
            }
        }

        //if we hit here then all are active so we have to push off the top one
        //remove the top tutorial which pushes everything up
        RemoveTutorial(Popups[0]);
        //return the bottom one
        return Popups[Popups.Count - 1];
    }

    protected override void OnSoftReset()
    {
        base.OnSoftReset();
        ClearAllPopups();
    }
}
