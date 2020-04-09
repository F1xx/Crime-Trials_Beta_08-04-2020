using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillBox : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Collider m_ObjectCollider;

        m_ObjectCollider = GetComponent<Collider>();

        if (m_ObjectCollider)
        {
            m_ObjectCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Kill(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Kill(collision.gameObject);
    }

    private void Kill(GameObject obj)
    {
        HealthComponent health = obj.GetComponent<HealthComponent>();

        if (health != null)
        {
            health.Kill();
        }
        else
        {
            Destroy(obj);
        }
    }
}
