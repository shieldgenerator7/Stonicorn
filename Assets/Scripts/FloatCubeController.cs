using UnityEngine;
using System.Collections;

public class FloatCubeController : MonoBehaviour
{
    public const float MAX_FLOAT_HEIGHT = 15.0f;//the max propulsion height, only for visual communication, not an enforced cap
    public const float MIN_FLOAT_HEIGHT = 2.0f;//the min propulsion height, only for visual communication, not an enforced cap

    public GameObject switchObj;
    public GameObject psgoTrail;
    public GameObject psgoSparks;
    public float propulsionHeight;//how far off the ground the float cube can go
    public float liftForce = 0;//how much force can be applied each frame
    public float expectedGravity = 9.81f;//the expected gravity scale for this float cube
    public float forceMultiplierSelf = 1.1f;//when float cube is empty, how much to multiply the default lift force 
    public float forceMultiplierHeavy = 2.0f;//how much to multiply everything when float cube has something on it weighing it down
    public float forceMultiplierSteady = 1.0f;//how much to multiply when trying to remain steady
    public float variance;//the amount of variance in either direction the propulsionHeight is allowed to be

    private Rigidbody2D rb;
    private BoxCollider2D bc2d;
    private Vector3 upDirection;//used to determine the up direction of the float cube
    private Quaternion upAngle;//used to determine which direction the float cube should rotate towards
    //Particles
    private ParticleSystem psTrail;
    private ParticleSystem psSparks;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc2d = GetComponent<BoxCollider2D>();
        psTrail = psgoTrail.GetComponent<ParticleSystem>();
        if (psTrail != null)
        {
            psTrail.Pause();
            psTrail.Clear();
        }
        psSparks = psgoSparks.GetComponent<ParticleSystem>();
        if (psSparks != null)
        {
            psSparks.Pause();
            psSparks.Clear();
        }
        //Propulsion Height Indicator
        GameObject phi = null;
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name.Contains("indicator"))
            {
                phi = t.gameObject;
                break;
            }
        }
        float newY = Utility.convertToRange(
            Mathf.Min(propulsionHeight, MAX_FLOAT_HEIGHT),
            MIN_FLOAT_HEIGHT, MAX_FLOAT_HEIGHT,
            -0.5f, 0.5f);
        phi.transform.localPosition = new Vector2(phi.transform.localPosition.x, newY);
        //
        if (liftForce == 0)
        {
            liftForce = rb.mass * expectedGravity;
        }
        upDirection = transform.up.normalized;
        upAngle = Quaternion.Euler(transform.eulerAngles);
    }

    // Update is called once per frame
    void Update()
    {
        if (switchObj.GetComponent<WeightSwitchActivator>().pressed)
        {
            if (psTrail != null)
            {
                psTrail.transform.rotation = transform.rotation;
                psTrail.Play();
            }
            if (psSparks != null)
            {
                psSparks.Play();
            }            
        }
        else {
            if (psTrail != null)
            {
                psTrail.Pause();
                psTrail.Clear();
            }
            if (psSparks != null)
            {
                psSparks.Pause();
                psSparks.Clear();
            }
        }
    }

    void FixedUpdate()
    {
        if (switchObj.GetComponent<WeightSwitchActivator>().pressed)
        {
            if (propulsionHeight > 0)
            {
                rb.freezeRotation = true;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, upAngle, 5);
                bool propping1 = false;
                bool propping2 = false;

                Vector2 start = getGroundVector(propulsionHeight);
                Debug.DrawLine(start, transform.position, Color.black);
                RaycastHit2D[] rch2ds = new RaycastHit2D[10];
                bc2d.Cast(-transform.up, rch2ds, propulsionHeight, true);
                foreach (RaycastHit2D rch2d in rch2ds)
                {
                    if (rch2d && rch2d.collider != null && !rch2d.collider.isTrigger)
                    {
                        GameObject ground = rch2d.collider.gameObject;
                        if (ground != null && !ground.Equals(transform.gameObject))
                        {
                            propping1 = true;
                        }
                    }
                }
                start = getGroundVector((propulsionHeight + variance));
                bc2d.Cast(-transform.up, rch2ds, propulsionHeight + variance, true);
                foreach (RaycastHit2D rch2d in rch2ds)
                {
                    if (rch2d && rch2d.collider != null && !rch2d.collider.isTrigger)
                    {
                        GameObject ground = rch2d.collider.gameObject;
                        if (ground != null && !ground.Equals(transform.gameObject))
                        {
                            propping2 = true;
                        }
                    }
                }
                if (propping1 && propping2)
                {
                    float heavyMultiplier = 1;
                    float upVelocity = Vector3.Dot(rb.velocity, upDirection.normalized);
                    if (upVelocity <= 0)
                    {
                        heavyMultiplier = forceMultiplierHeavy;
                    }
                    float force = liftForce * forceMultiplierSelf * heavyMultiplier;
                    rb.AddForce(upDirection * force);
                }
                else if (!propping1 && propping2)
                {
                    float heavyMultiplier = 1;
                    float upVelocity = Vector3.Dot(rb.velocity, upDirection.normalized);
                    if (upVelocity < 0)
                    {
                        heavyMultiplier = forceMultiplierHeavy;
                    }
                    else if (upVelocity > 0)
                    {
                        heavyMultiplier = 0;
                    }
                    rb.AddForce(upDirection * liftForce * forceMultiplierSteady * heavyMultiplier);
                }
                else
                {
                    Debug.DrawLine(start, transform.position, Color.red);
                }
            }
        }
        else
        {
            rb.freezeRotation = false;
        }
    }

    /// <summary>
    /// Returns a vector with magnitude propulsionHeight and angle this.upDirection
    /// </summary>
    /// <param name="propulsionHeight"></param>
    /// <returns></returns>
    Vector3 getGroundVector(float propulsionHeight)
    {
        return transform.position - upDirection.normalized * propulsionHeight;
    }
}
