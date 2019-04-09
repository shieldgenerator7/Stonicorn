using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerInput
{
    private InputData inputDataVar = new InputData();
    protected InputData inputData
    {
        get
        {
            if (inputDataVar == null)
            {
                inputDataVar = new InputData();
            }
            return inputDataVar;
        }
    }

    public abstract InputData getInput();
}
