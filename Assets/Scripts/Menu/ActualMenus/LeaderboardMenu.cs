using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardMenu : Leaderboard_BASE
{
    [SerializeField]
    SideWaysNavigator LevelToChoose = null;

    const string LEVEL_PREF_KEY = "MainMenuLeaderBoardSelectedLevel";
    const int SCENES_BEFORE_FIRST_LEVEL_IN_BUILD_ORDER = 2;

    int m_CurrentLevel = 2;

    protected override void Awake()
    {
        base.Awake();

        Listen(LEVEL_PREF_KEY, LevelSelectChanged);
    }

    void LevelSelectChanged()
    {
        int temp = m_CurrentLevel;
        m_CurrentLevel = PlayerPrefs.GetInt(LEVEL_PREF_KEY, 0) + SCENES_BEFORE_FIRST_LEVEL_IN_BUILD_ORDER;

        if(RequestLoadLeaderboard(m_CurrentLevel) == false)
        {
            m_CurrentLevel = temp;
            LevelToChoose.ResetToPrevValue();
        }
    }

    protected override void OnChangeDBType()
    {
        //Loads the DB type
        LoadSettings();
        RequestLoadLeaderboard(m_CurrentLevel);
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        IsActive = true;
        LevelSelectChanged();
    }

    private void OnDisable()
    {
        IsActive = false;
    }
}

