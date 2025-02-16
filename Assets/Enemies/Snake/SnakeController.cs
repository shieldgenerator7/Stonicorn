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

    [Header("Components")]
    public List<Transform> movePath;

    public Transform head;
    public Transform tail;



    [AutoInitialize, SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    [AutoInitialize(SearchChildren =true), SerializeField, HideInInspector]
    private SpriteShapeController ssc;
    [AutoInitialize(SearchChildren = true), SerializeField, HideInInspector]
    private EdgeCollider2D ec2d;




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
            //TODO: account for if the transform moves after setting the targetPos?
            targetPos = movePath[targetIndex].position;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        init();
    }

    public override void init()
    {
        TargetIndex = 0;
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
        //body
        Spline spline = ssc.spline;
        spline.Clear();
        List<Vector2> ec2dpoints = new List<Vector2>();
        points.ForEach(p =>
        {
            Vector2 p1 = transform.InverseTransformPoint(p);
            spline.InsertPointAt(0, p1);
            ec2dpoints.Insert(0, p1);
        });
        Vector2 p = transform.InverseTransformPoint(transform.position);
        spline.InsertPointAt(0, p);
        ec2dpoints.Insert(0, p);
        ec2d.points = ec2dpoints.ToArray();

        //head
        head.position = transform.position;
        head.right = rb2d.linearVelocity;

        //tail
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

    public override SavableObject CurrentState { 
        get => new SavableObject(this,
            "targetPos", targetPos
            )
            .addList<Vector2>("points",points);
        set {
            targetPos = value.Vector2("targetPos");
            targetIndex = movePath.IndexOf(movePath.FirstOrDefault(t=>(Vector2)t.position==targetPos));
            if (targetIndex < 0)
            {
                targetIndex = 0;
            }
            points = value.List<Vector2>("points");
            updateBody();
        }
    }
}
