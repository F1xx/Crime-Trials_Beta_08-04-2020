using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SubtitlesNavigator : SideWaysNavigator
{

    const string FILE_KEY = "OnLanguageChange";

    protected override void Awake()
    {
        LoadAllSubtitleLanguageFileNames();
        base.Awake();
    }

    protected override void LoadSettings()
    {
        if(Options.Length < 1)
        {
            Debug.LogError(gameObject.name + " Options did not get loaded from file somehow. Major issue.");
        }

        if (PlayerPrefs.HasKey(FILE_KEY))
        {
            string defaultVal = PlayerPrefs.GetString(FILE_KEY);

            int index = FindIndexInOptions(defaultVal);

            //if we didn't find one reset to default
            if (index == -1)
            {
                CurrentIndex = PlayerPrefs.GetInt(PREF_KEY + "Default");

                //if our reset is bigger than the options size then set it to the start and that is the new default
                if (CurrentIndex > Options.Length)
                {
                    CurrentIndex = 0;
                    SaveDefaultAndValueToPlayerPrefs();
                }

                SaveValueToPlayerPrefs();
                BroadCastEvent();
                //Update the text and quit
                ChangeTextToIndex();
                return;
            }

            //we found it. Set it and leave
            CurrentIndex = index;
            SaveValueToPlayerPrefs();
        }
        else
        {
            //the key doesn't exist, lets make one.
            CurrentIndex = 0;
            SaveDefaultAndValueToPlayerPrefs();
        }

        ChangeTextToIndex();
    }

    int FindIndexInOptions(string toFind)
    {
        //find the saved value in the options
        for (int i = 0; i < Options.Length; i++)
        {
            if (Options[i] == toFind)
            {
                return i;
            }
        }

        return -1;
    }

    protected override void SaveValueToPlayerPrefs()
    {
        PlayerPrefs.SetInt(PREF_KEY, CurrentIndex);
        PlayerPrefs.SetString("OnLanguageChange", Options[CurrentIndex]);
        PlayerPrefs.Save();
    }

    private void LoadAllSubtitleLanguageFileNames()
    {
        string LanguageLoadFilePath = Application.dataPath + "/Resources/Language";
        //Check our languages directory assuming it exists
        if (Directory.Exists(LanguageLoadFilePath))
        {
            DirectoryInfo info = new DirectoryInfo(LanguageLoadFilePath);

            List<string> languageFiles = new List<string>();

            //Find our text files
            foreach (FileInfo item in info.GetFiles("*.txt"))
            {
                string rawName = item.Name;
                rawName = rawName.Remove(rawName.Length - 4);

                languageFiles.Add(rawName);
            }

            Options = languageFiles.ToArray();

            if(Options.Length == 0)
            {
                Options = new string[] { "No Options Found" };
            }
        }
        else
        {
            Debug.LogError("Error: Resources/Language folder does not exist.");
            return;
        }
    }


}
