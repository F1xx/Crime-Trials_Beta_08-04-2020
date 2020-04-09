using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "RandomAudioListPrefab", menuName = "ScriptableObjects/AudioList/RandomAudioList", order = 1)]
public class RandomAudioList : AudioList
{
    protected override ScriptableAudio OnGetAudioObject()
    {
        int randnum = Random.Range(0, AudioObjects.Count - 1);
        return AudioObjects[randnum];
    }
}