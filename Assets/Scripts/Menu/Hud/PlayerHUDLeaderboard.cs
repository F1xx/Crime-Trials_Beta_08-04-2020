using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHUDLeaderboard : Leaderboard_BASE
{
    public static bool IsShowing { get; private set; }

    [Header("Leaderboard"), SerializeField]
    GameObject SubmitPanel = null;
    LeaderboardSubmitPanel SubmitPanelScript = null;
    [SerializeField]
    TMPro.TMP_Text PlayerScoreText = null;

    string m_PlayerScore = "";
    string m_Ordinal = "";

    protected override void Awake()
    {
        base.Awake();


        IsActive = false;
        IsShowing = false;
    }

    protected override void Start()
    {
        base.Start();

        SubmitPanelScript = SubmitPanel.GetComponent<LeaderboardSubmitPanel>();

        Listen("OnLevelComplete", OnLevelComplete);
        //hide the menu at start
        DisableCurrentScreen();
    }

    public void SubmitScore()
    {
        if (DevConsole.Instance().HasBeenActivated)
        {
            OnClick(true);
        }
        else
        {
            OnClick();
            OpenPanel(SubmitPanel);
        }
    }

    public override void LoadNextLevel()
    {
        IsActive = false;
        base.LoadNextLevel();
    }

    public override bool RequestLoadLeaderboardFB(int LevelID)
    {
        if (LoadLeaderboardTask == null && FirebaseManager.Instance().IsUsableAndAvailable())
        {
            LoadLeaderboardTask = FirebaseManager.GetLeaderboardAsync(LevelID);
            return true;
        }
        return false;
    }

    public override bool RequestLoadLeaderboardLocal(int LevelID)
    {
        return base.RequestLoadLeaderboardLocal(LevelID);
    }

    public void OnLevelComplete(EventParam eventparam)
    {
        IsActive = true;

        base.OpenPanel(gameObject);

        OnLevelCompleteParam param = (OnLevelCompleteParam)eventparam;
        CurrentTime = param.TimeReached;
        m_PlayerScore = "YOUR SCORE: " + WorldTimeManager.GetTimeAsFormattedString(CurrentTime);
        SubmitScore();

        RequestLoadLeaderboard();
    }

    protected override void OnLeaderboardFilled()
    {
        base.OnLeaderboardFilled();

        int index = FindPlaceInLeaderboard();

        m_Ordinal = " (" + MathUtils.AddOrdinal(index + 1) + ")";

        SetScore();
    }

    void SetScore()
    {
        PlayerScoreText.text = m_PlayerScore + m_Ordinal;
        SubmitPanelScript.PlayerScoreText.text = m_PlayerScore;
    }

    bool IsSlotMatching(int index)
    {
        if(LeaderboardScoresAsFloats.Count <= index)
        {
            return false;
        }

        return MathUtils.FloatCloseEnough(CurrentTime, LeaderboardScoresAsFloats[index], 0.0009f);
    }

    int FindPlaceInLeaderboard()
    {
        int index = LeaderboardScoresAsFloats.Count;//if the leaderboard is empty then we'd be in first

        for (int i = 0; i < LeaderboardScoresAsFloats.Count; i++)
        {
            //find them if they submitted
            if (DBChoice == 0)
            {
                if (Submitted)
                {
                    //if submitted find our time
                    if (IsSlotMatching(i) && UsedName == LeaderboardNames[i])
                    {
                        index = i;
                        break;
                    }
                }
                else //otherwise find the next best time
                {
                    //if this slot matches our time
                    if (IsSlotMatching(i))
                    {
                        //continue to the next one if they are the same time (we'll be at the bottom)
                        if (IsSlotMatching(i + 1))
                        {
                            continue;
                        }

                        index = i + 1;
                        break;
                    }
                    else if (CurrentTime < LeaderboardScoresAsFloats[i])
                    {
                        index = i - 1;
                        break;
                    }
                }
            }
            else
            {
                if (IsSlotMatching(i))
                {
                    if (IsSlotMatching(i + 1))
                    {
                        continue;
                    }
                    index = i;
                    break;
                }
                else if (CurrentTime < LeaderboardScoresAsFloats[i])
                {
                    if(i == 0)
                    {
                        index = i;
                        break;
                    }

                    index = i - 1;
                    break;
                }
            }
        }
        return index;// Mathf.Clamp(index, 0, int.MaxValue);
    }

    protected override bool ColorSectionIfThisRun(int index)
    {
        base.ColorSectionIfThisRun(index);

        if (Submitted == false && DBChoice == 0)
        {
            return false;
        }

        int place = FindPlaceInLeaderboard();

        if (index == place)
        {
            return true;
        }
        return false;
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        IsShowing = true;
    }

    private void OnDisable()
    {
        IsShowing = false;
    }
}
