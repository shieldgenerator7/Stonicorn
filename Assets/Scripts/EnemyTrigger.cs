﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour {

    [Tooltip("Define it in the editor, or leave it blank to be auto-populated by the enemies in the trigger area")]
    public List<EnemySimple> enemies;

    void Start()
    {
        if (enemies.Count == 0)
        {
            RaycastHit2D[] rch2ds = new RaycastHit2D[Utility.MAX_HIT_COUNT];
            Utility.RaycastAnswer answer = Utility.Cast(GetComponent<BoxCollider2D>(), Vector2.zero, rch2ds, 0, true);
            for (int i = 0; i < answer.count; i++){
                RaycastHit2D rch2d = answer.rch2ds[i];
                if (!rch2d.collider.isTrigger)
                {
                    EnemySimple es = rch2d.collider.gameObject.GetComponent<EnemySimple>();
                    if (es != null)
                    {
                        enemies.Add(es);
                    }
                }
            }
        }
        foreach (EnemySimple es in enemies)
        {
            es.activeMove = false;
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == GameManager.playerTag)
        {
            foreach (EnemySimple es in enemies)
            {
                es.activeMove = true;
            }
        }
    }
}
