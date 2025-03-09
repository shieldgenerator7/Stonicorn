using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ContinuallyCheckForDialogue : MonoBehaviour
{
    [AutoInitialize]
    public List<EventTrigger> triggers;
    public List<string> variablesToListenFor;

    private void OnEnable()
    {
        Managers.Progress.onVariableChange -= checkForDialogue;
        Managers.Progress.onVariableChange += checkForDialogue;
    }

    private void OnDisable()
    {
        Managers.Progress.onVariableChange -= checkForDialogue;
    }

    private void checkForDialogue(string varName, int oldValue, int newValue)
    {
        if (variablesToListenFor.Count == 0 || variablesToListenFor.Contains(varName))
        {
            Debug.Log($"(dialogue) listened to {varName} change, processing!", this);
            triggers.ForEach(trigger => trigger.ActivateTrigger());
        }
        else
        {
            Debug.Log($"(dialogue) listened to {varName} change, ignoring...", this);
        }
    }
}
