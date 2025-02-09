using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckPointActivator : MonoBehaviour, ISetupable
{
    //Place this on an object with a Collider
    // w/ isTrigger checked
    //and whose parent is a Checkpoint_Root


    [AutoInitialize(SearchParent =true), SerializeField, HideInInspector]
    private CheckPointChecker checker;

    private void Start()
    {
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.isPlayer())
        {
            checker.activate();
        }
    }

    public int setup()
    {
        //Check for errors
        if (transform.parent == null)
        {
            throw new UnityException("The object ("+name+") has no parent! parent: " + transform.parent);
        }
        if (checker == null)
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

        return 0;
    }
}
