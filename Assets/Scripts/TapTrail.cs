using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapTrail : MonoBehaviour {
    //put a lot of these together to make a trail of tap target highlights

    public GameObject nextTapStone;//the next piece of the trail

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == GameManager.playerTag)
        {
            if (nextTapStone != null)
            {
                EffectManager.highlightTapArea(nextTapStone.transform.position);
            }
            else
            {
                EffectManager.highlightTapArea(Vector2.zero, false);
            }
        }
    }
}
