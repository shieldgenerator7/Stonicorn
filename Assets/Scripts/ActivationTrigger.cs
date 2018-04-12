using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationTrigger : MonoBehaviour {

    public enum ActivationOptions
    {
        ACTIVATE,
        DEACTIVATE,
        SWITCH,
        DO_NOTHING
    }
    public ActivationOptions triggerEnterAction = ActivationOptions.ACTIVATE;
    public ActivationOptions triggerExitAction = ActivationOptions.DEACTIVATE;

    public List<GameObject> objectsToActivate;

    public bool forPlayerOnly = true;

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (!forPlayerOnly || coll.gameObject.CompareTag("Player"))
        {
            processObjects(triggerEnterAction);
        }
    }
    private void OnTriggerExit2D(Collider2D coll)
    {
        if (!forPlayerOnly || coll.gameObject.CompareTag("Player"))
        {
            processObjects(triggerExitAction);
        }
    }
    private void processObjects(ActivationOptions action)
    {
        foreach (GameObject go in objectsToActivate)
        {
            switch (action)
            {
                case ActivationOptions.ACTIVATE:
                    go.SetActive(true);
                    break;
                case ActivationOptions.DEACTIVATE:
                    go.SetActive(false);
                    break;
                case ActivationOptions.SWITCH:
                    go.SetActive(!go.activeSelf);
                    break;
                case ActivationOptions.DO_NOTHING:
                    break;
            }
        }
    }
}
