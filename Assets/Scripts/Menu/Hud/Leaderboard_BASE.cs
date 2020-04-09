using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class Leaderboard_BASE : MenuBase
{
    [Header("Leaderboard Fields"), SerializeField]
    List<TMPro.TMP_Text> LeaderboardPlacesText = new List<TMPro.TMP_Text>();
    [SerializeField]
    List<TMPro.TMP_Text> LeaderboardNamesText = new List<TMPro.TMP_Text>();
    [SerializeField]
    List<TMPro.TMP_Text> LeaderboardScoresText = new List<TMPro.TMP_Text>();
    [Header("Leaderboard Others"), SerializeField]
    Image CrownPic = null;
    [SerializeField, Tooltip("How many pages can be shown. 5 = 50 names will be possible to see.")]
    int MaxNumberOfPages = 5;
    [SerializeField]
    Button FirstSelected = null;
    [SerializeField]
    Color ColorOfThisRun = Color.red;
    Color DefaultColor = Color.black;

    int m_CurrentPage = -1;

    protected List<string> LeaderboardNames = new List<string>(); 
    protected List<string> LeaderboardScores = new List<string>();
    protected List<float> LeaderboardScoresAsFloats = new List<float>();

    [Header("Database")]
    protected Task<List<HighscoreData>> LoadLeaderboardTask = null;
    protected bool Submitted = false;
    protected float CurrentTime = -1.0f; //used for the leaderboard
    protected string UsedName = "";
    public static bool IsActive { get; protected set; }
    protected string PREF_KEY = "OnLeaderboardDBTypeToggle";
    protected int DBChoice = 0; //0 is firebase, 1 is local, potentially friends and stuff if we expand.

    protected override void Awake()
    {
        base.Awake();

        LoadSettings();
        Listen(PREF_KEY, OnChangeDBType);
        DefaultColor = LeaderboardNamesText[0].color;
        CanGoBackFromBaseMenu = false;
    }

    protected virtual void OnEnable()
    {
        StartCoroutine("SelectContinueButtonLater");
    }

    protected void LoadSettings()
    {
        DBChoice = PlayerPrefs.GetInt(PREF_KEY, 0);
    }

    protected override void Update()
    {
        base.Update();

        //Get info from the Leaderboard Database
        if (IsActive)
        {
            base.Update();
            if (LoadLeaderboardTask != null && LoadLeaderboardTask.IsCompleted)
            {
                if (LoadLeaderboardTask.IsFaulted == false)
                {
                    LeaderboardNames.Clear();
                    LeaderboardScores.Clear();
                    LeaderboardScoresAsFloats.Clear();

                    if (LoadLeaderboardTask.Result != null)
                    {
                        foreach (var entry in LoadLeaderboardTask.Result)
                        {
                            //store the data
                            LeaderboardNames.Add(string.Format("{0}", entry.Username));
                            LeaderboardScores.Add(string.Format("{0}", WorldTimeManager.GetTimeAsFormattedString(entry.Score)));
                            LeaderboardScoresAsFloats.Add(entry.Score);
                        }
                    }
                }
                else
                {
                    Debug.LogError(string.Format("Leaderboard request failed: {0}", LoadLeaderboardTask.Exception));
                }
                LoadLeaderboardTask = null;
                //fill the leaderboard
                UpdateTextByPage(0, true);
                OnLeaderboardFilled();
            }
        }
    }

    protected virtual void OnLeaderboardFilled() { }

    protected bool RequestLoadLeaderboard()
    {
        return RequestLoadLeaderboard(SceneManager.GetActiveScene().buildIndex);
    }

    public virtual bool RequestLoadLeaderboard(int LevelID)
    {
        if (DBChoice == 0)
        {
            return RequestLoadLeaderboardFB(LevelID);
        }
        else if (DBChoice == 1)
        {
            return RequestLoadLeaderboardLocal(LevelID);
        }
        return false;
    }

    public virtual bool RequestLoadLeaderboardFB(int LevelID)
    {
        if (LoadLeaderboardTask == null && FirebaseManager.Instance().IsUsableAndAvailable())
        {
            LoadLeaderboardTask = FirebaseManager.GetLeaderboardAsync(LevelID);
            return true;
        }
        return false;
    }

    public virtual bool RequestLoadLeaderboardLocal(int LevelID)
    {
        //Load Names and scores with info from LocalDB
        LeaderboardNames.Clear();
        LeaderboardScores.Clear();
        LeaderboardScoresAsFloats.Clear();

        bool success = false;
        float[] things = SQLManager.GetAllLocalHighscoresForScene(LevelID);

        //if we actually found the leaderboard display it
        if (things != null)
        {
            foreach (float thing in things)
            {
                //store the data
                LeaderboardNames.Add("LOCAL");
                LeaderboardScores.Add(string.Format("{0}", WorldTimeManager.GetTimeAsFormattedString(thing)));
                LeaderboardScoresAsFloats.Add(thing);
            }
            success = true;
        }
        //otherwise let them know it failed to load
        else
        {
            LeaderboardNames.Add("");
            LeaderboardScores.Add(string.Format(""));
            LeaderboardScoresAsFloats.Add(0.0f);
        }

        //update the leaderboard visually
        UpdateTextByPage(0, true);
        OnLeaderboardFilled();

        //return if we succeeded
        return success;
    }

    protected virtual void OnChangeDBType()
    {
        //Loads the DB type
        LoadSettings();
        RequestLoadLeaderboard();
    }

    /// <summary>
    /// This is where this will submit to FireBase and set the Panel back to the normal one.
    /// </summary>
    public void OnSubmit(string name)
    {
        if (DevConsole.Instance().HasBeenActivated == false)
        {
            if (!Submitted)
            {
                Submitted = true;
                UsedName = name;
                FirebaseManager.SaveHighScore(UsedName, CurrentTime, SceneManager.GetActiveScene().buildIndex);
                RequestLoadLeaderboard();
            }
        }

        OnClick(DevConsole.Instance().HasBeenActivated);
    }

    /// <summary>
    /// used for the editor
    /// </summary>
    /// <param name="page"></param>
    public void UpdateTextByPage(int page)
    {
        UpdateTextByPage(page, false);
    }

    public void UpdateTextByPage(int page, bool force)
    {
        if (SetPage(page) == false && force == false)
        {
            return;
        }

        ShowOrHideCrown();

        if (LoadLeaderboardTask != null)
        {
            return;
        }

        //set everything back to white
        //if this is not done and 
        for (int x = 0; x < LeaderboardPlacesText.Count; x++)
        {
            LeaderboardPlacesText[x].color = DefaultColor;
            LeaderboardNamesText[x].color = DefaultColor;
            LeaderboardScoresText[x].color = DefaultColor;
        }

        int minEntry = page * LeaderboardPlacesText.Count;
        int maxEntry = (page + 1) * LeaderboardPlacesText.Count;

        bool runAlreadyColored = false;

        int count = 0;
        for(int i = minEntry; i < maxEntry; i++)
        {
            //don't write the place for first place
            if(i != 0)
            {
                LeaderboardPlacesText[count].text = (i + 1).ToString();
            }
            else
            {
                LeaderboardPlacesText[count].text = "";
            }

            //if either of these columns is not long enough as where we are, leave
            if (LeaderboardNames.Count <= i || LeaderboardScores.Count <= i)
            {
                LeaderboardNamesText[count].text = "";
                LeaderboardScoresText[count].text = "";

                count++;
                continue;
            }

            LeaderboardNamesText[count].text = LeaderboardNames[i];
            LeaderboardScoresText[count].text = LeaderboardScores[i];

            //color this run
            if(ColorSectionIfThisRun(i) && !runAlreadyColored)
            {
                LeaderboardPlacesText[count].color = ColorOfThisRun;
                LeaderboardNamesText[count].color = ColorOfThisRun;
                LeaderboardScoresText[count].color = ColorOfThisRun;

                runAlreadyColored = true;
            }

            count++;
        }
    }

    protected virtual bool ColorSectionIfThisRun(int index) { return false; }

    public void NextPage()
    {
        UpdateTextByPage(m_CurrentPage + 1);
    }

    public void PreviousPage()
    {
        UpdateTextByPage(m_CurrentPage - 1);
    }

    bool SetPage(int num)
    {
        int temp = m_CurrentPage;

        m_CurrentPage = Mathf.Clamp(num, 0, MaxNumberOfPages - 1);

        if (temp == m_CurrentPage)
        {
            return false;
        }
        return true;
    }

    void ShowOrHideCrown()
    {
        //first page, show the crown and the background
        if(m_CurrentPage == 0)
        {
            CrownPic.enabled = true;
        }
        else
        {
            CrownPic.enabled = false;
        }
    }

    IEnumerator SelectContinueButtonLater()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(FirstSelected.gameObject);
        FirstSelected.Select();
    }
}
