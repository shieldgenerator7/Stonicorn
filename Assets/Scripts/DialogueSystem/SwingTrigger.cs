using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WheelJoint2D))]
public class SwingTrigger : MonoBehaviour
{
    public float swingSpeed = 5;
    public float ungrabbableDuration = 0.2f;

    private Rigidbody2D rb2d;
    private WheelJoint2D joint;
    private SpriteRenderer sr;

    private float ungrabbleStartTime = -1;

    private bool _grabbable = true;
    public bool Grabbable
    {
        get => _grabbable;
        set
        {
            _grabbable = value;
            Color color = sr.color;
            color.a = (_grabbable || joint.connectedBody != null)
                ? 1
                : 0.2f;
            sr.color = color;
            ungrabbleStartTime = (_grabbable)
                ? -1
                : Time.time;
        }
    }
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        joint = GetComponent<WheelJoint2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Grabbable && joint.connectedBody == null)
        {
            if (ungrabbleStartTime > 0
            && Time.time > ungrabbleStartTime + ungrabbableDuration)
            {
                Grabbable = true;
                ungrabbleStartTime = -1;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Grabbable)
        {
            GameObject go = collision.gameObject;
            if (go.CompareTag("Player"))
            {
                connectToObject(go);
                go.GetComponent<PlayerController>().groundedCheck += groundedCheck;
                PlayerController.OnPlayerJump += disconnectPlayer;
                Grabbable = false;
            }
        }
    }


    private void connectToObject(GameObject go = null, bool connect = true)
    {
        if (connect)
        {
            Rigidbody2D rb2dGO = go.GetComponent<Rigidbody2D>();
            joint.connectedBody = rb2dGO;
            rb2d.velocity = new Vector2(
                Mathf.Sign(go.transform.localScale.x) * -1 * swingSpeed,
                0
                );
            rb2dGO.velocity = rb2d.velocity;
        }
        else
        {
            joint.connectedBody = null;
        }
    }

    private bool groundedCheck() => true;

    private void disconnectPlayer()
    {
        FindObjectOfType<PlayerController>().groundedCheck -= groundedCheck;
        PlayerController.OnPlayerJump -= disconnectPlayer;
        connectToObject(null, false);
        Grabbable = false;
        transform.eulerAngles = Vector3.zero;
        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0;
    }
}
