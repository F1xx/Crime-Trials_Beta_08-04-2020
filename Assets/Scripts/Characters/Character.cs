using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(InputComponentBase))]
[RequireComponent(typeof(AudioChannel))]
public abstract class Character : BaseObject
{
    [Header("Feet")]
    [SerializeField, Tooltip("If this object cares about walking along the ground reference the foot location object here.")]
    private GameObject _BaseLocationObj;

    [Header("Spawning")]
    public Vector3 SpawnPoint;
    public Quaternion SpawnRot;

    public GameObject BaseLocationObj { get { return _BaseLocationObj; } set { _BaseLocationObj = value; } }

    [HideInInspector] public HealthComponent m_HealthComp { get; protected set; }
    [HideInInspector] public InputComponentBase m_InputComponent { get; protected set; }
    [HideInInspector] public AudioChannel m_AudioSource { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        //create/setup components and handles for them
        m_HealthComp = GetComponent<HealthComponent>();
        m_InputComponent = GetComponent<InputComponentBase>();
        m_AudioSource = GetComponent<AudioChannel>();
        
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        ParentObject = gameObject;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public virtual void Respawn(bool forced = false)
    {

    }
}
