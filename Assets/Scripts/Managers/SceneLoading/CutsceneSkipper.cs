using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneSkipper : MonoBehaviour
{
    public void DisablePanel()
    {
        this.gameObject.SetActive(false);
    }
}
