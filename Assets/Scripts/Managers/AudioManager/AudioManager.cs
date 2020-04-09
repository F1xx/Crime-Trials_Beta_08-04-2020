using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class AudioManager : Singleton<AudioManager>
{
    //Our loaded wrappers. We use a map here because no audio wrapper should have the same name.
    [SerializeField, Tooltip("Our manually loaded AudioWrappers.")]
    private Dictionary<string, AudioWrapper> AudioWrappers;

    [SerializeField, Tooltip("The raw file paths of our loaded audio data.")]
    private List<string> m_AudioClipsLoaded;
    private string AudioLoadFilePath;

    [Range(0.0001f, 1.0f), SerializeField, Tooltip("Our Master volume control.")]
    private float m_MasterVolume = 0.5f;
    public float MasterVolume { get { return m_MasterVolume; } set { m_MasterVolume = value; OnVolumeChanged(eChannelType.Master); } }

    [Range(0.0001f, 1.0f), SerializeField, Tooltip("Our Player volume control.")]
    private float m_PlayerVolume = 0.5f;
    public float PlayerVolume { get { return m_PlayerVolume; } set { m_PlayerVolume = value; OnVolumeChanged(eChannelType.Player); } }

    [Range(0.0001f, 1.0f), SerializeField, Tooltip("Our Commentator volume control.")]
    private float m_CommentatorVolume = 0.5f;
    public float CommentatorVolume { get { return m_CommentatorVolume; } set { m_CommentatorVolume = value; OnVolumeChanged(eChannelType.Commentator); } }

    [Range(0.0001f, 1.0f), SerializeField, Tooltip("Our Music volume control.")]
    private float m_MusicVolume = 0.5f;
    public float MusicVolume { get { return m_MusicVolume; } set { m_MusicVolume = value; OnVolumeChanged(eChannelType.Music); } }

    [Range(0.0001f, 1.0f), SerializeField, Tooltip("Our SoundFX volume control.")]
    private float m_SoundEffectsVolume = 0.5f;
    public float SoundEffectsVolume { get { return m_SoundEffectsVolume; } set { m_SoundEffectsVolume = value; OnVolumeChanged(eChannelType.SoundEffects); } }

    public AudioMixer m_AudioMixer;

    //this is where the subtitle shit goes
    public TMPro.TMP_Text SubtitleText = null;
    private AudioWrapper SubtitleAudio;
    private int CurrentSubtitleIndex = 0;
    private Timer SubtitleUpdateTimer;
    public bool MaturitySettings = false;

    //Language support
    private string CurrentLanguageSetting = "";
    private string LanguageLoadFilePath;
    private Dictionary<string, SubtitleInfo> LoadedSubtitleLanguageData;

    public enum eChannelType
    {
        Player,
        Commentator,
        Music,
        SoundEffects,
        Master,
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnAwake()
    {
        DontDestroyOnLoad(this);

        m_AudioClipsLoaded = new List<string>();

        m_AudioMixer = Resources.Load("GameAudioMixer") as AudioMixer;
        BuildAudioWrappers();

        AudioLoadFilePath = Application.dataPath + "/Resources/Audio";
        LanguageLoadFilePath = Application.dataPath + "/Resources/Language";

        SubtitleUpdateTimer = CreateTimer(float.MaxValue, UpdateSubtitleAudio);

        GameObject sub = GameObject.Find("Subtitle_Text");
        if(sub)
        {
            SubtitleText = sub.GetComponent<TMPro.TMP_Text>();
            SubtitleText.text = "";
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        CollectAllFilesToLoad(AudioLoadFilePath);
        LoadAllAudioFiles();

        Listen("OnVolumeChange", OnVolumeChanged);
        Listen("OnNewSubtitle", UpdateSubtitleAudio);
        Listen("OnMatureSettingChange", OnMatureSettingChanged);
        Listen("OnLanguageChange", OnLanguageChanged);

        LoadFromSettings();
        OnLanguageChanged();
    }

    void LoadFromSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", MasterVolume);

        PlayerVolume = PlayerPrefs.GetFloat("PlayerVolume", PlayerVolume);

        CommentatorVolume = PlayerPrefs.GetFloat("CommentatorVolume", CommentatorVolume);

        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", MusicVolume);

        SoundEffectsVolume = PlayerPrefs.GetFloat("SoundEffectsVolume", SoundEffectsVolume);

        if (PlayerPrefs.HasKey("OnMatureSettingChange"))
            MaturitySettings = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnMatureSettingChange"));
    }

    // Update is called once per frame
    void Update()
    {
        OnVolumeChanged(eChannelType.Commentator);
        OnVolumeChanged(eChannelType.Master);
        OnVolumeChanged(eChannelType.Music);
        OnVolumeChanged(eChannelType.Player);
        OnVolumeChanged(eChannelType.SoundEffects);
    }

    /// <summary>
    /// Build all the AudioWrappers into AudioManager to keep in memory for Dev console purposes or anything else that might care.
    /// </summary>
    void BuildAudioWrappers()
    {
        GameObject wrapperBuilder = Resources.Load("Prefabs/WrapperBuilder/WrapperBuilder") as GameObject;
        WrapperBuilder comp;

        if (wrapperBuilder != null)
        {
            comp = wrapperBuilder.GetComponent<WrapperBuilder>();

            AudioWrappers = new Dictionary<string, AudioWrapper>();
            List<AudioWrapper> baseList = comp.m_Wrappers.Distinct().ToList();

            foreach (var audio in baseList)
            {
                AudioWrappers.Add(audio.name.ToUpper(), audio);
            }
        }
        else
        {
            Debug.LogError("WrapperBuilder failed to load.");
        }
    }

    private void CollectAllFilesToLoad(string BaseDirectory)
    {
        if (Directory.Exists(BaseDirectory))
        {
            DirectoryInfo info = new DirectoryInfo(BaseDirectory);

            foreach (DirectoryInfo directory in info.GetDirectories())
            {
                CollectAllFilesToLoad(directory.FullName);
            }

            foreach (FileInfo item in info.GetFiles("*.ogg"))
            {
                //format string to be readable by unity
                string filename = item.FullName;
                filename = filename.Replace('\\', '/');
                filename = filename.Replace(AudioLoadFilePath, "");
                filename = filename.Replace(".ogg", "");
                filename = "Audio" + filename;

                //ready to be used by unity resources
                m_AudioClipsLoaded.Add(filename);
            }
        }
    }

    /// <summary>
    /// Get an AudioWrapper that is managed by the AudioManager.
    /// </summary>
    /// <param name="Name"></param>
    /// <returns></returns>
    public static AudioWrapper GetAudioWrapper(string Name)
    {
        AudioManager instance = Instance();

        if (Name.Length == 0)
        {
            return null;
        }

        if (instance.AudioWrappers.ContainsKey(Name.ToUpper()) == false)
        {
            Debug.LogWarning("AudioWrapper with name " + Name + " does not exist in this context.");
            return null;
        }

        return instance.AudioWrappers[Name.ToUpper()];
    }

    void LoadAllAudioFiles()
    {
        for (int i = 0; i < m_AudioClipsLoaded.Count; i++)
        {
            AudioClip clipSource = Resources.Load<AudioClip>(m_AudioClipsLoaded[i]);
            
            string filename = Path.GetFileName(m_AudioClipsLoaded[i]).ToUpper();

            AudioWrapper wrapper = ScriptableObject.CreateInstance<AudioWrapper>();
            wrapper.AudioClip = clipSource;           
            wrapper.SetWrapper(filename.Split('_'));

            AudioWrappers.Add(filename, wrapper);
            //Debug.Log("Loaded " + filename);
        }
    }

    /// <summary>
    /// Register a Channel with the AudioManager. Will be added to the list of managed objects and linked with the game's AudioMixer.
    /// </summary>
    /// <param name="Channel"></param>
    public static void RegisterChannel(AudioChannel Channel)
    {
        //Link it to the mixer.
        ChangeAudioChannelMixerGroup(Channel);
    }

    public static void ChangeAudioChannelMixerGroup(AudioChannel Channel)
    {
        AudioManager instance = Instance();

        //Set channel volume
        switch (Channel.GetChannelType())
        {
            case AudioManager.eChannelType.Player:
                Channel.AudioSourceObject.outputAudioMixerGroup = instance.m_AudioMixer.FindMatchingGroups("Player")[0];
                break;
            case AudioManager.eChannelType.Commentator:
                Channel.AudioSourceObject.outputAudioMixerGroup = instance.m_AudioMixer.FindMatchingGroups("Commentator")[0];
                break;
            case AudioManager.eChannelType.Music:
                Channel.AudioSourceObject.outputAudioMixerGroup = instance.m_AudioMixer.FindMatchingGroups("Music")[0];
                break;
            case AudioManager.eChannelType.SoundEffects:
                Channel.AudioSourceObject.outputAudioMixerGroup = instance.m_AudioMixer.FindMatchingGroups("SoundEffects")[0];
                break;
            default:
                Debug.LogError("Unknown Channel type: " + Channel.GetChannelType());
                break;
        }
    }

    /// <summary>
    /// Unregister a Channel from the AudioManager. Will also flush any AudioMixer data.
    /// </summary>
    /// <param name="Channel"></param>
    public static void UnregisterChannel(AudioChannel Channel)
    {
        Channel.AudioSourceObject.outputAudioMixerGroup = null; 
    }

    public static void PlayRandomSoundFromList(List<AudioClip> list, AudioChannel channel)
    {
        if (list.Count < 1)
        {
            return;
        }

        int randnum = Random.Range(0, list.Count - 1);

        channel.PlayAudio(list[randnum].name);
    }

    /// <summary>
    /// Passes the adjusted internal variable value stored here and sends it into the audiomixergroup.
    /// </summary>
    /// <param name="Type"></param>
    static private void OnVolumeChanged(eChannelType Type)
    {
        AudioManager instance = Instance();

        switch (Type)
        {
            case eChannelType.Player:
                if (MathUtils.AlmostEquals(instance.m_PlayerVolume, 0.0f, 0.01f))
                {
                    instance.m_AudioMixer.SetFloat("PlayerVolume", -100.0f);
                }
                else
                {
                    instance.m_AudioMixer.SetFloat("PlayerVolume", Mathf.Log10(instance.m_PlayerVolume) * 20);
                }
                break;
            case eChannelType.Commentator:
                if (MathUtils.AlmostEquals(instance.m_CommentatorVolume, 0.0f, 0.01f))
                {
                    instance.m_AudioMixer.SetFloat("CommentatorVolume", -100.0f);
                }
                else
                {
                    instance.m_AudioMixer.SetFloat("CommentatorVolume", Mathf.Log10(instance.m_CommentatorVolume) * 20);
                }
                break;
            case eChannelType.Music:
                if (MathUtils.AlmostEquals(instance.m_MusicVolume, 0.0f, 0.01f))
                {
                    instance.m_AudioMixer.SetFloat("MusicVolume", -100.0f);
                }
                else
                {
                    instance.m_AudioMixer.SetFloat("MusicVolume", Mathf.Log10(instance.m_MusicVolume) * 20);
                }
                break;
            case eChannelType.SoundEffects:
                if (MathUtils.AlmostEquals(instance.m_SoundEffectsVolume, 0.0f, 0.01f))
                {
                    instance.m_AudioMixer.SetFloat("SoundEffectsVolume", -100.0f);
                }
                else
                {
                    instance.m_AudioMixer.SetFloat("SoundEffectsVolume", Mathf.Log10(instance.m_SoundEffectsVolume) * 20);
                }
                break;
            case eChannelType.Master:
                if (MathUtils.AlmostEquals(instance.m_MasterVolume, 0.0f, 0.01f))
                {
                    instance.m_AudioMixer.SetFloat("MasterVolume", -100.0f);
                }
                else
                {
                    instance.m_AudioMixer.SetFloat("MasterVolume", Mathf.Log10(instance.m_MasterVolume) * 20);
                }
                break;
            default:
                break;
        }
    }

    void OnVolumeChanged(EventParam param)
    {
        OnVolumeChangeParam VCP = (OnVolumeChangeParam)param;

        switch (VCP.Channel)
        {
            case eChannelType.Player:
                PlayerVolume = PlayerPrefs.GetFloat("PlayerVolume");
                break;
            case eChannelType.Commentator:
                CommentatorVolume = PlayerPrefs.GetFloat("CommentatorVolume");
                break;
            case eChannelType.Music:
                MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
                break;
            case eChannelType.SoundEffects:
                SoundEffectsVolume = PlayerPrefs.GetFloat("SoundEffectsVolume");
                break;
            case eChannelType.Master:
                MasterVolume = PlayerPrefs.GetFloat("MasterVolume");
                break;
        }
    }

    void OnMatureSettingChanged()
    {
        MaturitySettings = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnMatureSettingChange"));
    }

    /// <summary>
    /// Update AudioManager with a new AudioWrapper to begin pulling subtitle information from to display.
    /// </summary>
    /// <param name="NewSubtitleAudio"></param>
    static public void SetSubtitleAudio(AudioWrapper NewSubtitleAudio)
    {
        AudioManager instance = Instance();

        instance.SubtitleAudio = NewSubtitleAudio;
        instance.CurrentSubtitleIndex = 0;
        instance.SubtitleUpdateTimer.StopTimer();

        //Debug.Log("Subtitle set.");
        EventManager.TriggerEvent("OnNewSubtitle");
        
    }

    /// <summary>
    /// Internal Function call for updating the subtitle text display. This function is called in 2 ways: response to OnNewSubtitle, or SubtitleUpdateTimer completion.
    /// </summary>
    private void UpdateSubtitleAudio()
    {
       
        //If the index is outside the array bounds then we're done displaying text and can safely clear the canvas.
        if (CurrentSubtitleIndex >= LoadedSubtitleLanguageData[SubtitleAudio.name].SubtitleLines.Length)
        {
            //Debug.Log("Cleared subtitle.");

            //set canvas to empty
            SubtitleText.text = "";        
            return;
        }

        //set string for canvas here
        SubtitleText.text = LoadedSubtitleLanguageData[SubtitleAudio.name].SubtitleLines[CurrentSubtitleIndex];

        //Debug.Log(string.Format("Update subtitle: index - {0}, text - {1}", CurrentSubtitleIndex, SubtitleAudio.SubtitleLines[CurrentSubtitleIndex]));

        //prepare timer for next string in sequence
        SubtitleUpdateTimer.SetDuration(LoadedSubtitleLanguageData[SubtitleAudio.name].SubtitleDurations[CurrentSubtitleIndex]);
        SubtitleUpdateTimer.Restart();

        CurrentSubtitleIndex++;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject sub = GameObject.Find("Subtitle_Text");
        if (sub)
        {
            SubtitleText = sub.GetComponent<TMPro.TMP_Text>();
            SubtitleText.text = "";
        }

        SubtitleAudio = null;
        CurrentSubtitleIndex = 0;
        SubtitleUpdateTimer.StopTimer();
    }

    /// <summary>
    /// Used only in editor to update whenever volume changes because someone messes with it.
    /// </summary>
    static private void OnValidate()
    {
        AudioManager instance = Instance();
        instance.PlayerVolume = instance.m_PlayerVolume;
        instance.CommentatorVolume = instance.m_CommentatorVolume;
        instance.MusicVolume = instance.m_MusicVolume;
        instance.SoundEffectsVolume = instance.m_SoundEffectsVolume;
    }
    
    /// <summary>
    /// Activated on OnAwake to build initial Langauge data from settings, this will also be automatically called if language settings change.
    /// </summary>
    private void OnLanguageChanged()
    {
        if (PlayerPrefs.HasKey("OnLanguageChange"))
        {
            CurrentLanguageSetting = PlayerPrefs.GetString("OnLanguageChange");
        }

        //We'll default to english if there's nothing loaded from preferences.
        if (CurrentLanguageSetting == "")
        {
            CurrentLanguageSetting = "English";
        }

        string RawLanguageInfo = null;

        //Check our languages directory assuming it exists
        if (Directory.Exists(LanguageLoadFilePath))
        {
            DirectoryInfo info = new DirectoryInfo(LanguageLoadFilePath);

            //Find our text files
            foreach (FileInfo item in info.GetFiles("*.txt"))
            {
                //If the name matches our language then we're good to parse.
                if (item.Name == CurrentLanguageSetting + item.Extension)
                {
                    RawLanguageInfo = File.ReadAllText(item.FullName, Encoding.UTF8);
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("Error: Resources/Language folder does not exist.");
            return;
        }

        //If we found raw data we can begin parsing
        if (RawLanguageInfo != null)
        {
            ParseLanguageFile(ref RawLanguageInfo);
        }
        //if no file matches the input language name (empty string or something) then we'll make an empty list and leave.
        else
        {
            LoadedSubtitleLanguageData = new Dictionary<string, SubtitleInfo>();
        }


    }
    
    /// <summary>
    /// Parse raw string data interpreted as a CrimeTrials language file. This will wipe clean any existing language data and build the new data using the file.
    /// </summary>
    /// <param name="RawLanguageInfo"></param>
    private void ParseLanguageFile(ref string RawLanguageInfo)
    {
        //wipe old data clean and start fresh
        LoadedSubtitleLanguageData = new Dictionary<string, SubtitleInfo>();

        //first things first, we need to remove all comments from this beast as well as any \r unicode format.

        RawLanguageInfo = RawLanguageInfo.Replace("\r", "");

        //Lets remove all //\n lines
        while (RawLanguageInfo.Contains("//"))
        {
            int commentStartIndex = RawLanguageInfo.IndexOf("//");
            int commentEndIndex = RawLanguageInfo.Substring(commentStartIndex).IndexOf("\n");
           
            if (commentEndIndex != -1)
            {
                RawLanguageInfo = RawLanguageInfo.Substring(0, commentStartIndex) + RawLanguageInfo.Substring(commentStartIndex + commentEndIndex + 1);
            }
            //this only happens if theres a comment at the end of the file
            else
            {
                RawLanguageInfo = RawLanguageInfo.Substring(0, commentStartIndex);
            }
        }

        //Now, remove all /* */ comments
        while (RawLanguageInfo.Contains("/*"))
        {
            int commentStartIndex = RawLanguageInfo.IndexOf("/*");
            int commentEndIndex = RawLanguageInfo.Substring(commentStartIndex).IndexOf("*/");

            if (commentEndIndex != -1)
            {
                RawLanguageInfo = RawLanguageInfo.Substring(0, commentStartIndex) + RawLanguageInfo.Substring(commentStartIndex + commentEndIndex + 2);
            }
            //If we're here there's an error in the text file. Everything after the /* is going to be destroyed. before parsing.
            else
            {
                Debug.LogError("File parsing error. Found /* but no pairing */");
                RawLanguageInfo = RawLanguageInfo.Substring(0, commentStartIndex);
            }
        }

        //Now that all comments are removed we can begin parsing and making the subtitle data structures
        while (RawLanguageInfo != null)
        {
            //We're removing everything before the next entry to trim whitespace and any other things that shouldn't be there.
            if (RawLanguageInfo.Contains("["))
            {
                RawLanguageInfo = RawLanguageInfo.Substring(RawLanguageInfo.IndexOf('['));
            }

            int startAudioWrapperNameIndex = RawLanguageInfo.IndexOf('[');
            int endAudioWrapperNameIndex = RawLanguageInfo.IndexOf(']');

            //No more info to pull so we are done.
            if ((startAudioWrapperNameIndex == -1 && endAudioWrapperNameIndex == -1) == true)
            {
                RawLanguageInfo = null;
                continue;
            }

            //get the name of our wrapper
            string EntryAudioWrapperName = RawLanguageInfo.Substring(startAudioWrapperNameIndex + 1, endAudioWrapperNameIndex - 1);

            //this is purely for safety purposes in case theres a typo but if there was an issue with the bracketing this will catch it and crash the parsing since the file is broken.
            if (EntryAudioWrapperName.Contains("\n"))
            {
                Debug.LogError(string.Format("Language file parse ERROR: bracket formatting broken with potential AudioWrapper name {0}.", EntryAudioWrapperName));
                RawLanguageInfo = null;
                continue;
            }

            //this checks that there's a matching exit bracket for the entry.
            if (RawLanguageInfo.Contains(string.Format("[/{0}/]", EntryAudioWrapperName)) == false)
            {
                Debug.LogError(string.Format("Language file parse ERROR: no exiting bracket for AudioWrapper name {0}.", EntryAudioWrapperName));
                RawLanguageInfo = null;
                continue;
            }
            
            //skip past the name line.
            RawLanguageInfo = RawLanguageInfo.Substring(RawLanguageInfo.IndexOf('\n') + 1);

            //now we parse each line inside our data and add it to the array
            string DataStructure = RawLanguageInfo.Substring(0, RawLanguageInfo.IndexOf(string.Format("[/{0}/]", EntryAudioWrapperName)) + 4 + EntryAudioWrapperName.Length);
            string[] lines = DataStructure.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            //If there is no actual subtitle info then we can skip this one.
            if (lines.Length <= 1)
            {
                Debug.LogWarning(string.Format("Language file parse warning: object {0} is defined but contains no subtitle information within.", EntryAudioWrapperName));
            }
            else
            {
                SubtitleInfo EntrySubtitleInfo = new SubtitleInfo(lines.Length - 1);
                bool successfulParse = true;

                //Each line should be a piece of the subtitle information.
                for(int i = 0; i < lines.Length - 1; i++)
                {
                    int colonIndex = lines[i].IndexOf(':');

                    //If a colon is missing, flag this structure as a failure.
                    if (colonIndex == -1)
                    {
                        Debug.LogWarning(string.Format("Language file parse warning: line {0} does not contain a colon delimiting the float and string in object {1}", lines[i], EntryAudioWrapperName));
                        successfulParse = false;
                        break;
                    }

                    //If the float could not be parsed, flag this structure as a failure.
                    if (float.TryParse(lines[i].Substring(0, colonIndex), out float SubtitleDuration) == false)
                    {
                        Debug.LogWarning(string.Format("Language file parse warning: float could not be parsed from within object: {0}, line: {1}", EntryAudioWrapperName, lines[i]));
                        successfulParse = false;
                        break;
                    }

                    string SubtitleLine = lines[i].Substring(colonIndex + 1);

                    EntrySubtitleInfo.SubtitleDurations[i] = SubtitleDuration;
                    EntrySubtitleInfo.SubtitleLines[i] = SubtitleLine;
                }

                //If this data structure parsed without any errors, then the data is good and we can add it.
                if (successfulParse)
                {
                    //Add the data structure to the dictionary
                    LoadedSubtitleLanguageData[EntryAudioWrapperName] = EntrySubtitleInfo;
                }
            }

            //Remove all the data parsed from the raw data
            RawLanguageInfo = RawLanguageInfo.Substring(RawLanguageInfo.IndexOf(string.Format("[/{0}/]", EntryAudioWrapperName)) + 4 + EntryAudioWrapperName.Length);
        }

    }

    /// <summary>
    /// Does the loaded subtitle data contain an entry for the supplied AudioWrapper?
    /// </summary>
    /// <param name="AudioWrapperName"></param>
    /// <returns></returns>
    public static bool ContainsSubtitleData(string AudioWrapperName)
    {
        if (Instance().LoadedSubtitleLanguageData != null)
        {
            return Instance().LoadedSubtitleLanguageData.ContainsKey(AudioWrapperName);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Does the loaded subtitle data contain an entry for the supplied AudioWrapper?
    /// </summary>
    /// <param name="AudioWrapperData"></param>
    /// <returns></returns>
    public static bool ContainsSubtitleData(AudioWrapper AudioWrapperData)
    {
        return ContainsSubtitleData(AudioWrapperData.name);
    }

    public static string GetSubtitleLanguage()
    {
        return Instance().CurrentLanguageSetting;
    }

}

[System.Serializable]
public struct SubtitleInfo
{
    public string[] SubtitleLines;
    public float[] SubtitleDurations;

    public SubtitleInfo(int NumberOfEntries)
    {
        SubtitleLines = new string[NumberOfEntries];
        SubtitleDurations = new float[NumberOfEntries];
    }
}