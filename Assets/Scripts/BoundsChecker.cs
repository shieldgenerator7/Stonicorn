using UnityEngine;
using System.Collections;

[NonSolid]
public class BoundsChecker : MonoBehaviour
{

    public Vector3 resetPoint = Vector3.zero;
    public bool loopSpace = true;
    public bool rewindTimeForPlayer = true;

    void OnTriggerExit2D(Collider2D coll)
    {
        if (enabled)
        {
            GameObject collGO = coll.gameObject;
            if (rewindTimeForPlayer && collGO.isPlayer())
            {
                Managers.Rewind.RewindToStart();
            }
            else if (loopSpace)
            {
                //If the object is moving away,
                Rigidbody2D rb2d = collGO.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    if (((Vector2)collGO.transform.position + rb2d.linearVelocity - (Vector2)transform.position).sqrMagnitude > (collGO.transform.position - transform.position).sqrMagnitude)
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

    [Initializer("resetPoint")]
    public Vector3 initResetPoint()
        => gameObject.getCollectiveColliderCenter();
}
