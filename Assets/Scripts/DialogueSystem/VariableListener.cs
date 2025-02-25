using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class VariableListener : MonoBehaviour
{
    public List<GameObject> objectsToEnable;
    public string variableToListenFor;
    public int minValue;

    void OnEnable()
    {
        Managers.Progress.onVariableChange -= listenForVariableChange;
        Managers.Progress.onVariableChange += listenForVariableChange;

        listenForVariableChange(variableToListenFor, 0, Managers.Progress.get(variableToListenFor));
    }
    private void OnDisable()
    {
        Managers.Progress.onVariableChange -= listenForVariableChange;
    }

    private void listenForVariableChange(string varName, int oldValue, int newValue)
    {
        if (varName == variableToListenFor)
        {
            bool enable = newValue >= minValue;
            objectsToEnable.ForEach(go => go.SetActive(enable));
        }
    }
}
