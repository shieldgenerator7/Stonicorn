using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.GraphicsBuffer;

public class SnakeController : SavableMonoBehaviour
{
    public float moveSpeed = 1;
    public float arriveThreshold = 0.1f;
    public int maxPointCount = 4;
    public float length = 10;

    [Header("Components")]
    //TODO: make tag to say this is OK to override
    //TODO: move this to scriptable object?
    public List<Transform> movePath;

    public Transform head;
    public Transform tail;



    [AutoInitialize, SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    [AutoInitialize(SearchChildren = true), SerializeField, HideInInspector]
    private SpriteShapeController ssc;
    [AutoInitialize(SearchChildren = true), SerializeField, HideInInspector]
    private EdgeCollider2D ec2d;




    private int targetIndex = 0;
    private Vector2 targetPos;
    private List<Vector2> points = new List<Vector2>();//world space coordinates of bending places of snake

    private bool atXPos = false;

    public int TargetIndex
    {
        get => targetIndex;
        set
        {
            targetIndex = Utility.loopValue(value, 0, movePath.Count - 1);
            //TODO: account for if the transform moves after setting the targetPos?
            targetPos = movePath[targetIndex].position;
        }
    }

    public Vector2 HeadPos => transform.position;
    //TODO: consolidate calls to TailPos (max once per frame)
    public Vector2 TailPos
    {
        get
        {
            float lenSoFar = 0;
            lenSoFar += Vector2.Distance(HeadPos, points.Last());
            //if long enough with only first point
            if (lenSoFar >= length)
            {
                return (points.Last() - HeadPos).normalized * length + HeadPos;
            }
            //if long enough with several points
            for (int i = points.Count - 1; i >= 1; i--)
            {
                float prev = lenSoFar;
                lenSoFar += Vector2.Distance(points[i - 1], points[i]);
                if (lenSoFar >= length)
                {
                    return (points[i - 1] - points[i]).normalized * (length - prev) + points[i];
                }
            }
            //not long enough even with all points
            return points[0];
        }
    }
    public List<Vector2> Points
    {
        get
        {
            float lenSoFar = 0;
            List<Vector2> plist = new List<Vector2>();
            plist.Insert(0, HeadPos);
            lenSoFar += Vector2.Distance(HeadPos, points.Last());
            plist.Insert(0, points.Last());
            //if long enough with only first point
            if (lenSoFar >= length)
            {
                plist.Insert(0, (points.Last() - HeadPos).normalized * length + HeadPos);
                return plist;
            }
            //if long enough with several points
            for (int i = points.Count - 1; i >= 1; i--)
            {
                float prev = lenSoFar;
                lenSoFar += Vector2.Distance(points[i - 1], points[i]);
                if (lenSoFar >= length)
                {
                    plist.Insert(0, TailPos);
                    return plist;
                }
                else
                {
                    plist.Insert(0, points[i - 1]);
                }
            }
            //not long enough even with all points
            return plist;
        }
    }

    public override void init()
    {
        TargetIndex = 0;
        points.Clear();
        for (int i = 0; i < ssc.spline.GetPointCount(); i++)
        {
            points.Add(transform.TransformPoint(ssc.spline.GetPosition(i)));
        }
        updateBody();
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
        //body
        Spline spline = ssc.spline;
        spline.Clear();
        List<Vector2> ec2dpoints = new List<Vector2>();
        int i = 0;
        List<Vector2> plist = Points;
        plist.ForEach(p =>
        {
            Vector2 p1 = transform.InverseTransformPoint(p);
            spline.InsertPointAt(i, p1);
            ec2dpoints.Insert(i, p1);
            i++;
        });
        ec2d.points = ec2dpoints.ToArray();

        //head
        head.position = HeadPos;
        head.right = rb2d.linearVelocity;

        //tail
        tail.position = TailPos;
        if (plist.Count >= 2)
        {
            tail.right = plist[1] - plist[0];
        }
        else
        {
            tail.right = rb2d.linearVelocity;
        }
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "targetPos", targetPos
            )
            .addList<Vector2>("points", points);
        set
        {
            targetPos = value.Vector2("targetPos");
            targetIndex = movePath.IndexOf(movePath.FirstOrDefault(t => (Vector2)t.position == targetPos));
            if (targetIndex < 0)
            {
                targetIndex = 0;
            }
            points = value.List<Vector2>("points");
            updateBody();
        }
    }
}
