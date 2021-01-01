using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class SecretAreaTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            GetComponentInParent<HiddenArea>()
                .Discovered = true;
        }
    }
}