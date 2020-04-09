using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Same as RandomAudioList but uses weights to give more power to certain audio.
/// </summary>
[System.Serializable, CreateAssetMenu(fileName = "WeightedRandomAudioListPrefab", menuName = "ScriptableObjects/AudioList/WeightedRandomAudioList", order = 2)]
public class WeightedRandomAudioList : AudioList
{
    [SerializeField, Tooltip("Add weights here. Ensure the amount of weights matches the amount of ScriptableAudio in the AudioList.")]
    public List<int> AudioWrapperWeights = new List<int>();

    private int AllWeights = 0;

    protected override void OnEnable()
    {
        base.OnEnable();

        AllWeights = 0;
        foreach (int i in AudioWrapperWeights)
        {
            AllWeights += i;
        }
    }

    protected override ScriptableAudio OnGetAudioObject()
    {
        int randnum = Random.Range(0, AllWeights);

        ScriptableAudio foundAudio = null;

        int index = 0;
        int threshold = 0;

        while (foundAudio == null)
        {
            //if our number minus any previous weights is less than or equal to the next weight that is the selected sound.
            if (randnum - threshold <= AudioWrapperWeights[index])
            {
                foundAudio = AudioObjects[index];
            }
            //Move down the chain and increase the threshold.
            else
            {
                threshold += AudioWrapperWeights[index];
                index++;
            }

            if (index >= AudioWrapperWeights.Count)
            {
                Debug.LogWarning("Random int outside of range of weights.");
                return null;
            }
        }

        return foundAudio;
    }
}