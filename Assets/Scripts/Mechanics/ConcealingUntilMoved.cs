using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcealingUntilMoved : MonoBehaviour
{
    public List<HiddenArea> haListToUncover;

    private Vector2 startPos;
    private StaticUntilTouched staticUntilTouched;

    private void Start()
    {
        //Record startPos
        startPos = transform.position;
        //Register with staticUntilTouched (if available)
        staticUntilTouched = GetComponent<StaticUntilTouched>();
        if (staticUntilTouched)
        {
            staticUntilTouched.onRootedChanged += (rooted) =>
            {
                if (!rooted)
                {
                    revealHiddenAreas();
                }
            };
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        checkRevealHiddenAreas();
    }

    private void checkRevealHiddenAreas()
    {
        Vector2 currentPosition = transform.position;
        if (currentPosition != startPos)
        {
            revealHiddenAreas();
        }
    }

    private void revealHiddenAreas()
    {
        haListToUncover.ForEach(ha =>
        {
            if (ha)
            {
                ha.Discovered = true;
            }
        });
        Destroy(this);
    }
}
