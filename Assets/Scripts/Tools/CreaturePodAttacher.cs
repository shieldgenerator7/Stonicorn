using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using static Utility;

public class CreaturePodAttacher : MonoBehaviour
{

    public GameObject prefab;

    public GameObject vinePrefab;
    public Transform vineFolder;

    public Transform anchorOffset;

    public List<GameObject> objectsToConvert;

    [SerializeField]
    private List<GameObject> createdObjects;

    private Vector2 anchorPos;

    public void convertAll()
    {
        anchorPos=anchorOffset.localPosition;

        createdObjects.Clear();
        objectsToConvert.ForEach(obj => convert(obj));
        objectsToConvert.Clear();
        objectsToConvert.AddRange(createdObjects);
    }

    private void convert(GameObject go)
    {
#if UNITY_EDITOR
        Transform folder = go.transform.parent;
        GameObject newgo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, folder);
        newgo.transform.position = go.transform.position;
        newgo.transform.localScale = go.transform.localScale;
        newgo.transform.rotation = go.transform.rotation;
        newgo.name = go.name;
        createdObjects.Add(newgo);

        GameObject.DestroyImmediate(go);

        EditorUtility.SetDirty(newgo);

        //vine pre-check
        if (!vinePrefab)
        {
            Debug.LogWarning("No vine prefab, so not adding vine.");
            return;
        }
        Transform _vineFolder = vineFolder ?? folder;

        //vine
        GravityZone gravityZone = GravityZone.getGravityZone(newgo.transform.position);
        Vector2 upDir = newgo.transform.position - gravityZone.transform.position;
        RaycastAnswer rca = RaycastAll(newgo.transform.position, upDir, 100);
        Vector2 contactPoint = newgo.transform.position;
        for (int i = 0; i < rca.count; i++)
        {
            RaycastHit2D rch2d = rca.rch2ds[i];
            if (rch2d.collider.gameObject != newgo && !rch2d.rigidbody)
            {
                contactPoint = rch2d.point;
                break;
            }
        }
        GameObject vine = (GameObject)PrefabUtility.InstantiatePrefab(vinePrefab, _vineFolder);
        vine.transform.position = contactPoint;
        vine.transform.up = upDir;
        SpriteShapeController ssc = vine.GetComponent<SpriteShapeController>();
        Vector2 endpoint = new Vector2(
            0,
            -Vector2.Distance(
                contactPoint,
                newgo.transform.position + newgo.transform.TransformDirection(anchorPos)
            )
        );
        if (ssc)
        {
            Spline spline = ssc.spline;
            spline.Clear();
            spline.setPoints(new List<Vector2>()
            {
                new Vector2(0, 0),
                endpoint,
            });
        }

        //hookup
        Rigidbody2D vineRB2D = vine.GetComponent<Rigidbody2D>();
        newgo.GetComponents<HingeJoint2D>().ToList().ForEach(joint =>
        {
            joint.connectedBody = vineRB2D;
            joint.connectedAnchor = endpoint;
        });

        EditorUtility.SetDirty(newgo);
        EditorUtility.SetDirty(vine);
#endif
    }
}
