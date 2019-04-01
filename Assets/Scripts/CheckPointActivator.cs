using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckPointActivator : MonoBehaviour
{
    //Place this on an object with a Collider
    // w/ isTrigger checked
    //and whose parent is a Checkpoint_Root

    private void Start()
    {
        //Check for errors
        if (transform.parent == null)
        {
            throw new UnityException("The object ("+name+") has no parent! parent: " + transform.parent);
        }
        if (transform.parent.GetComponent<CheckPointChecker>() == null)
        {
            throw new UnityException("The child object (" + gameObject.name + ") has a parent object (" + transform.parent.name + ") with no CheckPointChecker!");
        }
        if (GetComponent<Collider2D>() == null)
        {
            throw new UnityException("The object ("+name+") has no Collider2D!");
        }
        bool foundTrigger = false;
        foreach (Collider2D coll2d in GetComponents<Collider2D>())
        {
            if (coll2d.isTrigger)
            {
                foundTrigger = true;
                break;
            }
        }
        if (!foundTrigger)
        {
            throw new UnityException("The object ("+name+") has no trigger collider!");
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (GameManager.isPlayer(coll.gameObject))
        {
            transform.parent.GetComponent<CheckPointChecker>().activate();
        }
    }
}
