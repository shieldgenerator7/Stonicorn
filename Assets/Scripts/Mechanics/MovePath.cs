using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[NonSolid, RequireComponent(typeof(EdgeCollider2D))]
public class MovePath : MonoBehaviour
{
    [AutoInitialize, SerializeField, HideInInspector]
    private EdgeCollider2D ec2d;

    public List<Vector2> Points
        => ec2d.points.ToList()
        .ConvertAll(p => (Vector2)transform.TransformPoint(p));

    public int Count => ec2d.pointCount;

    public Vector2 this[int key]
    {
        get => transform.TransformPoint(ec2d.points[key]);
    }
    public int IndexOf(Vector2 point) => ec2d.points.ToList().IndexOf(point);
}
