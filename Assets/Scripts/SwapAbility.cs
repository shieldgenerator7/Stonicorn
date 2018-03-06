using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapAbility : PlayerAbility
{
    private PolygonCollider2D pc2d;

    private List<GameObject> swappableObjects = new List<GameObject>();
    public List<GameObject> SwappableObjects
    {
        get { return swappableObjects; }
    }

    protected override void Start()
    {
        base.Start();
        playerController.isOccupiedException += isColliderSwappable;
        playerController.onPreTeleport += refreshSwappableObjectList;
        playerController.onTeleport += swapObjects;
        pc2d = GetComponent<PolygonCollider2D>();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.isOccupiedException -= isColliderSwappable;
        playerController.onPreTeleport -= refreshSwappableObjectList;
        playerController.onTeleport -= swapObjects;
    }

    bool isColliderSwappable(Collider2D coll, Vector3 testPos)
    {
        return isColliderSwappableImpl(coll, testPos, transform.position);
    }

    bool isColliderSwappableImpl(Collider2D coll, Vector3 testPos, Vector3 origPos)
    {
        Vector3 swapPos = coll.gameObject.transform.position - testPos + origPos;
        if (coll.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            return !isOccupiedForObject(coll, swapPos);
        }
        return false;
    }

    /// <summary>
    /// Determines whether the given position is occupied by an object other than Merky or not
    /// 2018-02-12: copied from PlayerController.isOccupied(.)
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool isOccupiedForObject(Collider2D coll, Vector3 pos)
    {
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        Vector3 offset = pos - coll.gameObject.transform.position;
        float angle = coll.gameObject.transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html

        //Do the test
        Vector3 savedOffset = coll.offset;
        coll.offset = rOffset;
        coll.Cast(Vector2.zero, rh2ds, 0, true);
        coll.offset = savedOffset;
        foreach (RaycastHit2D rh2d in rh2ds)
        {
            if (rh2d.collider == null)
            {
                break;//reached the end of the valid RaycastHit2Ds
            }
            GameObject go = rh2d.collider.gameObject;
            //Make sure it's not a trigger
            if (!rh2d.collider.isTrigger)
            {
                //Make sure it's not detecting itself or this gameObject
                if (go != coll.gameObject && go != gameObject)
                {
                    return true;
                }

            }
        }
        return false;
    }

    void swapObjects(Vector2 oldPos, Vector2 newPos)
    {
        bool swappedSomething = false;
        foreach (GameObject go in swappableObjects)
        {
            //Clear connections
            Rigidbody2D goRB2D = go.GetComponent<Rigidbody2D>();
            if (goRB2D)
            {
                foreach (FixedJoint2D fj2d in GameObject.FindObjectsOfType<FixedJoint2D>())
                {
                    if (fj2d.connectedBody == goRB2D)
                    {
                        Destroy(fj2d);
                    }
                }
            }
            //Swap object
            Vector2 swapPos = (Vector2)gameObject.transform.position - newPos + oldPos;
            go.transform.position = swapPos;
            swappedSomething = true;
        }
        if (swappedSomething)
        {
            if (playerController.Range < playerController.baseRange)
            {
                playerController.Range = playerController.baseRange;
            }
            playerController.airPorts = 0;
        }
    }

    private bool refreshSwappableObjectList(Vector2 oldPos, Vector2 newPos, Vector2 triesPos)
    {
        swappableObjects = getSwappableObjects(newPos, oldPos);
        return true;
    }

    List<GameObject> getSwappableObjects(Vector3 pos, Vector3 origPos)
    {
        List<GameObject> gos = new List<GameObject>();
        //2018-02-12: copied from PlayerController.isOccupied(.)       
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        Vector3 offset = pos - transform.position;
        float angle = transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html

        //Find objects that would collide with the PolygonCollider2D
        //if it were at this location.
        Vector3 savedOffset = pc2d.offset;
        pc2d.offset = rOffset;
        pc2d.Cast(Vector2.zero, rh2ds, 0, true);
        pc2d.offset = savedOffset;
        foreach (RaycastHit2D rh2d in rh2ds)
        {
            if (rh2d.collider == null)
            {
                break;//reached the end of the valid RaycastHit2Ds
            }
            GameObject go = rh2d.collider.gameObject;
            if (!rh2d.collider.isTrigger)
            {
                if (go != gameObject)
                {
                    if (isColliderSwappableImpl(rh2d.collider, pos, origPos))
                    {
                        gos.Add(go);
                    }
                }

            }
        }
        return gos;
    }
}
