using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartCheckVariable : MonoBehaviour
{
    [Tooltip("Action will be taken if this variable is at or above the value.")]
    public string requiredVariable;
    [Tooltip("Action will be taken if the variable is at or above this value.")]
    public int requiredValue;
    public enum ActionType
    {
        ENABLE,
        DISABLE
    }
    public ActionType actionToTake;

    /// <summary>
    /// Called from DialogueManager when a scene loads
    /// </summary>
    public void checkTakeAction(ProgressManager progressManager)
    {
        int value = progressManager.get(requiredVariable);
        if (value >= requiredValue)
        {
            takeAction();
        }
        Destroy(this);
    }

    void takeAction()
    {
        switch (actionToTake)
        {
            case ActionType.ENABLE: gameObject.SetActive(true);break;
            case ActionType.DISABLE: gameObject.SetActive(false); break;
            default: throw new ArgumentException("Action type not supported: " + actionToTake);
        }
    }
}
