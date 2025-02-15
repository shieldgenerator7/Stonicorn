using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.GraphicsBuffer;

public class SnakeController : MonoBehaviour
{
    public float moveSpeed = 1;
    public float arriveThreshold = 0.1f;
    public int maxPointCount = 4;

    [Header("Components")]
    public List<Transform> movePath;

    public Transform head;
    public Transform tail;



    [AutoInitialize, SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    [AutoInitialize(SearchChildren =true), SerializeField, HideInInspector]
    private SpriteShapeController ssc;



    private int targetIndex = 0;
    private Vector2 targetPos;
    private List<Vector2> points = new List<Vector2>();
    private bool atXPos = false;

    public int TargetIndex
    {
        get => targetIndex;
        set
        {
            targetIndex = Utility.loopValue(value,0, movePath.Count-1);
            Debug.Log($"Snake: targetIndex: {targetIndex}");
            //TODO: account for if the transform moves after setting the targetPos?
            targetPos = movePath[targetIndex].position;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TargetIndex = 0;
        //points.Add(transform.position);
        turn();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //move head
        //TODO: make this work in a round world
        if (Mathf.Abs(targetPos.x - transform.position.x) <= arriveThreshold)
        {
            Vector2 pos = transform.position;
            pos.x = targetPos.x;
            transform.position = pos;
            if (!atXPos)
            {
                atXPos = true;
                turn();
            }
        }
        if (Mathf.Abs(targetPos.y - transform.position.y) <= arriveThreshold)
        {
            transform.position = targetPos;
            TargetIndex++;
            if (atXPos)
            {
                atXPos = false;
                turn();
            }
        }
        if (Mathf.Abs(targetPos.x - transform.position.x) > arriveThreshold)
        {
            rb2d.linearVelocity = Vector2.right * Mathf.Sign(targetPos.x - transform.position.x) * moveSpeed;
            atXPos = false;
        }
        else
        {
            rb2d.linearVelocity = Vector2.up * Mathf.Sign(targetPos.y - transform.position.y) * moveSpeed;
        }
        updateBody();
    }

    void turn()
    {
        if (points.Count >= maxPointCount)
        {
            points.RemoveAt(0);
        }
        points.Add(transform.position);
    }

    void updateBody()
    {
        Spline spline = ssc.spline;
        spline.Clear();
        points.ForEach(p =>
        {
            spline.InsertPointAt(0, transform.InverseTransformPoint(p));
        });
        spline.InsertPointAt(0, transform.InverseTransformPoint(transform.position));
        head.position = transform.position;
        head.right = rb2d.linearVelocity;
        tail.position = (points.Count >= 1)?points.First():transform.position;
        if (points.Count >= 2)
        {
            tail.right = points[1] - points[0];
        }
        else
        {
            tail.right = rb2d.linearVelocity;
        }
    }
}
