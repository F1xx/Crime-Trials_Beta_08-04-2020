using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Sets the player's RespawnPoint to this object's position
[RequireComponent(typeof(AudioChannel))]
[RequireComponent(typeof(Collider))]
public class RespawnPoint : BaseObject
{
    [Header("Banners")]
    [Tooltip("Banners (or whatever you want this to be) that will change color once Respawn point is active")]
    public List<GameObject> BannersToChange = new List<GameObject>();
    [Tooltip("Material to show on banners when point hasn't been hit yet")]
    public Material BannerUntouchedMaterial = null;
    [Tooltip("Material to show on banners when point is hit")]
    public Material BannerTouchedMaterial = null;
    [Tooltip("Material to show on banners when point is Off")]
    public Material BannerDeactivatedMaterial = null;

    [Header("Banner Bodies")]
    [Tooltip("Banners (or whatever you want this to be) that will change color once Respawn point is active")]
    public List<GameObject> BannerBodiesToChange = new List<GameObject>();
    [Tooltip("Material to show on banners when point hasn't been hit yet")]
    public Material BannerBodyUntouchedMaterial = null;
    [Tooltip("Material to show on banners when point is hit")]
    public Material BannerBodyTouchedMaterial = null;
    [Tooltip("Material to show on banners when point is Off")]
    public Material BannerBodyDeactivatedMaterial = null;

    [Header("Ground")]
    [Tooltip("Mesh that is the ground cause we change that too.")]
    public GameObject GroundMesh = null;
    [Tooltip("Material to show on ground when point hasn't been hit yet")]
    public Material GroundUntouchedMaterial = null;
    [Tooltip("Material to show on ground when point is hit")]
    public Material GroundTouchedMaterial = null;
    [Tooltip("Material to show after Checkpoint has been Deactivated")]
    public Material GroundDeactivatedMaterial = null;

    [Header("Audio")]
    AudioChannel m_AudioChannel = null;
    public ScriptableAudio ReachedCheckpointSounds = null;

    [Header("Misc")]
    [Tooltip("Location Where the player will Respawn from")]
    public GameObject PositionToRespawn = null;
    [Tooltip("If the level was done in perfect order, which checkpoint would this be in that order?")]
    public int RespawnOrderInLevel = 0;
    bool m_HasBeenActivatedAlready = false;

    protected override void Awake()
    {
        base.Awake();
        m_AudioChannel = GetComponent<AudioChannel>();

        if (m_AudioChannel == null)
        {
            Debug.LogError("Checkpoint failed to find AudioChannel");
        }
    }

    void Start()
    {
        //set everything to untouched
        Reset();
    }

    /// <summary>
    /// NOTE THIS REQUIRES A TRIGGER ON THE CHECKPOINT
    /// </summary>
    /// <param name="other">What collided with us</param>
    void OnTriggerEnter(Collider other)
    {
        if (m_HasBeenActivatedAlready == false)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.SetRespawnPoint(this);

                if (ReachedCheckpointSounds)
                    m_AudioChannel.PlayAudio(ReachedCheckpointSounds);
            }
        }
    }

    public void SetRespawnPoint(ref Vector3 pos, ref Quaternion rot)
    {
        pos = PositionToRespawn.transform.position;
        rot = PositionToRespawn.transform.rotation;

        SetActive();
    }

    public void SetInactive()
    {
        ChangeMaterials(BannersToChange, BannerDeactivatedMaterial);
        ChangeMaterials(BannerBodiesToChange, BannerBodyDeactivatedMaterial);
        ChangeMaterials(GroundMesh, GroundDeactivatedMaterial);

        ParticleSystem sys = GetComponentInChildren<ParticleSystem>();
        if(sys)
        {
            sys.Stop();
        }
    }

    public void SetActive()
    {
        m_HasBeenActivatedAlready = true;

        ChangeMaterials(BannersToChange, BannerTouchedMaterial);
        ChangeMaterials(BannerBodiesToChange, BannerBodyTouchedMaterial);
        ChangeMaterials(GroundMesh, GroundTouchedMaterial);
    }

    void ChangeMaterials(GameObject objToChange, Material ChangeToMaterial)
    {
        objToChange.GetComponent<Renderer>().material = ChangeToMaterial;
    }

    void ChangeMaterials(List<GameObject> objsToChange, Material ChangeToMaterial)
    {
        foreach (GameObject obj in objsToChange)
        {
            obj.GetComponent<Renderer>().material = ChangeToMaterial;
        }
    }

    private void Reset()
    {
        m_HasBeenActivatedAlready = false;
        ChangeMaterials(BannersToChange, BannerUntouchedMaterial);
        ChangeMaterials(BannerBodiesToChange, BannerBodyUntouchedMaterial);
        ChangeMaterials(GroundMesh, GroundUntouchedMaterial);

        ParticleSystem sys = GetComponent<ParticleSystem>();
        if (sys)
        {
            sys.Play();
        }
    }

    protected override void OnHardReset()
    {
        Reset();
    }
}
