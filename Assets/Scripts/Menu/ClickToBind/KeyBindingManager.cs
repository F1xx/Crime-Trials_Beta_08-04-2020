using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;


//static class that stores the key dictionary. The dictionary is loaded at runtime from Keybinding scripts.
//The keybinding scripts will load from the inspector unless there is a corresponding key in player prefs.
public class KeyBindingManager : Singleton<KeyBindingManager>
{

    public Dictionary<KeyBindType, Dictionary<KeyAction, KeyCode>> KeyDict = new Dictionary<KeyBindType, Dictionary<KeyAction, KeyCode>>();

    private bool m_IsReassigning = false;
    private KeyBinding m_BindingThatLastSetAssigning = null;

    public static bool IsReassigning { get { return Instance().m_IsReassigning; } }

    public static void SetAssigning(bool isassigning, KeyBinding bindingThatIsSettingUs)
    {
        if(Instance().m_BindingThatLastSetAssigning != null && Instance().m_BindingThatLastSetAssigning != bindingThatIsSettingUs)
        {
            Instance().m_BindingThatLastSetAssigning.ExitSelection();
            Instance().m_BindingThatLastSetAssigning = null;
        }

        Instance().m_BindingThatLastSetAssigning = bindingThatIsSettingUs;

        Instance().m_IsReassigning = isassigning;
    }

    protected override void OnAwake()
    {
        DontDestroyOnLoad(this);

        Listen("OnEditorQuit", OnApplicationClose);
        Listen("OnSaveKeybinds", SaveKeybindsConfig);

        KeyDict = new Dictionary<KeyBindType, Dictionary<KeyAction, KeyCode>>();

        KeyDict.Add(KeyBindType.primary, new Dictionary<KeyAction, KeyCode>());
        KeyDict.Add(KeyBindType.alternate, new Dictionary<KeyAction, KeyCode>());
        KeyDict.Add(KeyBindType.gamepad, new Dictionary<KeyAction, KeyCode>());
        KeyDict.Add(KeyBindType.gamepadalternate, new Dictionary<KeyAction, KeyCode>());

        LoadFromFile();
    }

    //Returns key code
    public static List<KeyCode> GetKeyCode(KeyAction key)
    {
        List<KeyCode> _keyCodes = new List<KeyCode>();

        //These nested loops result in filling a list with all keys
        //loop through the dictionary of dictionaries
        foreach (KeyValuePair<KeyBindType, Dictionary<KeyAction, KeyCode>> dictdict in Instance().KeyDict)
        {
            if (dictdict.Value.ContainsKey(key))
            {
                if (Input.GetKeyUp(dictdict.Value[key]))
                {
                    //return true when the first key to be down is pressed.
                    //if one is down thats enough
                    _keyCodes.Add(dictdict.Value[key]);
                    continue;
                }
            }
        }
        //return all keys associated with this action
        return _keyCodes;
    }

    /// <summary>
    /// Use in place of Input.GetKey
    /// Checks if the key/action is held down
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool GetKey(KeyAction key)
    {
        //loop through the dictionary of dictionaries
        foreach (KeyValuePair<KeyBindType, Dictionary<KeyAction, KeyCode>> dictdict in Instance().KeyDict)
        {
            if (dictdict.Value.ContainsKey(key))
            {
                if (Input.GetKey(dictdict.Value[key]))
                {
                    //return true when the first key to be down is pressed.
                    //if one is down thats enough
                    return true;
                }
            }
        }
        return false;
    }

    //Use in place of Input.GetKeyDown
    public static bool GetButtonDown(KeyAction key)
    {
        //loop through the dictionary of dictionaries
        foreach (KeyValuePair<KeyBindType, Dictionary<KeyAction, KeyCode>> dictdict in Instance().KeyDict)
        {
            if (dictdict.Value.ContainsKey(key))
            {
                if (Input.GetKeyDown(dictdict.Value[key]))
                {
                    //return true when the first key to be down is pressed.
                    //if one is down thats enough
                    return true;
                }
            }
        }
        return false;
    }

    //Use in place of Input.GetKeyUP
    public static bool GetButtonUp(KeyAction key)
    {
        //loop through the dictionary of dictionaries
        foreach (KeyValuePair<KeyBindType, Dictionary<KeyAction, KeyCode>> dictdict in Instance().KeyDict)
        {
            if(dictdict.Value.ContainsKey(key))
            {
                if (Input.GetKeyUp(dictdict.Value[key]))
                {
                    //return true when the first key to be down is pressed.
                    //if one is down thats enough
                    return true;
                }
            }
        }
        return false;
    }

    public static void UpdateDictionary(KeyBinding key, bool silent = false)
    {
        Dictionary<KeyAction, KeyCode> codeDict;

        //if we have the dictionary for this bind type (the enum like primary, alt, gamepad, gamepadalt)
        if(Instance().KeyDict.TryGetValue(key.keyBindType, out codeDict))
        {
            //if we don't have this entry yet, add it
            if (!codeDict.ContainsKey(key.keyAction))
            {
                codeDict.Add(key.keyAction, key.keyCode);
            }
            else
            {
                codeDict[key.keyAction] = key.keyCode;
            }

            //don't trigger the event if we're just nulling out a value
            if (key.keyCode != KeyCode.None && key.keyCode != KeyCode.Clear && silent != true)
            {
                OnChangeKeybindParam param = new OnChangeKeybindParam(key.keyAction, key.keyBindType, key.keyCode);
                EventManager.TriggerEvent("OnChangeKeybind", param);
            }
        }
    }

