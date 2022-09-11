using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcealingUntilMoved : MonoBehaviour
{
    public List<HiddenArea> haListToUncover;

    /// <summary>
    /// As long as the object is within this distance of its starting position, it does not reveal the hidden areas
    /// </summary>
    private float concealRange;
    private Vector2 startPos;
    private StaticUntilTouched staticUntilTouched;

    private void Start()
    {
        //Record concealRange
        Vector2 size = gameObject.getSize();
        concealRange = Mathf.Min(size.x, size.y);
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
                    checkRevealHiddenAreas();
                    this.enabled = true;
                }
            };
            this.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        checkRevealHiddenAreas();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        checkRevealHiddenAreas();
        this.enabled = true;
    }

    private void checkRevealHiddenAreas()
    {
        float distance = Vector2.Distance(transform.position, startPos);
        if (distance > concealRange)
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
