using UnityEngine;
using System.Collections;

public class TeleportRangeIndicatorUpdater : MonoBehaviour
{

    public GameObject parentObj;

    private PlayerController controller;
    private float baseRange = 2.5f;//2017-01-30: got this measurement from a test run. If the sprite size ever changes, this value will also have to change
    private float baseScale = 1;
    SpriteRenderer sr;

    // Use this for initialization
    void Start()
    {
        if (parentObj != null)
        {
            controller = parentObj.GetComponent<PlayerController>();
        }
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;
    }

    public void updateRange()
    {
        //TODO: Refactor, or perhaps scrap this script?
        //if (controller.Teleport.TeleportReady)
        //{
        //    sr.enabled = true;
        //}
        //else
        //{
        //    sr.enabled = false;
        //}
        float newSize = controller.Range;
        setRange(newSize);
    }
    public void setRange(float range)
    {
        float newScale = baseScale * range / baseRange;
        Vector3 newV = new Vector3(newScale, newScale, 0);
        transform.localScale = newV;
    }
}
