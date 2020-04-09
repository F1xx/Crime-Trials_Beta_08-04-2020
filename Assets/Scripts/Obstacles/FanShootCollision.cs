using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanShootCollision : MonoBehaviour
{
    FanHazard Fan = null;
    FanHealthComponent Health = null;
    //[SerializeField]
    //bool ShouldDealDamage = true;

    private void Start()
    {
        Fan = GetComponentInParent<FanHazard>();
        Health = GetComponentInParent<FanHealthComponent>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Projectile"))
        {
            Health.OnTakeDamage(100.0f, collision.collider.GetComponent<ShootableBase>().ParentObject);
            return;
        }

        //commented out because it wasn't hitting the player
        //if (ShouldDealDamage)
        //{
        //    HealthComponent hp = collision.collider.gameObject.transform.root.gameObject.GetComponent<HealthComponent>();

        //    if (hp)
        //    {
        //        hp.OnTakeDamage(1000, gameObject.transform.root.gameObject, true);
        //    }
        //}
    }
}
