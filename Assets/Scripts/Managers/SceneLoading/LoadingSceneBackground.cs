using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Linq;

public class LoadingSceneBackground : MonoBehaviour
{
    private VideoPlayer m_VideoPlayer = null;
    private RawImage LoadingImage = null;
    public List<Texture> LoadingImages = new List<Texture>();
    public List<VideoClip> LoadingClips = new List<VideoClip>();

    // Start is called before the first frame update
    void Start()
    {
        LoadingImage = GetComponent<RawImage>();
        m_VideoPlayer = GetComponent<VideoPlayer>();
        m_VideoPlayer.isLooping = true;

        int AnimatedorNotAnimated = Random.Range(0, 2);

        if(AnimatedorNotAnimated == 0 && LoadingClips.Count > 0)
        {
            int RandImage = Random.Range(0, LoadingClips.Count);
            m_VideoPlayer.clip = LoadingClips[RandImage];
        }
        else if (LoadingImages.Count > 0)
        {
            int RandImage = Random.Range(0, LoadingImages.Count);
            LoadingImage.texture = LoadingImages[RandImage];
        }
    }
}
