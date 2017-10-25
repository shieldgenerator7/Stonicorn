using UnityEngine;

public class CheckPointActivator : MonoBehaviour
{
    //Place this on an object with a Collider
    // w/ isTrigger checked
    //and whose parent is a Checkpoint_Root

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == GameManager.playerTag)
        {
            transform.parent.GetComponent<CheckPointChecker>().activate();
        }
    }
}
