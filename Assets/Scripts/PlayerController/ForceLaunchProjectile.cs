using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceLaunchProjectile : MonoBehaviour
{

    [AutoInitialize, SerializeField, HideInInspector]
    private ForceLaunchAbility forceLaunchAbility;

    private void Start()
    {
        forceLaunchAbility.setOnFire();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.isSolid())
        {
            Managers.Object.destroyObject(gameObject);
        }
    }
}
