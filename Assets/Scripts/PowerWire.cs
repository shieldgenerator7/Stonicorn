using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerWire : PowerConduit, ICuttable
{
    bool ICuttable.Cuttable => true;

    void ICuttable.cut(Vector2 start, Vector2 end)
    {
        Debug.Log("PowerWire " + name + " cut! " + start + ", " + end);
    }
}
