using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Linq;

public class SceneLoader : Singleton<SceneLoader>
{

    private float _Progress = 0.0f;
    public static float Progress { get { return Instance()._Progress; } private set { Instance()._Progress = value; } }

    [HideInInspector]
    static public List<string> SceneNames = new List<string>();
    [HideInInspector]
    static public List<int> SceneIndices = new List<int>();

    //VideoPlayer videoPlayer;
    protected override void OnAwake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void GoToLoadingScreen(string scene)
    {
        SceneManager.LoadScene("LoadingScene");
        LoadScene(scene);
    }

    public static void GoToLoadingScreen(int sceneIndex)
    {
        SceneManager.LoadScene("LoadingScene");
        LoadScene(sceneIndex);
    }


    public static void LoadScene(string scene)
    {
        Instance().StartCoroutine(Instance().LoadSceneAsync(scene));
        if (SceneNames.Count == 0)
        {
            //first scene so add this scene no matter what and skip foreach check
            SceneNames.Add(scene);
        }
        else
        {
            foreach (string sceneName in SceneNames)
            {
                //if it hasnt already been added add the current scene to this list to track which scenes the character has played already
                if (sceneName != scene)
                    SceneNames.Add(scene);
            }
        }
    }

    public static void LoadScene(int scene)
    {
        Instance().StartCoroutine(Instance().LoadSceneAsync(scene));
        if (SceneIndices.Count == 0)
        {
            //first scene so add this scene no matter what and skip foreach check
            SceneIndices.Add(scene);
        }
        else
        {
            foreach (int sceneName in SceneIndices)
            {
                //if it hasnt already been added add the current scene to this list to track which scenes the character has played already
                if (sceneName != scene)
                    SceneIndices.Add(scene);
            }
        }
    }

    IEnumerator LoadSceneAsync(int scene)
    {
        //just so you can actually see the loading bar move 
        yield return new WaitForSeconds(0.5f);
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(scene);
        while (!loadScene.isDone)
        {
            Progress = Mathf.Clamp01(loadScene.progress / 0.9f);

            yield return new WaitForEndOfFrame();
        }
        Progress = 0.0f;
    }

    IEnumerator LoadSceneAsync(string scene)
    {
        //just so you can actually see the loading bar move 
        yield return new WaitForSeconds(0.5f);
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(scene);
        while (!loadScene.isDone)
        {
            Progress = Mathf.Clamp01(loadScene.progress / 0.9f);

            yield return new WaitForEndOfFrame();
        }
        Progress = 0.0f;
    }
}