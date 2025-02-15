using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static UnityEngine.GraphicsBuffer;

public class SnakeController : MonoBehaviour
{
    public float moveSpeed = 1;
    public float arriveThreshold = 0.1f;

    [Header("Components")]
    public List<Transform> movePath;



    [AutoInitialize, SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    [AutoInitialize(SearchChildren =true), SerializeField, HideInInspector]
    private SpriteShapeController ssc;



    private int targetIndex = 0;
    private Vector2 targetPos;
    private List<Vector2> points;

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
        TargetIndex = 0;
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
        }
        if (Mathf.Abs(targetPos.y - transform.position.y) <= arriveThreshold)
        {
            transform.position = targetPos;
            TargetIndex++;
        }
        if (Mathf.Abs(targetPos.x - transform.position.x) > arriveThreshold)
        {
            rb2d.linearVelocity = Vector2.right * Mathf.Sign(targetPos.x - transform.position.x) * moveSpeed;           
        }
        else
        {
            rb2d.linearVelocity = Vector2.up * Mathf.Sign(targetPos.y - transform.position.y) * moveSpeed;
        }
    }
}
