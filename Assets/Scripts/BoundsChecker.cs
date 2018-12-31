using UnityEngine;
using System.Collections;

public class BoundsChecker : MonoBehaviour
{

    public Vector3 resetPoint = Vector3.zero;
    public bool loopSpace = true;
    public bool rewindTimeForPlayer = true;

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
            GameObject collGO = coll.gameObject;
            if (loopSpace)
            {
                //If the object is moving away,
                Rigidbody2D rb2d = collGO.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    if (((Vector2)collGO.transform.position + rb2d.velocity - (Vector2)transform.position).sqrMagnitude > (collGO.transform.position - transform.position).sqrMagnitude)
                    {
                        //Loop it over to the other side
                        collGO.transform.position = transform.position + (transform.position - collGO.transform.position);
                    }
                }
            }
            else
            {
                collGO.transform.position = new Vector3(resetPoint.x, resetPoint.y, collGO.transform.position.z);
            }
        }
    }
}
