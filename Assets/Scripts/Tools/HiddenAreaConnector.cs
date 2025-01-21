using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class HiddenAreaConnector : MonoBehaviour
{
    public HiddenArea hiddenArea;
    public LanternActivator lanternActivator;

    public void connect()
    {
        if (!lanternActivator)
        {
            throw new System.NullReferenceException(
                "HiddenAreaConnector needs a LanternActivator selected! "
                +"Try selecting one again."
                );
        }
        if (!hiddenArea)
        {
            throw new System.NullReferenceException(
                "HiddenAreaConnector needs a HiddenArea selected! "
                + "Try selecting one again."
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
        hiddenArea.GetComponentsInChildren<Collider2D>()
            .Where(coll => coll.isTrigger)
            .Where(coll => !coll.gameObject.CompareTag("NonTeleportableArea"))
            .Where(coll => coll.OverlapPoint(lanternActivator.transform.position)).ToList()
            .ForEach(coll => coll.enabled = false);
        Debug.Log(
            "Connected! Lantern " + lanternActivator.name
            + " and HiddenArea " + hiddenArea.name
            );
    }
}
#endif
