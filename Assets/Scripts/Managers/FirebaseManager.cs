using System.Collections.Generic;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class FirebaseManager : Singleton<FirebaseManager>
{
    /// <summary>
    /// The Firebase Application object
    /// </summary>
    private FirebaseApp FirebaseApplication = null;

    /// <summary>
    /// The CrimeTrials database reference.
    /// </summary>
    private DatabaseReference CrimeTrialsDatabase = null;

    /// <summary>
    /// Whether or not the Application is registered.
    /// </summary>
    private volatile bool IsUsable = false;

    /// <summary>
    /// The result of the last Internet Connectivity test.
    /// </summary>
    public eInternetState IsOnline = eInternetState.Unavailable;

    /// <summary>
    /// Uploading scores is asynchronous so we only allow 1 thread of it at a time.
    /// </summary>
    private Task CurrentTask = null;

    /// <summary>
    /// Downloading leaderboard scores is asynchronous so we only allow 1 thread of it at a time.
    /// </summary>
    private Task<DataSnapshot> LeaderboardTask = null;

    // Pre-defined URIs that are mapped in the Firebase Database
    #region URIS

    private static readonly string SCORE_URI = "scores";
    private static readonly string USER_SCORES_URI = "user-scores";

    #endregion

    #region Run-Once

    /// <summary>
    /// This should be set true before running the game if you want FirebaseManager 
    /// to do run-once sequences after the database has been registered. Note: even if set true, it will set itself to false during 
    /// run-time. This is intentional. It DID work.
    /// </summary>
    [UnityEngine.Header("RunOnce"), UnityEngine.SerializeField, UnityEngine.Tooltip(
        "This should be set true before running the game if you want FirebaseManager " +
        "to do run-once sequences after the database has been registered. Note: even if set true, it will set itself to false during " +
        "run-time. This is intentional. It DID work.")]
    private bool HasRunOnceAfterInitialization = true;

    /// <summary>
    /// Whether or not the FirebaseManager should disable itself as soon as it finishes initializing.
    /// If HasRunOnceAfterInitialization is false then this will determine whether or not the FirebaseManager 
    /// is initialized with listening events active. By default this SHOULD be true because we only want FirebaseManager
    /// enabled during specific moments.
    /// </summary>
    [UnityEngine.SerializeField, UnityEngine.Tooltip(
        "Whether or not the FirebaseManager should disable itself as soon as it finishes initializing. " +
        "If HasRunOnceAfterInitialization is false then this will determine whether or not the FirebaseManager " +
        "is initialized with listening events active. By default this SHOULD be true because we only want FirebaseManager " +
        "enabled during specific moments.")]
    private bool StartDisabled = true;

    #endregion

    //Leaderboard variables.
    #region Leaderboard

    /// <summary>
    /// This list either the current top 10 entries in the leaderboard or null if a query hasn't completed yet.
    /// </summary>
    [UnityEngine.Header("Scores"), UnityEngine.Tooltip("Contains a list of up to NumberOfEntriesToQuery amount" +
        " of the top leaderboard entries up-to-date from the latest query")]
    public Dictionary<int, List<HighscoreData>> CurrentHighScores = new Dictionary<int, List<HighscoreData>>();

    [UnityEngine.Tooltip("The number of entries to request from Firebase when performing a leaderboard query.")]
    public int NumberOfEntriesToQuery = 10;

    #endregion

    #region Realtime Events

    public bool UsesRealtimeLeaderboardUpdates = true;
    private bool ScoreAddedReady = false;

    #endregion

    /// <summary>
    /// The last-known state of the Internet connection that the game is using.
    /// </summary>
    public enum eInternetState
    {
        Connected,
        Unconnected,
        Unavailable
    };

    protected override async void OnAwake()
    {
        DontDestroyOnLoad(this);
        await AttemptFirebaseConnectAsync();
    }

    /// <summary>
    /// Initialize the FirebaseManager.
    /// </summary>
    public static void Init()
    {
        Instance();
    }

    private void Update()
    {
        //This only runs a single time and then never again.
        if (HasRunOnceAfterInitialization == true && IsUsable)
        {
            HasRunOnceAfterInitialization = false;

            //Check initial Internet connection now that the database should be registered.
            StartCoroutine(CheckInternetConnection(OnInternetConnectionCheck));

            //Disable the FirebaseManager if requested.
            if (StartDisabled)
            {
                enabled = false;
            }

        }
    }

    /// <summary>
    /// Is the Database usable and Internet verified?
    /// </summary>
    /// <returns></returns>
    public bool IsUsableAndAvailable()
    {
        return IsUsable && IsOnline != eInternetState.Unavailable;
    }

    /// <summary>
    /// Whenever we are enabled we need to register the callback to the node we care about listening to.
    /// </summary>
    private void OnEnable()
    {
        if (CrimeTrialsDatabase != null && IsUsable)
        {
            //Populate the leaderboard.
            GetLeaderboardAsync().Wait();

            if (UsesRealtimeLeaderboardUpdates && ScoreAddedReady == false)
            {
                CrimeTrialsDatabase.Database.GetReference(SCORE_URI).OrderByChild("score").ChildAdded += HandleScoreAdded;
                ScoreAddedReady = true;
            }
        }
    }

    /// <summary>
    /// Whenever we are disabled we need to de-register the callback to the node we care about listening to so we stop getting events.
    /// </summary>
    private void OnDisable()
    {
        if (CrimeTrialsDatabase != null && IsUsable)
        {
            if (UsesRealtimeLeaderboardUpdates && ScoreAddedReady == true)
            {
                CrimeTrialsDatabase.Database.GetReference(SCORE_URI).OrderByChild("score").ChildAdded -= HandleScoreAdded;
                ScoreAddedReady = false;
            }
        }
    }

    public async Task AttemptFirebaseConnectAsync()
    {
        //Only initialize once if we're good to go.
        if (IsUsable != true)
        {
            //Check dependencies
            await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(async task =>
            {
                var dependencyStatus = task.Result;

                //It passes the dependency test
                if (dependencyStatus == DependencyStatus.Available)
                {

                    //Set the app and the editor url
                    FirebaseApplication = FirebaseApp.DefaultInstance;
                    FirebaseApplication.SetEditorDatabaseUrl("https://crimetrials.firebaseio.com");

                    //Set the database
                    CrimeTrialsDatabase = FirebaseDatabase.DefaultInstance.RootReference;
                    //CrimeTrialsDatabase.Database.SetPersistenceEnabled(true);
                    CrimeTrialsDatabase.KeepSynced(true);

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    IsUsable = true;

                    //notify user
                    UnityEngine.Debug.Log("Firebase available");

                    //We'll populate the leaderboard immediately to get things going.
                    await GetLeaderboardAsync();

                    //Edge-case scenario here. If we are actually starting enabled we need to do what OnEnable does since
                    //the first call to OnEnable when the game starts will be useless...
                    if (StartDisabled == false && UsesRealtimeLeaderboardUpdates)
                    {
                        OnEnable();
                    }
                }
                else
                {
                    // Firebase Unity SDK is not safe to use here.
                    UnityEngine.Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    IsUsable = false;
                }
            });
        }
    }

    /// <summary>
    /// Have the local DB go offline, storing new entries locally
    /// until a connection is re-established.
    /// </summary>
    public void GoOffline()
    {
        CrimeTrialsDatabase.Database.GoOffline();
    }

    /// <summary>
    /// Re-establish connection to the FirebaseDatabase.
    /// </summary>
    public void GoOnline()
    {
        CrimeTrialsDatabase.Database.GoOnline();
    }

    /// <summary>
    /// simple check to Google.com to see if we have an internet connection. 
    /// </summary>
    /// <param name="syncResult">Callback function that holds our internet test result.</param>
    /// <returns></returns>
    public static IEnumerator CheckInternetConnection(System.Action<bool> syncResult)
    {
        const string echoServer = "http://google.com";

        bool result;
        using (var request = UnityWebRequest.Head(echoServer))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();
            result = !request.isNetworkError && !request.isHttpError && request.responseCode == 200;
        }
        syncResult(result);
    }

    /// <summary>
    /// Sets us online/offline based on resulting internet test.
    /// </summary>
    /// <param name="result">Result from testing internet connection.</param>
    public void OnInternetConnectionCheck(bool result)
    { 
        if (result == true && IsUsable)
        {
            GoOnline();
            IsOnline = eInternetState.Connected;
        }
        else
        {
            GoOffline();
            IsOnline = eInternetState.Unconnected;
        }
    }

    public static void SaveHighScore(string userID, float scoreID, int levelID)
    {
        FirebaseManager instance = Instance();

        if (instance)
        {
            if (instance.IsUsable == true)
            {
                if (instance.CurrentTask == null)
                {
                    // Create new entry at /user-scores/$userid/$scoreid and at
                    // /leaderboard/$scoreid simultaneously
                    string key = instance.CrimeTrialsDatabase.Child(SCORE_URI).Child(levelID.ToString()).Push().Key;
                    HighscoreData entry = new HighscoreData(userID, scoreID, levelID);
                    Dictionary<string, object> entryValues = entry.ToDictionary();

                    Dictionary<string, object> childUpdates = new Dictionary<string, object>
                    {
                        [string.Format("/{0}/{1}/{2}", SCORE_URI, levelID.ToString(), key)] = entryValues,
                        [string.Format("/{0}/{1}/{2}/{3}", USER_SCORES_URI, userID, levelID.ToString(), key)] = entry.Score
                    };

                    instance.CurrentTask = instance.CrimeTrialsDatabase.UpdateChildrenAsync(childUpdates);

                    //Callback function when the task completes.
                    instance.CurrentTask.ContinueWith((result) =>
                    {
                        if (result.IsCompleted)
                        {
                            if (result.IsFaulted)
                            {
                                UnityEngine.Debug.LogWarning(string.Format("Firebase callback failed. \nReason: {0}", result.Exception));
                            }

                            instance.CurrentTask = null;
                        }
                    } );
                }
            }
        }
    }

    /// <summary>
    /// Request the leaderboard from the database.
    /// </summary>
    public static async Task<List<HighscoreData>> GetLeaderboardAsync(int levelID = 0)
    {
        FirebaseManager instance = Instance();
        List<HighscoreData> leaders = null;

        if (instance)
        {
            if (instance.IsUsable)
            {
                if (instance.LeaderboardTask == null)
                {
                    //create the task
                    instance.LeaderboardTask = instance.CrimeTrialsDatabase.Database.GetReference(string.Format("scores/{0}", levelID.ToString())).OrderByChild("score").LimitToFirst(50).GetValueAsync();

                    //add a callback to run when the task completes that cleans up the task and sends the data to the parsing function
                    await instance.LeaderboardTask.ContinueWith((result) =>
                   {
                       if (result.IsCompleted)
                       {
                           if (result.IsFaulted == false)
                           {
                               leaders = instance.OnLeaderboardReceived(result.Result);
                           }
                           else
                           {
                               UnityEngine.Debug.LogWarning(string.Format("Leaderboard fetch failed: {0}", result.Exception));
                           }
                       }
                   });
                }
            }
        }

        instance.CurrentHighScores[levelID] = leaders;
        return leaders;
    }

    private List<HighscoreData> OnLeaderboardReceived(DataSnapshot result)
    {
        LeaderboardTask = null;
        return result.Children.Select(record => CreateHighScoreFromRecord(record)).ToList();
    }

    /// <summary>
    /// Create a score using a google DataSnapshot and send it back.
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    private HighscoreData CreateHighScoreFromRecord(DataSnapshot record)
    {
        if (record == null)
        {
            UnityEngine.Debug.LogWarning("Null DataSnapshot record in FirebaseManager.CreateScoreFromRecord.");
            return null;
        }
        if (record.Child("score").Exists)
        {
            return new HighscoreData(record);
        }

        UnityEngine.Debug.LogWarning("Invalid record format in UserScore.CreateScoreFromRecord.");
        return null;
    }

    /// <summary>
    /// Callback function for what the database changes so that the leader board can be kept up-to-date.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleScoreAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            UnityEngine.Debug.LogError(args.DatabaseError.Message);
            return;
        }

        HighscoreData data = CreateHighScoreFromRecord(args.Snapshot);
        CompareScoreToKnownScores(data);
    }

    /// <summary>
    /// Logically determine if the supplied NewScore will replace a value in the current leaderboard list.
    /// </summary>
    /// <param name="NewScore"></param>
    private void CompareScoreToKnownScores(HighscoreData NewScore)
    {
        if (CurrentHighScores.Any())
        {
            //check what index we would insert the new score if it fits
            int indexToReplace = -1;
            for (int i = CurrentHighScores.Count - 1; i >= 0; i--)
            {
                //The new score is smaller which means it can fit into our leaderboard list.
                if (NewScore.Score < CurrentHighScores[NewScore.LevelID][i].Score)
                {
                    indexToReplace = i;
                }
                //The new score is bigger so at this point we can start going through the list.
                else
                {
                    break;
                }
            }

            //If the index is a valid number then we can insert it.
            if (indexToReplace != -1)
            {
                CurrentHighScores[NewScore.LevelID].Insert(indexToReplace, NewScore);
                CurrentHighScores[NewScore.LevelID].RemoveAt(CurrentHighScores.Count - 1);
            }
        }
       
    }
}

/// <summary>
/// Data chunk to be uploaded to Firebase NoSQL.
/// </summary>
[System.Serializable]
public class HighscoreData
{
    public HighscoreData() { }

    /// <summary>
    /// Generate a HighscoreData chunk using a google-generated DataSnapshot
    /// </summary>
    /// <param name="DataRecord"></param>
    public HighscoreData(DataSnapshot DataRecord)
    {
        Username = DataRecord.Child("username").Value.ToString();

        if (float.TryParse(DataRecord.Child("score").Value.ToString(), out float score) == true)
        {
            Score = score;
        }
        else
        {
            UnityEngine.Debug.LogWarning("Parsing of score failed.");
            Score = float.MaxValue;
        }
    }

    /// <summary>
    /// Generate a HighscoreData chunk using system-generated variables
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="score"></param>
    public HighscoreData(string userID, float score, int levelID)
    {
        Username = userID;
        Score = score;
        LevelID = levelID;
    }

    /// <summary>
    /// Prep the data to be sent out in dictionary format for easy node read.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>
        {
            ["username"] = Username,
            ["score"] = Score,
            ["level"] = LevelID
        };

        return result;
    }

    public string Username = "";
    public float Score = 0.0f;
    public int LevelID = 0;
}
