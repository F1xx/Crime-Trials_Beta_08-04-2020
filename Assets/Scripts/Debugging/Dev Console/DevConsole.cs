using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DevConsole : Singleton<DevConsole>
{
    public bool IsActive { get { return DevCanvas.enabled; } }

    public bool HasBeenActivated { get; protected set; }

    public Text ListText = null;//the log
    public Text SelectedItem = null;
    public Canvas DevCanvas = null;


    private InputField m_InputField = null;
    private GameObject m_Player = null;
    private AudioChannel m_AudioChannel = null;

    [HideInInspector] private AudioManager m_AudioManager { get; set; }

    private GameObject SelectedObject = null;

    protected override void OnAwake()
    {
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoad(scene, mode);
        HasBeenActivated = false;
        Init();
    }

    private void Init()
    {
        GameObject menu = PrefabManager.GetPrefab("DevCanvas");
        menu = Instantiate(menu);

        m_AudioChannel = gameObject.AddComponent<AudioChannel>();

        HasBeenActivated = false;

        DevCanvas = menu.GetComponent<Canvas>();

        var objs = menu.transform.GetComponentsInChildren<Transform>();

        foreach (var obj in objs)
        {
            if (obj.name == "ListText")
            {
                ListText = obj.gameObject.GetComponent<Text>();
            }
            else if (obj.name == "SelectedItemText")
            {
                SelectedItem = obj.gameObject.GetComponent<Text>();
            }
        }

        m_InputField = menu.GetComponentInChildren<InputField>();
        m_InputField.onEndEdit.AddListener(RecieveCommand);

        m_Player = GameObject.Find("Player");
        m_AudioManager = AudioManager.Instance();
        DevCanvas.enabled = false;
    }

    void Update()
    {
        if(Instance() == null)
        {
            return;
        }

        //see if we're selecting something
        if (Input.GetMouseButtonDown(0) && Instance().IsActive)
        {
            SelectObject();
        }

        if (Input.GetMouseButtonDown(1) && Instance().IsActive)
        {
            DeselectObject();
        }

        if(Input.GetKeyDown(KeyCode.F4))
        {
            HandleCommand("KILL");
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            HandleCommand("TGM");
        }
    }

    public static void ToggleConsole()
    {
        if (Instance())
        {
            if (Instance().DevCanvas.enabled)
            {
                Instance().CloseConsole();
                return;
            }
            Instance().OpenConsole();
        }
    }

    void OpenConsole()
    {
        if (!Instance())
        {
            return;
        }

        Instance().DevCanvas.enabled = true;
        Instance().m_InputField.enabled = true;
        SelectInputField();

        PauseManager.Pause();
    }

    void CloseConsole()
    {
        DevConsole instance = Instance();

        if (!instance)
        {
            return;
        }

        PauseManager.Unpause();

        instance.m_InputField.CancelInvoke();
        instance.m_InputField.DeactivateInputField();
        instance.ClearInputField();

        instance.m_InputField.enabled = false;
        instance.DevCanvas.enabled = false;
    }

    public static void RecieveCommand(string command)
    {
        DevConsole instance = Instance();
        if (instance)
        {
            if (instance.DevCanvas != null)
            {
                if (instance.DevCanvas.enabled)
                {
                    instance.AddCommandToList(command);

                    instance.HandleCommand(command);

                    instance.SelectInputField();
                }
            }
        }
    }

    void SelectInputField()
    {
        DevConsole instance = Instance();

        if (!instance)
        {
            return;
        }

        instance.ClearInputField();
        instance.m_InputField.ActivateInputField();
        instance.m_InputField.Select();
    }

    void ClearInputField()
    {
        DevConsole instance = Instance();

        if (!instance)
        {
            return;
        }

        instance.m_InputField.text = "";
    }

    void SelectObject()
    {
        DevConsole instance = Instance();

        if (!instance)
        {
            return;
        }

        RaycastHit hitInfo;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {
            instance.SelectedObject = hitInfo.transform.gameObject;
            instance.SelectedItem.text = instance.SelectedObject.name;
        }
    }

    void DeselectObject()
    {
        DevConsole instance = Instance();

        if (!instance)
        {
            return;
        }

        instance.SelectedObject = null;
        instance.SelectedItem.text = "";
    }

    void AddCommandToList(string command)
    {
        DevConsole instance = Instance();

        if (!instance)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(command) || command == "`")
        {
            return;
        }

        instance.ListText.text += command + "\n";
    }

    //actually parses the command
    void HandleCommand(string command)
    {
        DevConsole instance = Instance();

        if (!instance)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(command) || command == "`")
        {
            return;
        }

        command = command.ToUpper();
        //split the command into individuals
        //first should ALWAYS be the command. If it isn't the string is discarded
        string[] words = command.Split(' ');

        switch(words[0])
        {
            case "TP":
                instance.ParseTeleport(words);
                HasBeenActivated = true;
                break;
            case "TGM":
                HasBeenActivated = true;
                instance.ToggleGodMode(words);
                break;
            case "SPAWN":
                //instance.SpawnPrefab(words);
                instance.ListText.text += "Spawn has been removed for development reasons.\n";
                break;
            case "PLAY":
                instance.PlayDevSound(words);
                break;
            case "KILL":
                instance.Kill();
                break;
            case "DB":
                instance.ParseSQL(words);
                break;
            case "END":
                instance.EndLevel(words);
                break;

        }
    }

    void EndLevel(string[] words)
    {
        PauseManager.Pause();

        OnLevelCompleteParam param = new OnLevelCompleteParam(WorldTimeManager.TimePassed);
        EventManager.TriggerEvent("OnLevelComplete", param);
    }

    void ParseSQL(string[] args)
    {
        if (args.Length < 2)
        {
            ListText.text += "Commands:\n RESET - resets current level database\n RESETALL - resets all level databases\n STATUS - current connection status\n CONNECT - retry connecting to database.\n";
            return;
        }

        switch (args[1])
        {
            case "RESET":
                if (SQLManager.ResetLocalLeaderboard(SceneManager.GetActiveScene().name) == true)
                {
                    ListText.text += string.Format("Leaderboard for {0} reset.\n", SceneManager.GetActiveScene().name);
                }
                else
                {
                    ListText.text += string.Format("Leaderboard for {0} failed to reset.\n", SceneManager.GetActiveScene().name);
                }
                break;

            case "RESETALL":
                SQLManager.ResetEntireDatabase();
                ListText.text += "All leaderboards reset.\n";
                break;
            case "CONNECT":
                SQLManager.InitializeLocalDatabase();
                ListText.text += "attempting to reconnect... ";
                if (SQLManager.IsConnected())
                {
                    ListText.text += "database is connected.\n";
                }
                else
                {
                    ListText.text += "database is unconnected.\n";
                }
                break;
            case "STATUS":
                if (SQLManager.IsConnected())
                {
                    ListText.text += "database is connected.\n";
                }
                else
                {
                    ListText.text += "database is unconnected.\n";
                }
                break;
        }
    }

    //handles turning the command into actualy variables to teleport
    //if it is successful it does the teleport. if not errors will log to the DevConsole.
    void ParseTeleport(string[] args)
    {
        GameObject target = null;
        Vector3 location = Vector3.zero;

        string locName = "";

        if(args.Length > 3 || args.Length < 2) //requires 2-3 arguments
        {
            PrintTeleportHelp();
            return;
        }
        else if (args[1] == "HELP")
        {
            PrintTeleportHelp();
            return;
        }
        else if (args.Length == 2) //default that 2 arguments means you're teleporting the player
        {
            target = m_Player;
            locName = args[1];
        }

        if (target == null)
        {
            locName = args[2];

            if (FindTarget(args[1], out target) == false) //it didn't find the target (already logged in the function)
            {
                return;
            }
        }

        //didn't find a location. Already logged in function.
        if(FindLocation(locName, out location) == false)
        {
            return;
        }

        Teleport(target, location);
    }

    private bool FindLocation(string dest, out Vector3 location)
    {
        location = Vector3.zero;

        //if we don't enter this if then location is already filled with the parsed vector value
        if (MathUtils.StringToVector3(dest, out location) == false) //user didn't pass in 0,0,0
        {
            //maybe they passed in a gameobject
            GameObject target = null;
            if (FindTarget(dest, out target) == false) //it didn't find the target (already logged in the function)
            {
                AddCommandToList("Invalid Location. Must be either a gameobject or numbers in 0,0,0 format");
                return false;
            }
            //if we get here then it found a gameobject
            location = target.transform.position;
        }

        return true;
    }

    //returns true and fills the foundTarget GameObject if it finds the target
    //returns fals and null if not
    private bool FindTarget(string target, out GameObject foundtarget)
    {
        foundtarget = null;

        if (target == "PLAYER")
        {
            foundtarget = m_Player;
        }
        else if (target == "SELECTED")
        {
            if (SelectedObject == null)
            {
                AddCommandToList("No Object selected to use.");
                return false;
            }

            foundtarget = SelectedObject;
        }
        else
        {
            foundtarget = GameObject.Find(target);

            if (foundtarget == null)
            {
                AddCommandToList("Invalid Object: " + target + " Doesn't exist in this context");
                return false;
            }
        }

        return true;
    }

    private void Teleport(GameObject obj, Vector3 location)
    {
        obj.transform.position = location;
    }

    public static void LogToConsole(string Text)
    {
        Instance().AddCommandToList(Text);
    }

    //private void SpawnPrefab(string[] words)
    //{
    //    if (words.Length != 2)//spawn prefab, thats the only size it should be
    //    {
    //        PrintSpawnHelp();
    //        return;
    //    }
    //    if (words[1] == "HELP")
    //    {
    //        PrintSpawnHelp();
    //        return;
    //    }

    //    GameObject prefabToSpawn = PrefabManager.GetPrefab(words[1]);

    //    if(prefabToSpawn == null)
    //    {
    //        AddCommandToList(words[1] + " Is not a loaded Prefab.");
    //        return;
    //    }

    //    float size = prefabToSpawn.transform.localScale.magnitude;

    //    Vector3 playerpos = m_Player.transform.position;
    //    Vector3 playerDir = m_Player.transform.forward;
    //    Vector3 location = playerpos + (playerDir * (1.0f + size)); //in front of the player

    //    Instantiate<GameObject>(prefabToSpawn, location, Quaternion.identity);
    //}

    private void PlayDevSound(string[] words)
    {
        if (words.Length == 1)
        {
            PrintAudioHelp();
            return;
        }
        if (words[1] == "HELP")
        {
            PrintAudioHelp();
            return;
        }

        if (words.Length > 3)
        {
            AddCommandToList("Invalid operation size. Should be PLAY {Soundfilename} {position}");
            return;
        }

        AudioWrapper soundClip = AudioManager.GetAudioWrapper(words[1]);
        if (soundClip == null)
        {
            AddCommandToList("Invalid filename. Sound does not exist. Only include the name of the file. No directories or extensions.");
            return;
        }

        Vector3 target = Vector3.zero;

        if (words.Length == 3)
        {
            string targetName = words[2];

            if (targetName == "PLAYER")
            {
                target = m_Player.transform.position;
            }
            else if (targetName == "SELECTED" && SelectedObject)
            {
                target = SelectedObject.transform.position;
            }
            else if (targetName == "SELECTED" && !SelectedObject)
            {
                AddCommandToList("No Object selected to use.");
                return;
            }
            else if (targetName.Contains(","))
            {
                if (MathUtils.StringToVector3(targetName, out target))
                {
                }
                else
                {
                    AddCommandToList("Invalid position: " + targetName + " must be a location in x,y,z format");
                }
            }
            else
            {
                GameObject toCheck = GameObject.Find(targetName);

                if (toCheck == null)
                {
                    AddCommandToList("Invalid Object: " + targetName + " doesn't exist in this context");
                    return;
                }

                target = toCheck.transform.position;
            }
        }
        else
        {
            target = GameObject.Find("Player").transform.position;
        }

        m_AudioChannel.SetChannelType(soundClip.ChannelType);
        
        if (m_AudioChannel.IsPlaying())
        {
            m_AudioChannel.StopAudio();
        }

        m_AudioChannel.transform.position = target;
        m_AudioChannel.PlayAudio(soundClip);

        //AudioSource.PlayClipAtPoint(soundClip.AudioClip, target, m_AudioManager.MasterVolume);
        AddCommandToList("Played sound " + words[1] + ".");
    }

    private void Kill()
    {
        DevConsole instance = Instance();

        if(!instance)
        {
            return;
        }

        instance.m_Player.GetComponent<HealthComponent>().Kill();
    }

    private void PrintAudioHelp()
    {
        AddCommandToList("PLAY requires a sound file to play as well as a position (or object) in the world to play the sound.");
        AddCommandToList("Example usage: PLAY Quick_Player_3D_ShootSound 0,0,0");
        AddCommandToList("Example usage: PLAY Quick_Player_3D_ShootSound Player");
    }

    private void PrintGodModeHelp()
    {
        AddCommandToList("TGM, Godmode takes 0 arguments. type 'tgm' and the player will not take damage or will if you were already in Godmode.");
    }

//     private void PrintSpawnHelp()
//     {
//         AddCommandToList("SPAWN uses prefabs to spawn. If you're spawning something that isn't a prefab this will fail");
//         AddCommandToList("SPAWN parameters: spawn prefabtospawn");
//         AddCommandToList("If successful it should spawn in front of you.");
//     }

    private void PrintTeleportHelp()
    {
        AddCommandToList("CommandName ObjectToTeleport Location ex: tp Player 0,0,0 For ObjectToTeleport you can use player for player,");
        AddCommandToList("selected for the object selected by the cursor or type the name of an object in the world");
    }

    private void ToggleGodMode(string[] words)
    {
        if(words.Length > 1)
        {
            PrintGodModeHelp();
        }

        PlayerHealthComponent hc = m_Player.GetComponent<PlayerHealthComponent>();

        if(hc)
        {
            hc.ToggleGodMode();
        }
    }
}
