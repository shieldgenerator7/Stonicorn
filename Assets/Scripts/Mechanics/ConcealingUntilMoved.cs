using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConcealingUntilMoved : MonoBehaviour, ISetupable
{
    public List<HiddenArea> haListToUncover;

    /// <summary>
    /// As long as the object is within this distance of its starting position, it does not reveal the hidden areas
    /// </summary>
    [SerializeField,HideInInspector]
    private float concealRange;
    [SerializeField,HideInInspector]
    private Vector2 startPos;
    [AutoInitialize(AllowUnfound =true), SerializeField, HideInInspector]
    private StaticUntilTouched staticUntilTouched;

    private void Start()
    {
        //Register with staticUntilTouched (if available)
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

    public int setup()
    {
        int changeCount = 0;

        //Record concealRange
        Vector2 size = gameObject.getSize();
        float newRange = Mathf.Min(size.x, size.y);
        if (newRange != concealRange)
        {
            concealRange = Mathf.Min(size.x, size.y);
            changeCount++;
        }

        //Record startPos
        Vector2 newPos = transform.position;
        if (newPos != startPos)
        {
            startPos = transform.position;
            changeCount++;
        }

        return changeCount;
    }
    public int checkForErrors()
    {
        int errorCount = 0;
        //Error checking: soft check for at least one valid HA
        if (!haListToUncover.Any(ha => ha))
        {
            Debug.LogWarning($"ConcealingUntilMoved script on gameobject {gameObject.name} has no HiddenAreas to reveal!");
            errorCount++;
        }
        return errorCount;
    }
}
