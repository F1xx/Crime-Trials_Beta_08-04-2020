using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardSubmitPanel : BaseObject
{
    [Header("Submitting")]
    public TMPro.TMP_Text[] PlayerNameChars = { };
    [SerializeField]
    Leaderboard_BASE Leaderboard = null;
    [SerializeField]
    List<SideWaysNavigator> SubmittedName = new List<SideWaysNavigator>();
    public TMPro.TMP_Text PlayerScoreText = null;

    public static bool IsActive = false;

    [SerializeField]
    Button FirstSelected = null;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Submit()
    {
        string name = "";

        foreach(var item in SubmittedName)
        {
            name += item.GetCurrentValue();
        }

        Leaderboard.OnSubmit(name);
        Leaderboard.BackScreen();
    }

    private void OnEnable()
    {
        IsActive = true;
        StartCoroutine("SelectContinueButtonLater");
    }

    /// <summary>
    /// somehow this is actually required just to select a button
    /// </summary>
    /// <returns></returns>
    IEnumerator SelectContinueButtonLater()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(FirstSelected.gameObject);
        FirstSelected.Select();
    }

    private void OnDisable()
    {
        IsActive = false;
    }
}
