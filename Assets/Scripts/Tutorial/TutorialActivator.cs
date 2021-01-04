using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialActivator : MonoBehaviour
{

    public List<GameObject> objectsToActivate;

    [Tooltip("The list of triggers required for the objects to be activated.")]
    public List<ActivatorTrigger> requiredTriggers;

    // Start is called before the first frame update
    void Start()
    {
        requiredTriggers.ForEach(trigger => trigger.onTriggeredChanged += checkTriggers);

        //Error checking
#if UNITY_EDITOR
        if (objectsToActivate == null || objectsToActivate.Count <= 0)
        {
            Debug.LogError(
                "Tutorial Activator (" + gameObject.name
                + ") does not have any objects to activate.",
                gameObject
                );
        }
        if (requiredTriggers == null || requiredTriggers.Count <= 0)
        {
            Debug.LogError(
                "Tutorial Activator (" + gameObject.name
                + ") does not have any triggers to activate it.",
                gameObject
                );
        }
        Collider2D coll2d = GetComponent<Collider2D>();
        if (!coll2d.isTrigger)
        {
            Debug.LogError(
                "Tutorial Activator (" + gameObject.name
                + ") needs its Collider2D to be a trigger! (set 'Is Trigger' to true)",
                gameObject
                );
        }
#endif
    }

    void checkTriggers(bool triggered)
    {
        bool allTriggered = requiredTriggers.All(trigger => trigger.Triggered);
        objectsToActivate.ForEach(go => go.SetActive(allTriggered));
    }
}
