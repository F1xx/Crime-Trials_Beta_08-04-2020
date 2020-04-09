
// Base Shoot component that is attached to an object
// Give it a projectile type to shoot
// Should be able to shoot that projectile e z p z
// In that projectile's prefab should be able to change anything someone may want to

using UnityEngine;

[RequireComponent(typeof(AudioChannel))]
public class ShootComponent : BaseObject
{
    [Header("Shooting Variables")]
    [Tooltip("This is where the Shootables will spawn from. They also inherit the rotation of the Fire Point.")]
    public GameObject FirePoint = null;
    [Tooltip("This is the bullet we will shoot.")]
    public ShootableBase ShootableObject = null;
    [Tooltip("The sound to play when a shot is fired.")]
    public ScriptableAudio AudioToPlayOnShotFired = null;

    [Range(0.0f, 50.0f), Tooltip("How many bullets are fired each second if the \"Fire\" key is held down.")]
    public float ShotsPerSecond = 2.0f;

    [HideInInspector] public AudioChannel m_AudioSource { get; protected set; }

    protected Timer m_RateOfFireTimer = null;
    protected bool m_CanShoot = true;

    protected override void Awake()
    {
        base.Awake();

        m_AudioSource = GetComponent<AudioChannel>();
        ParentObject = transform.root.gameObject;
    }

    void Start()
    {
        m_RateOfFireTimer = CreateTimer(1.0f / ShotsPerSecond, ResetFire);
    }

    //called by timer
    public void ResetFire()
    {
        m_CanShoot = true;
    }

    // Change shootable objects on the fly
    protected void ChangeShootableObject(ShootableBase newShootableObject)
    {
        ShootableObject = newShootableObject;
    }

    // Function that is called by whatever needs to shoot that this component is attached to
    public void Shoot()
    {
        if (m_CanShoot)
        {
            //TODO maybe shoot anim here? or override in player shoot 
            if (FirePoint != null)
            {
                //get bullet from pool
                PooledObject bullet = ObjectPoolManager.Get(ShootableObject.gameObject);

                //determine what direction it goes (overridden by player)
                Quaternion dir = DetermineDirection();

                //get the actual bullet part from the bullet
                ShootableBase shootBase = bullet.gameObject.GetComponent<ShootableBase>();
                //make it die if it doesn't hit anything for a while.
                bullet.SetSelfDeactivation(shootBase.LifeTime);

                //actually shoot it
                shootBase.Shoot(FirePoint, bullet, dir, ParentObject);

                if (AudioToPlayOnShotFired)
                {
                    m_AudioSource.PlayAudio(AudioToPlayOnShotFired);
                }
                m_RateOfFireTimer.StartTimer();
                m_CanShoot = false;
            }
            else
            {
                Debug.Log(gameObject.name + " has No fire point");
            }
        }
    }

    protected virtual Quaternion DetermineDirection()
    {
        return FirePoint.transform.rotation;
    }
}
