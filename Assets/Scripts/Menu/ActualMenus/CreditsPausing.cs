using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsPausing : MonoBehaviour
{
    public AudioChannel MusicChannel = null;

    private void OnEnable()
    {
        MusicChannel.PauseAudioWrapper(true);
    }

    private void OnDisable()
    {
        MusicChannel.PauseAudioWrapper(false);
    }
}
