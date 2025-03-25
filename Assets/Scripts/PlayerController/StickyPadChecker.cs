using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StickyPadChecker : SavableMonoBehaviour
{
    private List<string> connectedObjs = new List<string>();
    private List<int> connectedIds = new List<int>();

    [AutoInitialize, SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    // Use this for initialization
    void OnEnable()
    {
        Debug.Log($"StickyPad.OnEnable()");
        init();
    }
    public override void init()
    {
        Debug.Log($"StickyPad.init()");
    }

    public void init(Vector2 normal)
    {
        Debug.Log($"StickyPad.init({normal})");
        transform.right = -normal;
    }

    //TODO: rethink how this gets rewound
    public override SavableObject CurrentState
    {
        //TODO: rethink how this gets rewound
        get
        {
            //Debug.Log($"StickyPad.CurrentState:get()");
            SavableObject so = new SavableObject(this);
            ////connectedObjs
            //for (int i = 0; i < connectedObjs.Count; i++)
            //{
            //    so.more($"conObj{i}", connectedObjs[i]);
            //}
            //so.more("conObjCount", connectedObjs.Count);
            ////connectedIds
            //for (int i = 0; i < connectedIds.Count; i++)
            //{
            //    so.more($"conId{i}", connectedIds[i]);
            //}
            //so.more("conIdCount", connectedIds.Count);
            ////
            return so;
        }
        //TODO: rethink how this gets rewound
        set
        {
            //Debug.Log($"StickyPad.CurrentState:set({value})");

            ////connect objects
            //List<string> newConnectedObjs = new List<string>();
            //int count = value.Int("conObjCount");
            //for (int i = 0; i < count; i++)
            //{
            //    newConnectedObjs.Add(value.String($"conObj{i}"));
            //}
            ////check for joints that need deleted
            //for(int i = connectedObjs.Count - 1; i >= 0; i--)
            //{
            //    string goName = connectedObjs[i];
            //    if (!newConnectedObjs.Contains(goName))
            //    {
            //        deleteJoint(goName);
            //    }
            //}
            ////check for joints that need added
            //for (int i = 0;i < newConnectedObjs.Count; i++)
            //{
            //    string goName = newConnectedObjs[i];
            //    if (!connectedObjs.Contains(goName))
            //    {
            //        createJoint(GameObject.Find(goName));
            //    }
            //}
            //connectedObjs = newConnectedObjs;

            ////connect ids
            //List<int> newConnectedIds = new List<int>();
            //int count2 = value.Int("conIdCount");
            //for (int i = 0; i < count2; i++)
            //{
            //    newConnectedIds.Add(value.Int($"conId{i}"));
            //}
            ////check for joints that need deleted
            //for (int i = connectedIds.Count - 1; i >= 0; i--)
            //{
            //    int goKey = connectedIds[i];
            //    if (!newConnectedIds.Contains(goKey))
            //    {
            //        deleteJoint(goKey);
            //    }
            //}
            ////check for joints that need added
            //for (int i = 0; i < newConnectedIds.Count; i++)
            //{
            //    int goKey = newConnectedIds[i];
            //    if (!connectedIds.Contains(goKey))
            //    {
            //        Rigidbody2D rb2d = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None)
            //            .First(rb2d => rb2d.gameObject.getKey() == goKey);
            //        createJoint(rb2d);
            //    }
            //}
            //connectedIds = newConnectedIds;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"StickyPad.OnCollisionEnter2D({collision})");
        if (!collision.collider.isTrigger)
        {
            stickToObject(collision.gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log($"StickyPad.OnTriggerEnter2D({coll})");
        if (!coll.isTrigger)
        {
            stickToObject(coll.gameObject);
        }
    }
    void stickToObject(GameObject go)
    {
        Debug.Log($"StickyPad.stickToObject({go})");
        if (go == null)
        {
            Debug.LogError($"Trying to stick to null object! {go}",this);
            return;
        }
        Rigidbody2D goRB2D = go.GetComponent<Rigidbody2D>();
        if (goRB2D)
        {
            int goKey = go.getKey();
            if (!connectedIds.Contains(goKey))
            {
                createJoint(goRB2D);
                connectedIds.Add(goKey);
            }
        }
        else
        {
            string goName = go.name;
            if (!connectedObjs.Contains(goName))
            {
                createJoint(go);
                connectedObjs.Add(goName);
            }
        }
    }

    void createJoint(GameObject go)
    {
        TargetJoint2D tj2d = gameObject.AddComponent<TargetJoint2D>();
        tj2d.autoConfigureTarget = false;
        updateConstraints();
    }

    void deleteJoint(string name)
    {
        TargetJoint2D tj2d = GetComponents<TargetJoint2D>().FirstOrDefault(tj2d => tj2d.name == name);
        if (tj2d)
        {
            Destroy(tj2d);
            updateConstraints();
        }
    }

    void createJoint(Rigidbody2D rb2d)
    {
        FixedJoint2D fj2d = gameObject.AddComponent<FixedJoint2D>();
        fj2d.connectedBody = rb2d;
        fj2d.autoConfigureConnectedAnchor = false;
    }

    void deleteJoint(int goKey)
    {
        FixedJoint2D fj2d = GetComponents<FixedJoint2D>().FirstOrDefault(fj2d => fj2d.connectedBody.gameObject.getKey() == goKey);
        if (fj2d)
        {
            Destroy(fj2d);
        }
    }

    void updateConstraints()
    {
        TargetJoint2D tj2d = gameObject.GetComponent<TargetJoint2D>();
        rb2d.constraints = (tj2d)
            ? RigidbodyConstraints2D.FreezeAll
            : RigidbodyConstraints2D.None;
    } 
}
