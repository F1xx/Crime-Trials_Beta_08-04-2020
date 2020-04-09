
// Code for shooting a bullet

using UnityEngine;

public class ShootBullet : ShootableBase
{
    [Header("BulletSettings"), SerializeField, Tooltip("This is the multiplier for the force the bullets apply to RigidBodies")]
    protected float Damage = 100.0f;
    //[SerializeField]
    //float CollisionForce = 30.0f;
    
    [Header("Bullet ParticleSystems"), SerializeField]
    protected GameObject MuzzlePrefab = null; // muzzle flash prefab that appears at gun barrel
    [SerializeField]
    protected GameObject HitPrefab = null; // the little hit effect when bullet hits something

    private PooledObject m_MuzzleInstance = null; // muzzle flash prefab that appears at gun barrel
    private PooledObject m_HitInstance = null; // the little hit effect when bullet hits something

    protected override void Init()
    {
        TrailRenderer renderer = GetComponentInChildren<TrailRenderer>();
        if (renderer)
        {
            renderer.Clear();
        }

        // Every time a bullet is created, create this muzzle flash 
        // Destroy it after the time set in the inspector
        if (MuzzlePrefab != null)
        {
            m_MuzzleInstance = ObjectPoolManager.Get(MuzzlePrefab, transform.position, Quaternion.identity);
            m_MuzzleInstance.gameObject.transform.forward = gameObject.transform.forward;
            var psMuzzle = m_MuzzleInstance.gameObject.GetComponent<ParticleSystem>();
            if (psMuzzle != null)
            {
                m_MuzzleInstance.SetSelfDeactivation(psMuzzle.main.duration);
            }
            else
            {
                var psChild = m_MuzzleInstance.gameObject.transform.GetChild(0).GetComponent<ParticleSystem>();
                m_MuzzleInstance.SetSelfDeactivation(psChild.main.duration);
            }
        }

        if (Speed <= 0)
        {
            Debug.Log("No Speed");
            Speed = 50.0f;
        }

        GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * Speed, ForceMode.Impulse);
    }

    //require LateUpdate if trying to match position
    private void LateUpdate()
    {
        if(m_MuzzleInstance != null)
        {
            m_MuzzleInstance.gameObject.transform.position = BarrelPosition.transform.position;
            m_MuzzleInstance.gameObject.transform.forward = BarrelPosition.transform.forward;
        }
    }

    // Destroy bullet when it hits something with a collider
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == ParentObject.name)
        {
            return;
        }

        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

        DealDamage(collision);

        if (HitPrefab != null)
        {
            m_HitInstance = ObjectPoolManager.Get(HitPrefab, pos, rot);
            // Same thing as the muzzle destroy
            var psHit = m_HitInstance.gameObject.GetComponent<ParticleSystem>();
            if (psHit != null)
            {
                m_HitInstance.SetSelfDeactivation(psHit.main.duration);
            }
            else
            {
                var psChild = m_HitInstance.gameObject.transform.GetChild(0).GetComponent<ParticleSystem>();
                m_HitInstance.SetSelfDeactivation(psChild.main.duration);
            }
        }

        Pool.Deactivate();
    }

    protected virtual void DealDamage(Collision collision)
    {
        if(Damage == 0.0f)
        {
            return;
        }

        HealthComponent hp = collision.gameObject.GetComponent<HealthComponent>();
        if (hp)
        {
            if (Damage > 0.0f)
            {
                hp.OnTakeDamage(Damage, ParentObject);
            }
            else
            {
                hp.Heal(Damage);
            }
        }

    }
}

