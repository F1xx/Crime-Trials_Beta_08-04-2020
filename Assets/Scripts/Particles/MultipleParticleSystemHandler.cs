using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleParticleSystemHandler : BaseObject
{
    ParticleSystem[] ParticleSystems = null;

    /// <summary>
    /// Play all particle effects in this GameObject. Systems are grabbed from the object pool.
    /// Systems will deactivate and return to the pool once finished playing.
    /// </summary>
    /// <param name="pos">the position the particles will be spawned from</param>
    public void Play(Vector3 pos)
    {
        ParticleSystems = GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem sys in ParticleSystems)
        {
            //get an instance of the particle from the pool
            PooledObject pooled = ObjectPoolManager.Get(sys.gameObject);
            pooled.gameObject.transform.position = pos;

            //get the actual system from the pooled object
            ParticleSystem ps = pooled.gameObject.GetComponent<ParticleSystem>();
            //tell it to kill itself
            pooled.SetSelfDeactivation(ps.main.duration);
            //start playing
            ps.Play();
        }
    }
}
