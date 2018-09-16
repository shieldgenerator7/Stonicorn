using UnityEngine;
using System.Collections;

public class BoundsChecker : MonoBehaviour {

    public Vector3 resetPoint = Vector3.zero;

    //// Use this for initialization
    void Start()
    {
        if (resetPoint == Vector3.zero)
        {
            resetPoint = Utility.getCollectiveColliderCenter(gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (enabled)
        {
            coll.gameObject.transform.position = new Vector3(resetPoint.x, resetPoint.y, coll.gameObject.transform.position.z);
        }
    }
}
