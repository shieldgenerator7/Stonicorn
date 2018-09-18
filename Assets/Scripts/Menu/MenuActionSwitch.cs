using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuActionSwitch : MonoBehaviour
{

    public abstract void doAction(bool active);

    public abstract bool getActiveState();

}
