using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HiddenAreaConnector : MonoBehaviour
{
    public HiddenArea hiddenArea;
    public LanternActivator lanternActivator;

    public void connect()
    {
        if (!lanternActivator)
        {
            throw new System.NullReferenceException(
                "HiddenAreaConnector needs a LanternActivator selected!"
                );
        }
        if (!hiddenArea)
        {
            throw new System.NullReferenceException(
                "HiddenAreaConnector needs a HiddenArea selected!"
                );
        }
        Undo.RecordObjects(
            new Object[] { lanternActivator, hiddenArea },
            "Connect lantern to hidden area"
            );
        EditorUtility.SetDirty(lanternActivator);
        EditorUtility.SetDirty(hiddenArea);
        //Connect lantern to hidden area
        lanternActivator.secretHider = hiddenArea;
        //Prepare hidden area for lantern
        hiddenArea.GetComponentsInChildren<Collider2D>().ToList()
            .FindAll(coll => coll.isTrigger)
            .FindAll(coll => !coll.gameObject.CompareTag("NonTeleportableArea"))
            .FindAll(coll => coll.OverlapPoint(lanternActivator.transform.position))
            .ForEach(coll => coll.enabled = false);
        Debug.Log(
            "Connected! Lantern " + lanternActivator.name
            + " and HiddenArea " + hiddenArea.name
            );
    }
}
