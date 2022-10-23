using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class VariableSetAction : MonoBehaviour
{
    public List<Action> actionList;

    public void processAllActions()
    {
        actionList.ForEach(action => Managers.Dialogue.takeAction(action));
    }
}
