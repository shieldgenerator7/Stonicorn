using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceLaunchProjectile : MonoBehaviour
{
    private void Start()
    {
        GetComponent<ForceLaunchAbility>().setOnFire();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.isSolid())
        {
            Managers.Object.destroyObject(gameObject);
        }
    }
}
