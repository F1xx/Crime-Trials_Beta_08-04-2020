using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class SQLManager : Singleton<SQLManager>
{

    /// <summary>
    /// The raw connection to the database.
    /// </summary>
    private IDbConnection LocalDatabase = null;

    /// <summary>
    /// Current player run. Stays up-to-date with the current scene but is useless inside any menus or loading screen.
    /// </summary>
    private LevelHeats CurrentPlayerRunForCurrentScene;

    /// <summary>
    /// Current player run. Stays up-to-date with the current scene but is useless inside any menus or loading screen.
    /// </summary>
    private LevelHeats CurrentPlayerSegmentsForCurrentScene;

    /// <summary>
    /// Best player data for a scene. Is automatically generated on scene change.
    /// </summary>
    private LevelHeats BestRunForCurrentScene;

    /// <summary>
    /// Best player segments for a scene. Is automatically generated on scene change.
    /// </summary>
    private LevelHeats BestSegmentsForCurrentScene;

    /// <summary>
    /// All unlocks loaded. Keys are the unlock group paired with their lock status.
    /// </summary>
    public Dictionary<string, List<bool>> LoadedUnlocks;

    /// <summary>
    /// If connection failed this will prevent any null culls.
    /// </summary>
    public bool IsAvailable = false;

    /// <summary>
    /// This key is the value used inside every SQLite table to denote the final time for the level.
    /// </summary>
    private static readonly string LevelScoreKey = "FinalScore";

    /// <summary>
    /// The directory that the database is located in
    /// </summary>
    private static readonly string UsersDirectory = "CrimeTrials";

    /// <summary>
    /// The name of the database file.
    /// </summary>
    private static readonly string CrimeTrialsDatabase = "Save.sav";

    /// <summary>
    /// An internal mapping dictionary for loading by index
    /// </summary>
    private Dictionary<int, string> SceneIndexDictionary;

    /// <summary>
    /// Count of checkpoints in the loaded scene.
    /// </summary>
    private int CurrentSceneCheckpointCount = 0;

    protected override void OnAwake()
    {
        DontDestroyOnLoad(this);
      
        InitializeLocalDatabase();
        
        Listen("OnCheckPointReached", OnCheckpointReachedEvent);
        Listen("OnLevelComplete", OnLevelCompletedEvent);
        Listen("OnUnlockArm", UnlockSomething);

        CurrentPlayerSegmentsForCurrentScene = new LevelHeats(false);
        CurrentPlayerRunForCurrentScene = new LevelHeats(false);
        BestRunForCurrentScene = new LevelHeats(false);
        BestSegmentsForCurrentScene = new LevelHeats(false);
    }

    /// <summary>
    /// Issue a query using a raw string. Only use this function specifically if you know what you're doing.
    /// </summary>
    /// <param name="query">The query. Must be formatted to SQLite standards or it will fail.</param>
    public bool ExecuteCommand(string query, out IDataReader Reader)
    {
        IDbCommand dbcmd = LocalDatabase.CreateCommand();
        dbcmd.CommandText = query;
        try
        {
            Reader = dbcmd.ExecuteReader();
        }
        catch (System.Exception e)
        {
            Debug.LogError(string.Format("SQL command error: {0}", e.Message));
            Reader = null;
            return false;
        }


        return true;
    }

    #region Create Functions

    /// <summary>
    /// Called on start as well as manually with dev console. Attempts to connect the database.
    /// </summary>
    public static void InitializeLocalDatabase()
    {
        SQLManager instance = Instance();

        if (instance)
        {
            instance.LoadedUnlocks = new Dictionary<string, List<bool>>();
            instance.SceneIndexDictionary = new Dictionary<int, string>();

            string usersFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string crimetrialsPath = usersFolderPath + "/" + UsersDirectory;
            string fullDatabasePath = crimetrialsPath + "/" + CrimeTrialsDatabase;
            bool databaseExists = true;

            //First we create the database if it doesn't exist.
            {            
                if (!Directory.Exists(crimetrialsPath))
                {
                    databaseExists = false;
                    Directory.CreateDirectory(crimetrialsPath);
                }

                if (!databaseExists || !File.Exists(fullDatabasePath))
                {
                    instance.CreateDatabase();
                }
            }

            //connect to our database.
            if (instance.LocalDatabase == null)
            {
                string conn = "URI=file:" + fullDatabasePath; //Path to database.

                instance.LocalDatabase = new SqliteConnection(conn);
                instance.LocalDatabase.Open(); //Open connection to the database.
            }

            //If the connection failed for any reason we got to abort.
            if (instance.LocalDatabase.State != ConnectionState.Open)
            {
                Debug.LogError("SQLManager failed to load the CrimeTrials local database.");
                instance.IsAvailable = false;

                instance.LocalDatabase.Close();
                instance.LocalDatabase = null;
            }
            else
            {
                Debug.Log("CrimeTrials local database available.");
                instance.IsAvailable = true;
            }

        }

        //Load the unlocks table if we connected successfully.
        if (instance.IsAvailable)
        {
            instance.LoadUnlocks();
            instance.LoadSceneDictionary();
        }
    }

    /// <summary>
    /// Create an entirely fresh database. This should only ever be called by Awake if a database doesnt exist or the user chose to wipe all data from their game and its being rebuilt.
    /// </summary>
    private void CreateDatabase()
    {
        string usersFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string crimetrialsPath = usersFolderPath + "/" + UsersDirectory;
        string fullDatabasePath = crimetrialsPath + "/" + CrimeTrialsDatabase;

        SqliteConnection.CreateFile(fullDatabasePath);

        string conn = "URI=file:" + fullDatabasePath; //Path to database.

        LocalDatabase = new SqliteConnection(conn);
        LocalDatabase.Open(); //Open connection to the database.

        //If the connection failed for any reason we got to abort.
        if (LocalDatabase.State != ConnectionState.Open)
        {
            LocalDatabase.Close();
            LocalDatabase = null;

            return;
        }

        //Create unlocks table query
        string CreateUnlocksTableQuery = "CREATE TABLE \"Unlocks\"(" +
                                        "\"Variant_1\" INTEGER NOT NULL DEFAULT 1," +
                                        "\"Variant_2\" INTEGER NOT NULL DEFAULT 0," +
                                        "\"Variant_3\" INTEGER NOT NULL DEFAULT 0," +
                                       "\"Variant_4\" INTEGER NOT NULL DEFAULT 0," +
                                        "\"Variant_5\" INTEGER NOT NULL DEFAULT 0," +
                                        "\"UnlockGroup\"   TEXT NOT NULL DEFAULT 'Arm'," +
                                        "PRIMARY KEY(\"UnlockGroup\"));";

        //Execute and create it.
        bool successfulUnlocksTableCreation = ExecuteCommand(CreateUnlocksTableQuery, out _);

        
        if (successfulUnlocksTableCreation)
        {
            //Create the default unlock fields
            string ArmUnlocksQuery = "INSERT INTO \"Unlocks\" (Variant_1, Variant_2, Variant_3, Variant_4, Variant_5, UnlockGroup) VALUES (1,0,0,0,0,\"Arm\");";
            successfulUnlocksTableCreation = ExecuteCommand(ArmUnlocksQuery, out _);
        }

        //If the unlocks table failed to create this database is fucked.
        if (successfulUnlocksTableCreation == false)
        {
            Debug.LogError("Database failed to generate itself.");
            LocalDatabase.Close();
            LocalDatabase = null;
        }
        else
        {
            Debug.Log("CrimeTrials database created at location " + fullDatabasePath + ".");
        }
    }

    /// <summary>
    /// Attempts to create the Run and Segments table for a level on the database using scene data matching the supplied scene name.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool CreateLevelTables(string sceneName)
    {
        //first thing, we need the build index of this scene
        //if this step fails then the table aint getting made

        Scene rawScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);

        if (rawScene == null)
        {
            Debug.LogError("TableExistsWithinDatabase error: " + name + " does not exist within SceneManager.");
            return false;
        }

        int buildID = rawScene.buildIndex;
        int totalCheckpoints = 0;

        GameObject[] rawObjectData = rawScene.GetRootGameObjects();

        foreach (GameObject obj in rawObjectData)
        {
            RespawnPoint[] respawnPoints = obj.GetComponentsInChildren<RespawnPoint>();

            if (respawnPoints != null)
            {
                totalCheckpoints += respawnPoints.Length;
            }
        }

        string createTableRunsQuery = string.Format("CREATE TABLE \"{0}:{1}\" (", buildID, sceneName);
        string createTableSegmentsQuery = string.Format("CREATE TABLE \"{0}:{1}\" (", buildID, sceneName + "-Segments");

        for (int i = 0; i < totalCheckpoints; i++)
        {
            createTableRunsQuery += string.Format("\"Checkpoint{0}\" REAL,", (i + 1));
            createTableSegmentsQuery += string.Format("\"Checkpoint{0}\" REAL,", (i + 1));
        }

        createTableRunsQuery += "\"FinalScore\" REAL);";
        createTableSegmentsQuery += "\"FinalScore\" REAL);";

        bool successfulLevelRunsTable = ExecuteCommand(createTableRunsQuery, out _);
        bool successfulLevelSegmentsTable = ExecuteCommand(createTableSegmentsQuery, out _);

        if (successfulLevelRunsTable && successfulLevelSegmentsTable)
        {
            SceneIndexDictionary[buildID] = string.Format("{0}:{1}", buildID, sceneName);
            return true;
        }

        return false;
    }

    #endregion

    #region Load Functions

    /// <summary>
    /// Load the tables in the database into a managed dictionary for easy table-lookups.
    /// </summary>
    private void LoadSceneDictionary()
    {

        if (IsAvailable && Instance().ExecuteCommand("SELECT name FROM sqlite_master WHERE type = 'table'", out IDataReader reader))
        {
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.IsDBNull(i) != true)
                    {
                        string output = reader.GetString(i);

                        //Screen out the unlocks and segments tables.
                        if (output.Contains("-Segments") || output == "Unlocks")
                        {
                            continue;
                        }
                        
                        int ID = System.Convert.ToInt32(output.Substring(0, output.IndexOf(@":")));
                        SceneIndexDictionary[ID] = output;
                    }
                }

            }

            reader.Close();
        }

    }

    /// <summary>
    /// Load all unlock data known in the local game.
    /// </summary>
    private void LoadUnlocks()
    {
        if (Instance().ExecuteCommand("SELECT * FROM Unlocks", out IDataReader reader))
        {
            while (reader.Read())
            {
                string key = reader.GetString(reader.FieldCount - 1);

                List<bool> val = new List<bool>();

                for (int i = 0; i < reader.FieldCount - 1; i++)
                {
                    val.Add(reader.GetBoolean(i));
                }

                LoadedUnlocks.Add(key, val);
            }

            reader.Close();
        }
    }

    /// <summary>
    /// Loads the best times for heat data in the supplied level. Returns whether it succeeded or not.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool LoadSceneRunData(string sceneName)
    {
        //safety check to ensure this scene name is actually legit
        if (SceneManager.GetSceneByName(sceneName) == null)
        {
            Debug.LogWarning(string.Format("Tried to load heat data for a level that doesnt exist. Passed name: {0}", sceneName));
            return false;
        }

        if (sceneName == "LoadingScene" || sceneName == "MainMenu")
        {
            return false;
        }

        SQLManager instance = Instance();

        if (instance.IsAvailable && instance.TableExistsWithinDatabase(sceneName))
        {

            GetTableFormattedSceneName(sceneName, out string FormattedName);
            if (instance.ExecuteCommand(string.Format("SELECT sql FROM sqlite_master WHERE tbl_name = '{0}' AND type = 'table'", FormattedName), out IDataReader reader))
            {

                while (reader.Read())
                {
                    CurrentSceneCheckpointCount = GetSubstringOccurenceCount(reader.GetString(0), "Checkpoint");
                }
                reader.Close();

                LoadBestRecordedRunForScene(FormattedName);
                LoadBestPossibleRunForScene(FormattedName);


                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Loads the best run for a given scene.
    /// </summary>
    /// <param name="name"></param>
    private void LoadBestRecordedRunForScene(string name)
    {
        BestRunForCurrentScene = new LevelHeats(name);

        SQLManager instance = Instance();
        IDbCommand dbcmd;
        IDataReader reader;

        string rawCommand = "SELECT ";

        for (int i = 0; i < CurrentSceneCheckpointCount; i++)
        {
            rawCommand += string.Format("Checkpoint{0},", (i + 1).ToString());
        }

        rawCommand += string.Format("{1} from '{0}' ORDER BY {1} ASC LIMIT 1", name, LevelScoreKey);

        dbcmd = instance.LocalDatabase.CreateCommand();
        dbcmd.CommandText = rawCommand;

        reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            for (int i = 0; i < CurrentSceneCheckpointCount; i++)
            {
                float parsedValue = -1;
                if (reader[i].GetType() != typeof(System.DBNull))
                    parsedValue = reader.GetFloat(i);

                BestRunForCurrentScene.CheckpointTimepoints[string.Format("Checkpoint{0}", (i + 1).ToString())] = parsedValue;
            }

            float finalVal = reader.GetFloat(CurrentSceneCheckpointCount);
            BestRunForCurrentScene.CheckpointTimepoints[LevelScoreKey] = finalVal;
        }

        //This only happens if the table is completely empty. In which case we'll going to stuff it with fake values so we don't get false red flag errors later.
        if (BestRunForCurrentScene.CheckpointTimepoints.Count == 0)
        {
            for (int i = 0; i < CurrentSceneCheckpointCount; i++)
            {
                BestRunForCurrentScene.CheckpointTimepoints[string.Format("Checkpoint{0}", (i + 1).ToString())] = -1;
            }
            BestRunForCurrentScene.CheckpointTimepoints[LevelScoreKey] = -1;
        }

        reader.Close();
        dbcmd.Dispose();
    }

    /// <summary>
    /// Loads the best segments for a given scene.
    /// </summary>
    /// <param name="name"></param>
    private void LoadBestPossibleRunForScene(string name)
    {
        string formattedName = name + "-Segments";
        BestSegmentsForCurrentScene = new LevelHeats(formattedName);

        SQLManager instance = Instance();
        IDbCommand dbcmd;
        IDataReader reader;


        //Get all checkpoint column values
        for (int i = 0; i < CurrentSceneCheckpointCount; i++)
        {
            dbcmd = instance.LocalDatabase.CreateCommand();
            dbcmd.CommandText = string.Format("SELECT Checkpoint{0} from '{1}' ORDER BY Checkpoint{0} ASC LIMIT 1", (i + 1).ToString(), formattedName);
            reader = dbcmd.ExecuteReader();

            float checkpointVal = -1;

            while (reader.Read())
            {
                if (reader[0].GetType() != typeof(System.DBNull))
                    checkpointVal = reader.GetFloat(0);
            }

            BestSegmentsForCurrentScene.CheckpointTimepoints[string.Format("Checkpoint{0}", (i + 1).ToString())] = checkpointVal;

            dbcmd.Dispose();
        }

        //Get the final score value
        {
            dbcmd = instance.LocalDatabase.CreateCommand();
            dbcmd.CommandText = string.Format("SELECT {0} from '{1}' ORDER BY {0} ASC LIMIT 1", LevelScoreKey, formattedName);
            reader = dbcmd.ExecuteReader();

            float finalScoreValue = -1;

            while (reader.Read())
            {
                finalScoreValue = reader.GetFloat(0);
            }

            BestSegmentsForCurrentScene.CheckpointTimepoints[LevelScoreKey] = finalScoreValue;

            dbcmd.Dispose();
        }


        reader.Close();
    }

    #endregion

    #region Save Functions


    /// <summary>
    /// Submits the run data for a scene to the leaderboard in the active scene file.
    /// </summary>
    private bool SubmitRunData()
    {

        if (CurrentPlayerRunForCurrentScene.IsValid == false)
        {
            Debug.LogWarning(string.Format("Warning: SQLManager attempted to Submit Level Data with corrupted data. Level: {0}", CurrentPlayerRunForCurrentScene.LevelName));
            return false;
        }

        string keysQuery = "";
        string valuesQuery = "";

        for (int i = 0; i < CurrentPlayerRunForCurrentScene.CheckpointTimepoints.Count; i++)
        {
            keysQuery += string.Format("{0},", CurrentPlayerRunForCurrentScene.CheckpointTimepoints.ElementAt(i).Key);
            valuesQuery += string.Format("{0},", CurrentPlayerRunForCurrentScene.CheckpointTimepoints.ElementAt(i).Value);
        }

        keysQuery = keysQuery.Remove(keysQuery.Length - 1);
        valuesQuery = valuesQuery.Remove(valuesQuery.Length - 1);

        return ExecuteCommand(string.Format("INSERT into '{0}' ({1}) VALUES ({2})", CurrentPlayerRunForCurrentScene.LevelName, keysQuery, valuesQuery), out _);
    }

    /// <summary>
    /// Submits a new entry to the segments table (used for calculating best possible run).
    /// </summary>
    /// <returns></returns>
    private bool SubmitSegmentData()
    {
        if (CurrentPlayerSegmentsForCurrentScene.IsValid == false)
        {
            Debug.LogWarning(string.Format("Warning: SQLManager attempted to Submit Segment Data with corrupted data. Level: {0}", CurrentPlayerSegmentsForCurrentScene.LevelName));
            return false;
        }

        string keysQuery = "";
        string valuesQuery = "";

        for (int i = 0; i < CurrentPlayerSegmentsForCurrentScene.CheckpointTimepoints.Count; i++)
        {
            keysQuery += string.Format("{0},", CurrentPlayerSegmentsForCurrentScene.CheckpointTimepoints.ElementAt(i).Key);
            valuesQuery += string.Format("{0},", CurrentPlayerSegmentsForCurrentScene.CheckpointTimepoints.ElementAt(i).Value);
        }

        keysQuery = keysQuery.Remove(keysQuery.Length - 1);
        valuesQuery = valuesQuery.Remove(valuesQuery.Length - 1);

        return ExecuteCommand(string.Format("INSERT into '{0}' ({1}) VALUES ({2})", CurrentPlayerSegmentsForCurrentScene.LevelName, keysQuery, valuesQuery), out _);
    }

    /// <summary>
    /// Flag an unlock as true that exists with a particular key. Use the table-specific index. 
    /// </summary>
    /// <param name="key">The key as known internally by the Unlocks table.</param>
    /// <param name="index">The unlock index.</param>
    public void UnlockSomething(string key, int index)
    {
        if (LoadedUnlocks[key][index - 1] == false)
        {
            string tableField = string.Format("Variant_{0}", index);

            string sqlQuery = string.Format("UPDATE Unlocks SET {0}=1 WHERE UnlockGroup='{1}'", tableField, key);

            if (Instance().ExecuteCommand(sqlQuery, out _))
            {
                Debug.Log(string.Format("Thing unlocked. Group:{0}, index:{1}", key, index));
                LoadedUnlocks[key][index - 1] = true;
            }
            else
            {
                Debug.LogWarning(string.Format("Thing failed to unlock. Group:{0}, index:{1}", key, index));
            }
        }
    }

    public void UnlockSomething(EventParam param)
    {
        OnUnlockParam realParam = (OnUnlockParam)param;
        UnlockSomething(realParam.Key, realParam.Index);
    }

    #endregion

    #region Reset Functions

    /// <summary>
    /// This function will wipe a Leaderboard that matches with a particular level. By default all tables in the local database have a name that matches the scene name so not format is required.
    /// </summary>
    /// <param name="sceneName"></param>
    public static bool ResetLocalLeaderboard(string sceneName)
    {
        //safety check to ensure this scene name is actually legit
        if (SceneManager.GetSceneByName(sceneName) == null)
        {
            Debug.LogWarning(string.Format("Tried to reset a leaderboard that probably doesnt exist. Passed name: {0}", sceneName));
            return false;
        }

        SQLManager instance = Instance();

        if (instance && instance.IsAvailable)
        {
            bool val, valTwo;
            val = instance.ExecuteCommand(string.Format("DELETE FROM '{0}'", sceneName), out _);
            valTwo = instance.ExecuteCommand(string.Format("DELETE FROM '{0}'", sceneName + "-Segments"), out _);

            if (val && valTwo)
                return true;
        }

        return false;
    }

    /// <summary>
    /// This function will wipe a Leaderboard that matches with a particular level. By default all tables in the local database have a name that matches the scene name so not format is required.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool ResetLocalLeaderboard(int id)
    {
        if (IsConnected())
        {
            string name = Instance().GetSceneByIndex(id);

            if (name != null)
            {
                return ResetLocalLeaderboard(name);
            }
        }

        return false;
    }

    /// <summary>
    /// This function will wipe every local leaderboard clean of data. Use with caution.
    /// </summary>
    public static void ResetEntireDatabase()
    {
        SQLManager instance = Instance();

        if (instance.IsAvailable)
        {
            instance.LocalDatabase.Close();
            instance.LocalDatabase = null;
            instance.IsAvailable = false;
        }

        string usersFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string crimetrialsPath = usersFolderPath + "/" + UsersDirectory;
        string fullDatabasePath = crimetrialsPath + "/" + CrimeTrialsDatabase;

        if (File.Exists(fullDatabasePath))
            File.Delete(fullDatabasePath);

        PlayerPrefs.HasKey("");

        InitializeLocalDatabase();
    }

    #endregion

    #region Getters

    /// <summary>
    /// Internal check to see if the database has a table matching with a supplied id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private string GetSceneByIndex(int id)
    {
        if (IsAvailable && SceneIndexDictionary.ContainsKey(id))
        {
            return SceneIndexDictionary[id];
        }

        return null;
    }

    /// <summary>
    /// The database uses a slightly different naming convention for scenes. This will safely convert them and is safety-checked for formatted names.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="formattedName"></param>
    /// <returns></returns>
    private bool GetTableFormattedSceneName(string name, out string formattedName)
    {

        foreach (KeyValuePair<int, string> pair in SceneIndexDictionary)
        {
            if (name == pair.Value)
            {
                formattedName = name;
                return true;
            }
        }

        Scene scene = SceneManager.GetSceneByName(name);

        if (scene.buildIndex != -1)
        {
            formattedName = string.Format("{0}:{1}", scene.buildIndex, name);
            return true;
        }
        else
        {
            formattedName = "";
            Debug.LogError("TableExistsWithinDatabase error: " + name + " does not exist within SceneManager.");
            return false;
        }
    }

    /// <summary>
    /// Return's requested heat data for a particular checkpoint index in the current scene. Pass -1 for final score index. Returns -1 if no value was found.
    /// </summary>
    /// <param name="Index"></param>
    /// <returns></returns>
    public static float GetRunCheckpointDataFor(int Index)
    {
        SQLManager instance = Instance();

        if (instance.TableExistsWithinDatabase(SceneManager.GetActiveScene().name))
        {
            if (Index == 0)
            {
                return -1;
            }

            if (instance && instance.IsAvailable)
            {
                string IndexAsKey = Index != -1 ? string.Format("Checkpoint{0}", Index) : LevelScoreKey;

                if (instance.BestRunForCurrentScene.CheckpointTimepoints.ContainsKey(IndexAsKey) == true)
                {
                    return instance.BestRunForCurrentScene.CheckpointTimepoints[IndexAsKey];
                }
                else
                {
                    Debug.LogError(string.Format("SQL Error: run data was requested from a checkpoint where no data existed. Value passed: {0}", Index));
                }
            }           
        }

        return -1;
    }

    /// <summary>
    /// Return's requested segment data for a particular checkpoint index in the current scene. Pass -1 for final score index. Returns -1 if no value was found.
    /// </summary>
    /// <param name="Index"></param>
    /// <returns></returns>
    public static float GetSegmentCheckpointDataFor(int Index)
    {
        SQLManager instance = Instance();

        if (instance.TableExistsWithinDatabase(SceneManager.GetActiveScene().name + "-Segments"))
        {
            if (Index == 0)
            {
                Debug.LogWarning("WARNING: Checkpoint index has not been set in the scene. Please set it to get proper checkpoint data.");
                return -1;
            }

            string IndexAsKey = Index != -1 ? string.Format("Checkpoint{0}", Index) : LevelScoreKey;

            if (instance.BestSegmentsForCurrentScene.CheckpointTimepoints.ContainsKey(IndexAsKey) == true)
            {
                return instance.BestSegmentsForCurrentScene.CheckpointTimepoints[IndexAsKey];
            }
            else
            {
                Debug.LogError(string.Format("SQL Error: segment data was requested from a checkpoint where no data existed. Value passed: {0}", Index));
            }
        }

        return -1;
    }

    /// <summary>
    /// Get the high score for this run. Intended to be used ONLY when the scene ends.
    /// </summary>
    /// <returns></returns>
    public static float GetCurrentRunHighscore()
    {
        if (IsConnected())
        {
            SQLManager instance = Instance();

            if (instance.CurrentPlayerRunForCurrentScene.IsValid)
            {
                if (instance.CurrentPlayerRunForCurrentScene.CheckpointTimepoints.ContainsKey("FinalScore"))
                {
                    return instance.CurrentPlayerRunForCurrentScene.CheckpointTimepoints["FinalScore"];
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Get high score data from the local database for a scene name.
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public static float[] GetAllLocalHighscoresForScene(string scene)
    {
        if (IsConnected())
        {
            SQLManager instance = Instance();

            if (instance.GetTableFormattedSceneName(scene, out string formattedName))
            {
                if (instance.TableExistsWithinDatabase(scene) && instance.ExecuteCommand(string.Format("SELECT FinalScore FROM '{0}' ORDER BY FinalScore LIMIT 50", formattedName), out IDataReader reader))
                {
                    List<float> allHighscoreResults = new List<float>();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader[i].GetType() != typeof(System.DBNull))
                                allHighscoreResults.Add(reader.GetFloat(i));
                        }
                    }

                    reader.Close();
                    return allHighscoreResults.ToArray();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Get high score data from the local database for a scene build index.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static float[] GetAllLocalHighscoresForScene(int id)
    {
        string sceneName = Instance().GetSceneByIndex(id);

        if (sceneName != null)
        {
            return GetAllLocalHighscoresForScene(sceneName);
        }

        return null;
    }

    /// <summary>
    /// How often does the pattern appear in a string?
    /// </summary>
    /// <param name="text">The subject text.</param>
    /// <param name="pattern">The pattern to look for.</param>
    /// <returns></returns>
    public static int GetSubstringOccurenceCount(string primaryString, string substringToSeach)
    {
        // Loop through all instances of the string 'text'.
        int count = 0;
        int i = 0;
        while ((i = primaryString.IndexOf(substringToSeach, i)) != -1)
        {
            i += substringToSeach.Length;
            count++;
        }
        return count;
    }

    /// <summary>
    /// Returns whether or not the SQLManager is currently connected to a database.
    /// </summary>
    /// <returns></returns>
    public static bool IsConnected()
    {
        return Instance().IsAvailable;
    }

    /// <summary>
    /// Checks if theres a table reference in the scene that matches.
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private bool TableExistsWithinDatabase(string scene)
    {
        if (IsAvailable)
        {
            if (GetTableFormattedSceneName(scene, out string tableName) == false)
            {
                return false;
            }

            //check if the table exists
            IDbCommand dbcmd = LocalDatabase.CreateCommand();
            dbcmd.CommandText = string.Format("SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{0}'", tableName);

            IDataReader reader = dbcmd.ExecuteReader();

            int index = 0;
            while (reader.Read())
            {
                string output = reader.GetString(index);
                if (output == tableName)
                {
                    dbcmd.Dispose();
                    return true;
                }

            }

            dbcmd.Dispose();
        }


        //if we got here that means the table doesnt exist. so we're going to make it
        return CreateLevelTables(scene);
    }

    #endregion

    #region Events

    /// <summary>
    /// Every time a scene is loaded the database will automatically try and load run info related to the scene.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoad(scene, mode);

        GetTableFormattedSceneName(scene.name, out string FormattedRunName);

        CurrentPlayerRunForCurrentScene = new LevelHeats(FormattedRunName);
        CurrentPlayerSegmentsForCurrentScene = new LevelHeats(FormattedRunName + "-Segments");

        LoadSceneRunData(scene.name);
    }

    /// <summary>
    /// When an unload happens we flush the old data.
    /// </summary>
    /// <param name="scene"></param>
    protected override void OnSceneUnload(Scene scene)
    {
        base.OnSceneUnload(scene);

        CurrentPlayerRunForCurrentScene = new LevelHeats(false);
        CurrentPlayerSegmentsForCurrentScene = new LevelHeats(false);
        BestRunForCurrentScene = new LevelHeats(false);
        BestSegmentsForCurrentScene = new LevelHeats(false);
    }

    /// <summary>
    /// Called by each checkpoint in levels.
    /// </summary>
    /// <param name="param"></param>
    private void OnCheckpointReachedEvent(EventParam param)
    {
        if (IsAvailable)
        {
            OnCheckpointReachedParam CheckpointParam = (OnCheckpointReachedParam)param;

            if (CheckpointParam.CheckpointIndex == 0)
            {
                Debug.LogError("Error: RespawnPoint Index not set. Set immediately in inspector.");
                return;
            }

            string DictionaryKey = string.Format("Checkpoint{0}", CheckpointParam.CheckpointIndex.ToString());
            string PreviousKey = string.Format("Checkpoint{0}", (CheckpointParam.CheckpointIndex - 1).ToString());

            if (CurrentPlayerRunForCurrentScene.CheckpointTimepoints.ContainsKey(DictionaryKey))
            {
                Debug.LogError(string.Format("Error: SQLManager received OnCheckpointReached with a checkpoint index value already assigned. Did you forget to set the checkpoint" +
                    " index in the inspector? (Value:{0})", CheckpointParam.CheckpointIndex));
                return;
            }

            CurrentPlayerRunForCurrentScene.CheckpointTimepoints[DictionaryKey] = CheckpointParam.TimeReached;

            if (CheckpointParam.CheckpointIndex == 1)
            {
                CurrentPlayerSegmentsForCurrentScene.CheckpointTimepoints[DictionaryKey] = CheckpointParam.TimeReached;
            }
            else
            {
                if (CurrentPlayerRunForCurrentScene.CheckpointTimepoints.ContainsKey(PreviousKey))
                {
                    CurrentPlayerSegmentsForCurrentScene.CheckpointTimepoints[DictionaryKey] = CheckpointParam.TimeReached -
                        CurrentPlayerRunForCurrentScene.CheckpointTimepoints[PreviousKey];
                }
            }
        }
    }

    /// <summary>
    /// Called by the finishing line event in levels.
    /// </summary>
    private void OnLevelCompletedEvent(EventParam param)
    {
        if (IsAvailable && TableExistsWithinDatabase(SceneManager.GetActiveScene().name))
        {
            OnLevelCompleteParam Param = (OnLevelCompleteParam)param;

            CurrentPlayerRunForCurrentScene.CheckpointTimepoints[LevelScoreKey] = Param.TimeReached;
            if (CurrentPlayerRunForCurrentScene.CheckpointTimepoints.ContainsKey(string.Format("Checkpoint{0}", CurrentSceneCheckpointCount.ToString())))
            {
                CurrentPlayerSegmentsForCurrentScene.CheckpointTimepoints[LevelScoreKey] = Param.TimeReached -
                CurrentPlayerRunForCurrentScene.CheckpointTimepoints[string.Format("Checkpoint{0}", CurrentSceneCheckpointCount.ToString())];
            }
            else
            {
                CurrentPlayerSegmentsForCurrentScene.CheckpointTimepoints[LevelScoreKey] = -1;
            }



            if (SubmitRunData() == false)
            {
                Debug.LogWarning("SubmitLevelData failed.");
            }

            if (SubmitSegmentData() == false)
            {
                Debug.LogWarning("SubmitSegmentData failed.");
            }
        }
    }

    #endregion

    private void OnApplicationQuit()
    {
        if (LocalDatabase != null)
        {
            LocalDatabase.Close();
            LocalDatabase = null;
        }
    }

    /// <summary>
    /// SQLManager structure for containing level data in an understandable manner.
    /// </summary>
    public struct LevelHeats
    {
        /// <summary>
        /// Map of each checkpoint and it's associated value.
        /// </summary>
        public Dictionary<string, float> CheckpointTimepoints;

        /// <summary>
        /// Scene name. Is null if data is corrupt.
        /// </summary>
        public string LevelName;

        /// <summary>
        /// If the data is corrupted or unusable, this will be false.
        /// </summary>
        public bool IsValid;

        public LevelHeats(bool IsValid = false)
        {
            this.IsValid = false;
            LevelName = "null";
            CheckpointTimepoints = new Dictionary<string, float>();
        }

        public LevelHeats(string levelName)
        {
            LevelName = levelName;
            IsValid = true;
            CheckpointTimepoints = new Dictionary<string, float>();
        }
    }

}