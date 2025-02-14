using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HiddenArea : MemoryMonoBehaviour, ISetupable
{

    //The class is just here so that the Hidden Areas themselves
    //remember whether they've been found or not,
    //and not their triggers

    //2016-11-26: called when this HiddenArea has just been discovered now
    protected override void nowDiscovered()
    {
        Fader fader = gameObject.AddComponent<Fader>();
        fader.ignorePause = true;
    }

    //2016-11-26: called when this HiddenArea had been discovered in a previous session
    protected override void previouslyDiscovered()
    {
        Destroy(gameObject);
    }


    public int setup()
    {
        int changedCount = 0;
        string TAG = "NonTeleportableArea";
        string UNTAG = "Untagged";
        HiddenArea ha = this;
        //TODO: setup this in HiddenArea.setup()
        Utility.doForGameObjectAndChildren(
            ha.gameObject,
            (go) =>
            {
                //If it doesn't have the correct tag,
                if (!go.CompareTag(TAG))
                {
                    //And it has a renderer,
                    if (go.GetComponent<Renderer>())
                    {
                        //Then it should have the correct tag
                        go.tag = TAG;
                        //EditorUtility.SetDirty(go);
                        Debug.LogWarning(
                            $"Changed {go.name} tag to {TAG}.",
                            go
                            );
                        changedCount++;
                    }
                }
                //If it does have the tag,
                else
                {
                    //But does not have a renderer,
                    if (!go.GetComponent<Renderer>())
                    {
                        //Then it should not have the tag
                        go.tag = UNTAG;
                        //EditorUtility.SetDirty(go);
                        Debug.LogWarning(
                            $"Changed {go.name} tag to {UNTAG}.",
                            go
                            );
                        changedCount++;
                    }
                }
                //Position
                if (go.transform.position.z != 0)
                {
                    go.transform.position = (Vector2)go.transform.position;
                    //EditorUtility.SetDirty(go);
                    Debug.LogWarning(
                        $"Changed {go.name} pos to {go.transform.position}.",
                        go
                        );
                    changedCount++;
                }
                //
                //Renderer && Collider
                //
                Renderer renderer = go.GetComponent<Renderer>();
                Collider2D coll2d = go.GetComponent<Collider2D>();
                string layerName = "Foreground";
                if (renderer)
                {
                    if (renderer.sortingLayerName != layerName)
                    {
                        renderer.sortingLayerName = layerName;
                        //EditorUtility.SetDirty(go);
                        Debug.LogWarning(
                            $"Changed {go.name} layer name to {layerName}.",
                            go
                            );
                        changedCount++;
                    }
                    if (!coll2d)
                    {
                        Debug.LogError(
                            $"{go.name} has renderer without a collider!",
                            go
                            );
                        //Fake a change
                        if (changedCount == 0)
                        {
                            changedCount++;
                        }
                    }
                }
                if (coll2d)
                {
                    if (!coll2d.isTrigger)
                    {
                        coll2d.isTrigger = true;
                        //EditorUtility.SetDirty(go);
                        Debug.LogWarning(
                            $"Changed {go.name} collider isTrigger to {coll2d.isTrigger}.",
                            go
                            );
                        changedCount++;
                    }
                }
            }
            );

        //if (changedCount > 0)
        //{
        //    Debug.LogWarning(
        //        $"HiddenArea changes: Made {changedCount} changes."
        //        );
        //}
        return changedCount;
    }
}
