using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        //Error checking: soft check for at least one valid HA
        if (!haListToUncover.Any(ha => ha))
        {
            Debug.LogWarning($"ConcealingUntilMoved script on gameobject {gameObject.name} has no HiddenAreas to reveal!");
            Destroy(this);
            return;
        }
        //Record concealRange
        Vector2 size = gameObject.getSize();
        concealRange = Mathf.Min(size.x, size.y);
        //Record startPos
        startPos = transform.position;
        //Register with staticUntilTouched (if available)
        staticUntilTouched = GetComponent<StaticUntilTouched>();
        if (staticUntilTouched)
        {
            staticUntilTouched.onRootedChanged += onRootedChanged;
            this.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (staticUntilTouched)
        {
            staticUntilTouched.onRootedChanged -= onRootedChanged;
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

    private void onRootedChanged(bool rooted)
    {
        if (!rooted)
        {
            checkRevealHiddenAreas();
            this.enabled = true;
        }
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
