using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : Singleton<CutsceneManager>
{
    public VideoPlayer videoPlayer;
    public List<VideoClip> Cutscenes;
    int CurrentCutsceneIndex = 0; 
    [SerializeField]
    TMPro.TMP_Text SkipText = null;

    Timer SkipTimer = null;
    [SerializeField]

    
    bool CutscenesHaveAlreadyPlayed = false;

    [Header("Play Options")]
    public bool ShouldNeverPlayCutscenes = false;
    public bool ShouldPlayCutscenesOnce = false;
    protected override void OnAwake()
    {
        CheckIfSceneHasBeenPlayed();
        SkipTimer = CreateTimer(5.0f, HideSkipButtons);

        Listen("OnGamePaused", PauseCutscene);
        Listen("OnGameUnpaused", ResumeCutscene);
        Listen("OnToggleCutsceneNeverPlay", SetCutscenesNeverPlay);
        Listen("OnToggleCutscenePlayOnce", SetCutscenesPlayOnce);

        videoPlayer.clip = Cutscenes[CurrentCutsceneIndex];

        HideSkipButtons();
        LoadFromSettings();

        if(ShouldNeverPlayCutscenes)
        {
            DontPlayCutscenes();
        }
        else if (ShouldPlayCutscenesOnce)
        {
            if(CutscenesHaveAlreadyPlayed)
                DontPlayCutscenes();
        }

    }

    void LoadFromSettings()
    {
        if (PlayerPrefs.HasKey("OnToggleCutsceneNeverPlay"))
            ShouldNeverPlayCutscenes = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleCutsceneNeverPlay"));
        if (PlayerPrefs.HasKey("OnToggleCutscenePlayOnce"))
            ShouldPlayCutscenesOnce = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleCutscenePlayOnce"));

    }

    // Start is called before the first frame update
    void Start()
    {
        if(videoPlayer != null)
        {
            EventManager.TriggerEvent("CutsceneStart");
        }
        videoPlayer.loopPointReached += CutsceneFinished;
    }

    // Update is called once per frame
    void Update()
    {
        
         if (Input.anyKeyDown)
         {
             if (Input.GetButtonDown("Pause"))
             {
                 return;
             }
            if (SkipTimer.IsRunning == false)
            {
                SkipTimer.Restart();
                ShowSkipButtons();
                return;
            }
         }

         if (Input.GetButtonDown("Skip") && SkipTimer.IsRunning)
         {
            if (IsACutscenePlaying())
            {
                CutsceneFinished(videoPlayer);
                SkipTimer.StopTimer();
                HideSkipButtons();
            }
         }

    }

    bool IsACutscenePlaying()
    {
        return videoPlayer.isPlaying;
    }

    void CutsceneFinished(UnityEngine.Video.VideoPlayer vp)
    {
        if (vp != null)
        {
            if ((CurrentCutsceneIndex + 1) < Cutscenes.Count)
            {
                videoPlayer.clip = Cutscenes[CurrentCutsceneIndex + 1];
                CurrentCutsceneIndex++;
            }
            else
            {
                EventManager.TriggerEvent("CutsceneFinsished");
                vp.SendMessageUpwards("DisablePanel");
                if(ShouldPlayCutscenesOnce)
                {
                    CutscenesHaveAlreadyPlayed = true;
                }
            }
        }
        else
        {
            Debug.LogError("There is no Video Player to finish the cutscene");
        }
    }

    void DontPlayCutscenes()
    {
        videoPlayer.SendMessageUpwards("DisablePanel");
    }

    void PauseCutscene()
    {
        if (videoPlayer != null)
        {
            //pause the cutscene that is playing
            if (IsACutscenePlaying())
                videoPlayer.Pause();
        }
        else
        {
            Debug.LogError("There is no Video Player to pause a cutscene on");
        }
    }

    void ResumeCutscene()
    {
        if (videoPlayer != null)
        {
            //unpause cutscene without restarting 
            if (videoPlayer.isPaused)
                videoPlayer.Play();
        }
        else
        {
            Debug.LogError("There is no Video Player to unpause a cutscene on");

        }
    }

    void CheckIfSceneHasBeenPlayed()
    {
        //name check
         List<string> sceneNames = SceneLoader.SceneNames;
        //because the level names or indices are added by the scene loader befoire this is hit
        //if there is only one name in the names list that means the only level to be loaded has 
        //either been level one or tutorial level from main menu, if player hits resart or next level it will add to the index list 
        //so the check will be done there 
        if (sceneNames.Count != 0 && sceneNames.Count != 1)
        {
            foreach (string sceneName in sceneNames)
            {
                //if scene has already been played, cutscenes(if any) have already been watched or skipped  
                //so set that this scenes cutscenes have already been watched and should be skipped automatically if needed
                if (sceneName == SceneManager.GetActiveScene().name)
                {
                    CutscenesHaveAlreadyPlayed = true;
                    return;
                }
            }
        }
        //index check
        List<int> sceneIndices = SceneLoader.SceneIndices;
        if (sceneIndices.Count != 0)
        {
            foreach (int sceneIndex in sceneIndices)
            {
                //if scene has already been played, cutscenes(if any) have already been watched or skipped  
                //so set that this scenes cutscenes have already been watched and should be skipped automatically if needed          
                if (sceneIndex == SceneManager.GetActiveScene().buildIndex)
                {
                    CutscenesHaveAlreadyPlayed = true;
                    return;
                }
            }
        }
        //if scene name or index doesnt match any in the lists then scene is being played for the first time 
        CutscenesHaveAlreadyPlayed = false;
    }

    void SetCutscenesNeverPlay()
    {
        ShouldNeverPlayCutscenes = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleCutsceneNeverPlay"));
    }

    void SetCutscenesPlayOnce()
    {
        ShouldPlayCutscenesOnce = System.Convert.ToBoolean(PlayerPrefs.GetInt("OnToggleCutscenePlayOnce"));
    }

    void ShowSkipButtons()
    {
        SkipText.alpha = 1.0f;
    }

    void HideSkipButtons()
    {
        SkipText.alpha = 0.0f;

    }

}