    protected void LoadFromFile()
    {
        string fileName = "Keybinds.config";

        string usersFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string crimetrialsPath = usersFolderPath + "/" + "CrimeTrials";
        string fullDatabasePath = crimetrialsPath + "/" + fileName;

        string RawLanguageInfo = "";

        //Check our languages directory assuming it exists
        if (Directory.Exists(crimetrialsPath))
        {
            DirectoryInfo info = new DirectoryInfo(crimetrialsPath);

            //Find our text files
            foreach (FileInfo item in info.GetFiles(fileName))
            {
                RawLanguageInfo = File.ReadAllText(item.FullName, System.Text.Encoding.UTF8);
                break;
            }
        }
        else
        {
            Debug.LogError("Error: Users/CrimeTrials folder does not exist.");
            return;
        }

        if(RawLanguageInfo != "")
        {
            ParseKeybindsConfig(ref RawLanguageInfo);
        }
        else
        {
            if (CreateKeybindsConfig())
            {
                LoadFromFile();
            }
        }
    }

    void ParseKeybindsConfig(ref string RawLanguageInfo)
    {
        RawLanguageInfo = RawLanguageInfo.Replace("\r", "");

        string[] lines = RawLanguageInfo.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        for (int i = 0; i < lines.Length; i++)
        {
            int counter = 0;
            bool eof = false;

            KeyAction action = KeyAction.none;
            KeyBindType bindtype = KeyBindType.none;
            KeyCode code = KeyCode.None;

            while (lines[i].Length > 0)
            {
                int index = lines[i].IndexOf('-');

                if (index == -1)
                {
                    index = lines[i].Length;
                    eof = true;
                }

                switch (counter)
                {
                    case 0:
                        bindtype = (KeyBindType)System.Convert.ToInt32(lines[i].Substring(0, index));
                        break;
                    case 1:
                        action = (KeyAction)System.Convert.ToInt32(lines[i].Substring(0, index));
                        break;
                    case 2:
                        code = (KeyCode)System.Convert.ToInt32(lines[i].Substring(0, index));
                        break;
                }

                if (!eof)
                {
                    lines[i] = lines[i].Substring(index + 1);
                    counter++;
                }
                else
                {
                    lines[i] = "";
                }
            }

            KeyDict[bindtype][action] = code;
        }
    }

    void SaveKeybindsConfig()
    {
        string lines = "";

        //read each key and build line by line
        foreach (KeyValuePair<KeyBindType, Dictionary<KeyAction, KeyCode>> dictdict in Instance().KeyDict)
        {
            string bindtypeAsString = ((int)dictdict.Key).ToString();

            foreach (var val in dictdict.Value)
            {
                lines += bindtypeAsString + '-' + ((int)val.Key).ToString() + '-' + ((int)val.Value).ToString();
                lines += '\n';
            }
        }

        string fileName = "Keybinds.config";

        string usersFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string crimetrialsPath = usersFolderPath + "/" + "CrimeTrials";
        string fullDatabasePath = crimetrialsPath + "/" + fileName;

        //save
        System.IO.File.WriteAllText(@fullDatabasePath, lines);

    }

    protected bool CreateKeybindsConfig()
    {
       // GameObject baseMenu = GameObject.Find("MainMenu");
       // Transform finalTransform = baseMenu.transform.Find("ControlsVisual_Panel");

        //KeyBinding[] bindingObjects = finalTransform.GetComponentsInChildren<KeyBinding>();

        //if(bindingObjects.Length == 0)
       // {
       //     Debug.LogError("Failed to find any bindings. Critical Error.");
       //     return false;
       // }

        //string lines = "";
        string lines = "1-8-32\n1-6-304\n1-7-306\n1-5-323\n1-9-114\n2-8-0\n2-6-0\n2-7-99\n2-5-0\n2-9-0\n3-8-330\n3-6-334\n3-7-331\n3-5-489\n3-9-333\n4-8-0\n4-6-338\n4-7-0\n4-5-0\n4-9-0\n";

        //for (int i = 0; i < bindingObjects.Length; i++)
        //{
        //    KeyBindType bindtype = bindingObjects[i].keyBindType;
        //    KeyAction action = bindingObjects[i].keyAction;
        //    KeyCode code = bindingObjects[i].keyCode;
        //
        //    lines += ((int)bindtype).ToString() + '-' + ((int)action).ToString() + '-' + ((int)code).ToString();
        //    lines += '\n';
        //}

        string fileName = "Keybinds.config";

        string usersFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string crimetrialsPath = usersFolderPath + "/" + "CrimeTrials";
        string fullDatabasePath = crimetrialsPath + "/" + fileName;

        //save
        System.IO.File.WriteAllText(@fullDatabasePath, lines);
        return true;
    }

    public static KeyCode GetKeyDictionary(KeyAction action, KeyBindType type)
    {
        //KeyCode pref = PlayerPrefs.GetInt(action.ToString() + type.ToString(), 0); //Keycode 0 is KeyCode.None

        if (Instance().KeyDict[type].TryGetValue(action, out KeyCode pref) == false)
        {
            Debug.LogError(string.Format("No KeyCode found matching action {0} in dictionary {1}", action, type));
            return KeyCode.None;
        }

        return pref;
    }

    private void OnApplicationClose()
    {
        SaveKeybindsConfig();
    }
}

//used to safe code inputs
//Add new keys to "bind" here
public enum KeyAction
{
    none,
    moveforward,
    movebackward,
    moveleft,
    moveright,
    shoot,
    sprint,
    crouch,
    jump,
    reset,
}

public enum KeyBindType
{
    none,
    primary,
    alternate,
    gamepad,
    gamepadalternate
}