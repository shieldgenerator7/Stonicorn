using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverableActivator : MonoBehaviour, ISwappable
{
    public MemoryMonoBehaviour mmb;

    [AutoInitialize(AllowUnfound = true), SerializeField, HideInInspector]
    private Hazard hazard;

    private void Start()
    {
        mmb.onDiscovered += () => Destroy(this);
    }

    private void OnCollisionEnter2D(Collision2D coll2D)
    {
        if (coll2D.collider.isPlayerSolid() || coll2D.collider.GetComponent<PlayerPilotController>())
        {
            if (isSafeToCollect(coll2D.gameObject, coll2D.contacts[0].point))
            {
                mmb.Discovered = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D coll2D)
    {
        //early exit: it's off
        if (!mmb || !mmb.enabled || !mmb.gameObject.activeSelf) { return; }
        //processing
        if (coll2D.isPlayerSolid() || coll2D.GetComponent<PlayerPilotController>())
        {
            //if (isSafeToCollect(coll2D.gameObject, coll2D.transform.position))
            //{
            mmb.Discovered = true;
            //}
        }
    }

    public void nowSwapped()
    {
        mmb.Discovered = true;
    }

    private bool isSafeToCollect(GameObject go, Vector2 point)
    {
        PlayerController pc = go.GetComponent<PlayerController>();
        return !pc.canBeHitByHazard(hazard, point);
    }
}
