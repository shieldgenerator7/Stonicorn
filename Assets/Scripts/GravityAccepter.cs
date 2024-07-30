using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityAccepter : SavableMonoBehaviour
{
    //used for objects that need to know their gravity direction

    [Header("Settings")]
    public bool usesSideVector = false;//whether or not this use case needs to use the side vector

    [SerializeField]
    private bool saveValues = true;

    public float gravityScale = 1;
    private Vector2 gravityVector;
    public Vector2 Gravity
    {
        get
        {
            if (gravityVector == Vector2.zero)
            {
                return prevGravityVector;
            }
            return gravityVector;
        }
        private set
        {
            if (value == prevGravityVector)
            {
                gravityVector = prevGravityVector;
                if (usesSideVector)
                {
                    sideVector = prevSideVector;
                }
            }
            // 2017-06-02: moved here from PlayerController.setGravityVector(.)
            else if (value.x != gravityVector.x || value.y != gravityVector.y)
            {
                gravityVector = value;
                if (usesSideVector)
                {
                    //v = P2 - P1    //2016-01-10: copied from an answer by cjdev: http://answers.unity3d.com/questions/564166/how-to-find-perpendicular-line-in-2d.html
                    //P3 = (-v.y, v.x) / Sqrt(v.x ^ 2 + v.y ^ 2) * h
                    sideVector = new Vector3(-gravityVector.y, gravityVector.x) / Mathf.Sqrt(gravityVector.x * gravityVector.x + gravityVector.y * gravityVector.y);
                }
            }
        }
    }
    private Vector2 sideVector;
    public Vector2 SideVector
    {
        get
        {
            if (sideVector == Vector2.zero)
            {
                return prevSideVector;
            }
            return sideVector;
        }
        private set { sideVector = value; }
    }

    private Transform center;
    public Transform Center
    {
        get => center;
        set => center = value;
    }

    [SerializeField]
    private bool acceptsGravity = true;
    public bool AcceptsGravity
    {
        get { return acceptsGravity; }
        set { acceptsGravity = value; }
    }

    private Rigidbody2D rb2d;
    public Rigidbody2D Rigidbody2D
    {
        get
        {
            if (!rb2d)
            {
                rb2d = GetComponent<Rigidbody2D>();
            }
            return rb2d;
        }
    }

    public void addGravity(Vector2 newGravity)
    {
        Gravity = gravityVector + newGravity;
    }
    Vector2 prevGravityVector;
    Vector2 prevSideVector;

    public override void init()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }
    private void LateUpdate()
    {
        if (prevGravityVector != gravityVector
            && gravityVector != Vector2.zero)
        {
            onGravityChanged?.Invoke(gravityVector);
            prevGravityVector = gravityVector;
        }
        gravityVector = Vector2.zero;
        if (usesSideVector)
        {
            prevSideVector = sideVector;
            sideVector = Vector2.zero;
        }
    }

    public delegate void OnGravityChanged(Vector2 newGravity);
    public event OnGravityChanged onGravityChanged;

    public override SavableObject CurrentState
    {
        get
        {
            if (saveValues)
            {
                return new SavableObject(this,
                    "acceptsGravity", AcceptsGravity,
                    "gravityScale", gravityScale
                    );
            }
            else
            {
                return new SavableObject(this);
            }
        }
        set
        {
            if (saveValues)
            {
                AcceptsGravity = value.Bool("acceptsGravity");
                gravityScale = value.Float("gravityScale");
            }
        }
    }
}
