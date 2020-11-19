using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : Hazard
{
    [Header("Settings")]
    public float moveSpeed = 1;
    [Header("Animators")]
    public Animator legAnimator;
    [Header("Components")]
    public Collider2D cliffDetector;

    private Rigidbody2D rb2d;

    //
    // Properties
    //

    public override bool Hazardous => false;

    public bool CliffDetected
        => Utility.CastCountSolid(cliffDetector, Vector2.zero) == 0;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb2d.velocity =
            transform.right
            * Mathf.Sign(transform.localScale.x)
            * moveSpeed;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CliffDetected)
        {
            Vector3 scale = transform.localScale;
            transform.localScale = scale.setX(scale.x * -1);
        }
    }
}
