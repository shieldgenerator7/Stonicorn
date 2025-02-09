using System.Linq;
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
        return 0;
    }

    public int checkForErrors()
    {
        int errorCount = 0;

        //Check for errors
        if (transform.parent == null)
        {
            Debug.LogError($"The object ({name}) has no parent! parent: {transform.parent}");
            errorCount++;
        }
        if (checker == null)
        {
            Debug.LogError($"The child object ({gameObject.name}) has a parent object ({transform.parent.name}) with no CheckPointChecker!");
            errorCount++;
        }
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError($"The object ({name}) has no Collider2D!");
            errorCount++;
        }
        bool foundTrigger = GetComponents<Collider2D>()
            .Any(coll2d => coll2d.isTrigger);
        if (!foundTrigger)
        {
            Debug.LogError($"The object ({name}) has no trigger collider!");
            errorCount++;
        }

        return errorCount;
    }
}
