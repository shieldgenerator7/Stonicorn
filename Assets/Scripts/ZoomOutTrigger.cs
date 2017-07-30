using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomOutTrigger : MemoryMonoBehaviour {

    public bool triggered = false;//whether or this has zoomed out the camera
    public int scalePoint = CameraController.SCALEPOINT_DEFAULT;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            var camCtr = Camera.main.GetComponent<CameraController>();
            camCtr.setScalePoint(scalePoint);//zoom out
            if (scalePoint == CameraController.SCALEPOINT_TIMEREWIND)
            {
                FindObjectOfType<GestureManager>().switchGestureProfile("Rewind");
            }
            GameManager.saveMemory(this);
            Destroy(gameObject);
        }
    }
    

    public override MemoryObject getMemoryObject()
    {
        return new MemoryObject(this, triggered);
    }
    public override void acceptMemoryObject(MemoryObject memObj)
    {
        if (memObj.found)
        {
            triggered = true;
            GameManager.saveMemory(this);
            Destroy(gameObject);
        }
    }
}
