using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapTrail : MonoBehaviour {
    //put a lot of these together to make a trail of tap target highlights

    public GameObject nextTapStone;//the next piece of the trail

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.isPlayer())
        {
            if (nextTapStone != null)
            {
                Managers.Effect.highlightTapArea(nextTapStone.transform.position);
            }
            else
            {
                Managers.Effect.highlightTapArea(Vector2.zero, false);
            }
        }
    }
}
