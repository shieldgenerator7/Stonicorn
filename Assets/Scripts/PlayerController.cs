using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Settings
    [SerializeField] private float range = 3;
    public float Range
    {
        get { return range; }
        set { setRange(value); }
    }
    public float baseRange = 3;
    public float exhaustRange = 1;
    public int maxAirPorts = 0;
    public float exhaustCoolDownTime = 0.5f;//the cool down time for teleporting while exhausted in seconds
    [Range(0, 1)]
    public float gravityImmuneTimeAmount = 0.2f;//amount of time Merky is immune to gravity after landing (in seconds)
    public float autoTeleportDelay = 0.1f;//how long (sec) between each auto teleport using the hold gesture

    //Processing
    public float teleportTime = 0f;//the earliest time that Merky can teleport
    private float gravityImmuneTime = 0f;//Merky is immune to gravity until this time
    private float lastAutoTeleportTime = 0f;//the last time that Merky auto teleported using the hold gesture


    public GameObject teleportRangeParticalObject;
    private ParticleSystemController teleportRangeParticalController;
    public GameObject wallJumpAbilityIndicator;

    public int airPorts = 0;
    private bool grounded = true;//set in isGrounded()
    private bool groundedAbility = false;//true if grounded exclusively to a wall; set in isGrounded()
    public bool Grounded
    {
        get { return grounded || groundedAbility; }
    }
    private bool groundedPreTeleport = true;//true if Merky was grounded before teleporting
    private bool groundedAbilityPreTeleport = false;//true if grounded exclusively to a wall; set in isGrounded()
    public bool GroundedPreTeleport
    {
        get { return groundedPreTeleport || groundedAbilityPreTeleport; }
    }
    public bool GroundedPreTeleportAbility
    {
        get { return groundedAbilityPreTeleport; }
    }
    private bool shouldGrantGIT = false;//whether or not to grant gravity immunity, true after teleport
    private Rigidbody2D rb2d;
    private PolygonCollider2D pc2d;
    private GravityAccepter gravity;
    private Vector2 savedVelocity;
    private float savedAngularVelocity;
    private bool velocityNeedsReloaded = false;//because you can't set a Vector2 to null, using this to see when the velocity needs reloaded
    private float halfWidth = 0;//half of Merky's sprite width

    private bool inCheckPoint = false;//whether or not the player is inside a checkpoint
    private float[] rotations = new float[] { 285, 155, 90, 0 };
    private RaycastHit2D[] rch2dsGrounded = new RaycastHit2D[100];//used for determining if Merky is grounded

    public AudioClip teleportSound;
    public BoxCollider2D scoutCollider;//collider used to scout the level for teleportable spots

    private CameraController mainCamCtr;//the camera controller for the main camera
    public CameraController Cam
    {
        get { return mainCamCtr; }
        private set { mainCamCtr = value; }
    }
    private GestureManager gm;
    private HardMaterial hm;

    private TeleportAbility tpa;
    private ForceTeleportAbility fta;
    private WallClimbAbility wca;
    private ShieldBubbleAbility sba;

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        pc2d = GetComponent<PolygonCollider2D>();
        gravity = GetComponent<GravityAccepter>();
        mainCamCtr = Camera.main.GetComponent<CameraController>();
        gm = GameObject.FindGameObjectWithTag("GestureManager").GetComponent<GestureManager>();
        hm = GetComponent<HardMaterial>();
        hm.shattered += shattered;
        tpa = GetComponent<TeleportAbility>();
        fta = GetComponent<ForceTeleportAbility>();
        wca = GetComponent<WallClimbAbility>();
        sba = GetComponent<ShieldBubbleAbility>();
        halfWidth = GetComponent<SpriteRenderer>().bounds.extents.magnitude;
        teleportRangeParticalController = teleportRangeParticalObject.GetComponent<ParticleSystemController>();
        teleportRangeParticalController.activateTeleportParticleSystem(true, 0);
        onPreTeleport += canTeleport;
        setRange(baseRange);
    }

    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        Vector2 pos2 = new Vector2(pos.x, pos.y);
        checkGroundedState(false);
        if (shouldGrantGIT && grounded)//first grounded frame after teleport
        {
            shouldGrantGIT = false;
            grantGravityImmunity();
        }
        if (gravityImmuneTime > Time.time)
        {
        }
        else
        {
            gravity.AcceptsGravity = true;
            if (velocityNeedsReloaded)
            {
                rb2d.velocity = savedVelocity;
                rb2d.angularVelocity = savedAngularVelocity;
                velocityNeedsReloaded = false;
            }
        }
    }

    void grantGravityImmunity()
    {
        gravityImmuneTime = Time.time + gravityImmuneTimeAmount;
        savedVelocity = rb2d.velocity;
        savedAngularVelocity = rb2d.angularVelocity;
        gravity.AcceptsGravity = false;
        velocityNeedsReloaded = true;
        rb2d.velocity = new Vector3(0, 0);
        rb2d.angularVelocity = 0f;
    }

    /// <summary>
    /// Whether or not Merky is moving
    /// Does not consider rotation
    /// </summary>
    /// <returns></returns>
    bool isMoving()
    {
        return rb2d.velocity.magnitude >= 0.1f;
    }

    /// <summary>
    /// Rotates Merky to the next default rotation clockwise
    /// </summary>
    void rotate()
    {
        float newAngle = getNextRotation(transform.localEulerAngles.z);
        transform.localEulerAngles = new Vector3(0, 0, newAngle);
    }
    public float getNextRotation(float angleZ)
    {
        int closestRotationIndex = 0;
        //Figure out current rotation
        float gravityRot = Utility.RotationZ(gravity.Gravity, Vector3.down);
        float currentRotation = angleZ - gravityRot;
        currentRotation = Utility.loopValue(currentRotation, 0, 360);
        //Figure out which default rotation is closest
        float closest = 360;
        for (int i = 0; i < rotations.Length; i++)
        {
            float rotation = rotations[i];
            float diff = Mathf.Abs(rotation - currentRotation);
            diff = Mathf.Min(diff, Mathf.Abs(rotation - (currentRotation - 360)));
            if (diff < closest)
            {
                closest = diff;
                closestRotationIndex = i;
            }
        }
        int newRotationIndex = closestRotationIndex + 1;
        if (newRotationIndex >= rotations.Length)
        {
            newRotationIndex = 0;
        }
        //Set rotation
        float angle = rotations[newRotationIndex] + gravityRot;
        angle = Utility.loopValue(angle, 0, 360);
        return angle;
    }
    /// <summary>
    /// Checks to make sure teleport is not on cooldown
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPOs"></param>
    /// <param name="triedPos"></param>
    /// <returns></returns>
    private bool canTeleport(Vector2 oldPos, Vector2 newPos, Vector2 triedPos)
    {
        if (!isGrounded())
        {
            if (Time.time >= teleportTime)
            {
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Teleports, without any checking
    /// </summary>
    /// <param name="targetPos">Place to teleport to in world coordinations</param>
    /// <param name="playSound">Whether or not to play a sound</param>
    private void teleport(Vector3 targetPos, bool playSound = true)//
    {
        //Update mid-air cooldowns
        if (!isGrounded())
        {
            airPorts++;
        }
        if (airPorts > maxAirPorts)
        {
            //2017-03-06: copied from https://docs.unity3d.com/Manual/AmountVectorMagnitudeInAnotherDirection.html
            float upAmount = Vector3.Dot((targetPos - transform.position).normalized, -gravity.Gravity.normalized);
            teleportTime = Time.time + exhaustCoolDownTime * upAmount;
        }

        //Get new position
        Vector3 newPos = targetPos;
        //Actually Teleport
        Vector3 oldPos = transform.position;
        transform.position = newPos;

        //Show effect
        showTeleportEffect(oldPos, newPos);
        //Play Sound
        if (playSound)
        {
            if (groundedAbility && wca.enabled)
            {
                AudioSource.PlayClipAtPoint(wca.wallClimbSound, oldPos);
            }
            else
            {
                AudioSource.PlayClipAtPoint(teleportSound, oldPos);
            }
        }

        //Health Regen
        hm.addIntegrity(Vector2.Distance(oldPos, newPos));
        //Momentum Dampening
        if (!Mathf.Approximately(rb2d.velocity.sqrMagnitude, 0))//if Merky is moving
        {
            Vector3 direction = newPos - oldPos;
            float newX = rb2d.velocity.x;//the new x velocity
            float newY = rb2d.velocity.y;
            if (Mathf.Sign(rb2d.velocity.x) != Mathf.Sign(direction.x))
            {
                newX = rb2d.velocity.x + direction.x;
                if (Mathf.Sign(rb2d.velocity.x) != Mathf.Sign(newX))
                {//keep from exploiting boost in opposite direction
                    newX = 0;
                }
            }
            if (Mathf.Sign(rb2d.velocity.y) != Mathf.Sign(direction.y))
            {
                newY = rb2d.velocity.y + direction.y;
                if (Mathf.Sign(rb2d.velocity.y) != Mathf.Sign(newY))
                {//keep from exploiting boost in opposite direction
                    newY = 0;
                }
            }
            rb2d.velocity = new Vector2(newX, newY);
        }
        //Gravity Immunity
        velocityNeedsReloaded = false;//discards previous velocity if was in gravity immunity bubble
        gravityImmuneTime = 0f;
        shouldGrantGIT = true;
        checkGroundedState(true);//have to call it again because state has changed
        //On Teleport Effects
        if (onTeleport != null)
        {
            onTeleport(oldPos, newPos);
        }
        //Disable sticky pads stuck to Merky
        foreach (FixedJoint2D fj2d in GameObject.FindObjectsOfType<FixedJoint2D>())
        {
            if (fj2d.connectedBody == rb2d)
            {
                Destroy(fj2d);
            }
        }
    }

    /// <summary>
    /// Finds the teleportable position closest to the given targetPos
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns>targetPos if it is teleportable, else the closest teleportable position to it</returns>
    public Vector3 findTeleportablePosition(Vector3 targetPos)
    {
        //TSFS: Teleport Spot Finding System
        Vector3 newPos = targetPos;
        //Determine if new position is in range
        Vector3 oldPos = transform.position;
        if ((newPos - transform.position).sqrMagnitude <= range * range
            || (GestureManager.CHEATS_ALLOWED && gm.cheatsEnabled))//allow unlimited range while cheat is active
        {
        }
        else
        {
            newPos = ((newPos - oldPos).normalized * range) + oldPos;
        }

        //Determine if you can even teleport to the position (i.e. is it occupied or not?)
        {
            if (isOccupied(newPos))//test the current newPos first
            {
                //Try to adjust first
                Vector3 adjustedPos = adjustForOccupant(newPos);
                if (!isOccupied(adjustedPos))
                {
                    return adjustedPos;
                }
                //Search for a good landing spot
                List<Vector3> possibleOptions = new List<Vector3>();
                const int pointsToTry = 5;//try 5 points along the line
                const float difference = -1 * 1.00f / pointsToTry;//how much the previous jump was different by
                const float variance = 0.4f;//max amount to adjust angle by
                const int anglesToTry = 7;//try 7 angles off the line
                const float anglesDiff = variance * 2 / (anglesToTry - 1);
                //Vary the angle
                for (float a = -variance; a <= variance; a += anglesDiff)
                {
                    Vector3 dir = (newPos - oldPos).normalized;//the direction
                    dir = Utility.RotateZ(dir, a);
                    float oldDist = Vector3.Distance(oldPos, newPos);
                    Vector3 angledNewPos = oldPos + dir * oldDist;//ANGLED
                    //Backtrack
                    float distance = Vector3.Distance(oldPos, angledNewPos);
                    float percent = 1.00f - (difference * 2);//to start it off slightly further away
                    Vector3 norm = (angledNewPos - oldPos).normalized;
                    while (percent >= 0)
                    {
                        percent += difference;//actually subtraction in usual case, b/c "difference" is usually negative
                        Vector3 testPos = (norm * distance * percent) + oldPos;
                        if (isOccupied(testPos))
                        {
                            //adjust pos based on occupant
                            testPos = adjustForOccupant(testPos);
                            if (!isOccupied(testPos))
                            {
                                possibleOptions.Add(testPos);
                                if (percent <= 1)//make sure you at least try the standard position
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //found an open spot (tho it might not be optimal)
                            possibleOptions.Add(testPos);
                            if (percent <= 1)//make sure you at least try the standard position
                            {
                                break;
                            }
                        }
                    }
                }
                //Choose the closest option 
                float closestSqrDistance = (newPos - oldPos).sqrMagnitude;
                Vector3 closestOption = oldPos;
                foreach (Vector3 option in possibleOptions)
                {
                    float sqrDistance = (newPos - option).sqrMagnitude;
                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        closestOption = option;
                    }
                }
                return closestOption;
            }
        }
        return newPos;
    }

    void showTeleportEffect(Vector3 oldp, Vector3 newp)
    {
        EffectManager.showTeleportStar(oldp);
        //Check for wall jump
        if (wca.enabled && groundedAbility)
        {
            //Play jump effect in addition to teleport star
            wca.playWallClimbEffects(oldp);
        }
    }


    public delegate void OnTeleport(Vector2 oldPos, Vector2 newPos);
    public OnTeleport onTeleport;

    //Used when you also need to know where the user clicked
    public delegate bool OnPreTeleport(Vector2 oldPos, Vector2 newPos, Vector2 triedPos);
    public OnPreTeleport onPreTeleport;

    private void setRange(float newRange)
    {
        range = newRange;
        TeleportRangeIndicatorUpdater tri = GetComponentInChildren<TeleportRangeIndicatorUpdater>();
        if (tri != null)
        {
            tri.updateRange();
        }
        ParticleSystemController[] pscs = GetComponentsInChildren<ParticleSystemController>();
        foreach (ParticleSystemController psc in pscs)
        {
            if (psc.dependsOnTeleportRange)
            {
                psc.setOuterRange(newRange);
            }
        }
    }

    void checkGroundedState(bool exhaust)
    {
        if (isGrounded())
        {
            airPorts = 0;
            if (range < baseRange)
            {
                setRange(baseRange);
            }
        }
        else
        {
            if (exhaust && airPorts >= maxAirPorts)
            {
                if (range > exhaustRange)
                {
                    setRange(exhaustRange);
                }
            }
        }

    }

    bool isGrounded()
    {
        groundedPreTeleport = grounded;
        groundedAbilityPreTeleport = groundedAbility;
        //if (gravity.Gravity == Vector2.zero)
        //{
        //    return true;
        //}
        groundedAbility = false;
        bool isgrounded = isGrounded(gravity.Gravity);
        if (!isgrounded && isGroundedCheck != null)//if nothing found yet and there is an extra ground check to do
        {
            //Check each onPreTeleport delegate
            foreach (IsGroundedCheck igc in isGroundedCheck.GetInvocationList())
            {
                bool result = igc.Invoke();
                //If at least 1 returns true, Merky is grounded
                if (result == true)
                {
                    isgrounded = true;
                    groundedAbility = true;
                    break;
                }
            }
        }
        grounded = isgrounded;
        return isgrounded;
    }
    public bool isGrounded(Vector3 direction)
    {
        float length = 0.25f;
        int count = pc2d.Cast(direction, rch2dsGrounded, length, true);
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D rch2d = rch2dsGrounded[i];
            if (rch2d && rch2d.collider != null && !rch2d.collider.isTrigger)
            {
                GameObject ground = rch2d.collider.gameObject;
                if (ground != null && !ground.Equals(transform.gameObject))
                {
                    //Debug.Log("isGround: grounded on: " + ground.name);
                    return true;
                }
            }
        }
        return false;
    }

    public delegate bool IsGroundedCheck();
    public IsGroundedCheck isGroundedCheck;

    /// <summary>
    /// Determines whether the given position is occupied or not
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool isOccupied(Vector3 pos)
    {
        bool occupied = false;
        //Debug.DrawLine(pos, pos + new Vector3(0,0.25f), Color.green, 5);        
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        Vector3 offset = pos - transform.position;
        float angle = transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html
        //Test with scout collider
        {
            Vector3 savedOffset = scoutCollider.offset;
            scoutCollider.offset = rOffset;
            scoutCollider.Cast(Vector2.zero, rh2ds, 0, true);
            occupied = isOccupied(rh2ds, pos);
            scoutCollider.offset = savedOffset;
        }
        //Test with actual collider
        if (!occupied)
        {
            Vector3 savedOffset = pc2d.offset;
            pc2d.offset = rOffset;
            pc2d.Cast(Vector2.zero, rh2ds, 0, true);
            occupied = isOccupied(rh2ds, pos);
            pc2d.offset = savedOffset;
        }
        //Debug.DrawLine(pc2d.offset+(Vector2)transform.position, pc2d.bounds.center, Color.grey, 10);
        return occupied;
    }
    bool isOccupied(RaycastHit2D[] rch2ds, Vector3 triedPos)
    {
        foreach (RaycastHit2D rh2d in rch2ds)
        {
            if (rh2d.collider == null)
            {
                break;//reached the end of the valid RaycastHit2Ds
            }
            GameObject go = rh2d.collider.gameObject;
            if (!rh2d.collider.isTrigger)
            {
                if (go != gameObject)
                {
                    if (isOccupiedException != null)
                    {
                        //Check each isOccupiedCatch delegate
                        //2018-02-18: copied from processTapGesture(.)
                        foreach (IsOccupiedException ioc in isOccupiedException.GetInvocationList())
                        {
                            //Make it do what it needs to do, then return the result
                            bool result = ioc.Invoke(rh2d.collider, triedPos);
                            //If at least 1 returns true, it's considered not occupied
                            if (result == true)
                            {
                                return false;//return false for "not occupied"
                            }
                        }
                    }
                    //Debug.Log("Occupying object: " + go.name);
                    return true;
                }

            }
            if (go.tag == "NonTeleportableArea" || (go.transform.parent != null && go.transform.parent.gameObject.tag == "NonTeleportableArea"))
            {
                if (go.GetComponent<SecretAreaTrigger>() == null)
                {
                    return true;//yep, it's occupied by a hidden area
                }
            }
        }
        return false;
    }

    public delegate bool IsOccupiedException(Collider2D coll, Vector3 testPos);
    public IsOccupiedException isOccupiedException;

    /// <summary>
    /// Adjusts the given Vector3 to avoid collision with the objects that it collides with
    /// </summary>
    /// <param name="pos">The Vector3 to adjust</param>
    /// <returns>The Vector3, adjusted to avoid collision with objects it collides with</returns>
    public Vector3 adjustForOccupant(Vector3 pos)
    {
        Vector3 moveDir = new Vector3(0, 0, 0);//the direction to move the pos so that it is valid
        Vector3 savedOffset = pc2d.offset;
        Vector3 offset = pos - transform.position;
        float angle = transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html
        pc2d.offset = rOffset;
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        int count = pc2d.Cast(Vector2.zero, rh2ds, 0, true);
        pc2d.offset = savedOffset;
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D rh2d = rh2ds[i];
            GameObject go = rh2d.collider.gameObject;
            if (!rh2d.collider.isTrigger)
            {
                if (go.CompareTag("Checkpoint_Root"))
                {
                    if (rh2d.collider.OverlapPoint(pos))
                    {
                        return go.transform.position;
                    }
                }
                if (!go.Equals(transform.gameObject))
                {
                    Vector3 closPos = rh2d.point;
                    Vector3 dir = pos - closPos;
                    Vector3 size = pc2d.bounds.extents;
                    float d2 = (size.magnitude) - Vector3.Distance(pos, closPos);
                    moveDir += dir.normalized * d2;
                }
            }
            //if (go.tag.Equals("HidableArea") || (go.transform.parent != null && go.transform.parent.gameObject.tag.Equals("HideableArea")))
            //{
            //    return true;//yep, it's occupied by a hidden area
            //}
        }
        return pos + moveDir;//not adjusted because there's nothing to adjust for
    }

    /// <summary>
    /// Delegate method called when hm's integrity reaches 0
    /// </summary>
    private void shattered()
    {
        gm.switchGestureProfile("Rewind");
        GameManager.playerShattered();
        GameStatistics.incrementCounter("deathCount");
        if (GameStatistics.counter("deathCount") == 1)
        {
            Vector2 lsrgp = GameManager.getLatestSafeRewindGhostPosition();
            transform.position = ((Vector2)transform.position + lsrgp) / 2;
            EffectManager.highlightTapArea(lsrgp);
        }
    }
    public bool isIntact()
    {
        return hm.isIntact();
    }

    public void setIsInCheckPoint(bool iicp)
    {
        inCheckPoint = iicp;
    }
    public bool getIsInCheckPoint()
    {
        return inCheckPoint;
    }

    /// <summary>
    /// Returns true if the given Vector3 is on Merky's sprite
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public bool gestureOnPlayer(Vector3 pos)
    {
        return (pos - transform.position).sqrMagnitude < halfWidth * halfWidth;
    }

    public void processTapGesture(Vector3 gpos)
    {
        if (gestureOnPlayer(gpos))
        {
            //Rotate player 90 degrees
            rotate();
        }
        Vector3 prevPos = transform.position;
        Vector3 newPos = findTeleportablePosition(gpos);
        bool continueTeleport = true;
        if (onPreTeleport != null)
        {
            //Check each onPreTeleport delegate
            foreach (OnPreTeleport opt in onPreTeleport.GetInvocationList())
            {
                //Make it do what it needs to do, then return the result
                bool result = opt.Invoke(prevPos, newPos, gpos);
                //If at least 1 returns false, don't teleport
                if (result == false)
                {
                    continueTeleport = false;
                    break;
                }
            }
        }
        if (continueTeleport)
        {
            teleport(newPos);
            GameManager.Save();
        }
    }
    public void processTapGesture(GameObject checkPoint)
    {
        CheckPointChecker cpc = checkPoint.GetComponent<CheckPointChecker>();
        if (cpc != null)
        {
            Vector2 prevPos = transform.position;
            Vector3 newPos = checkPoint.transform.position;
            if (onPreTeleport != null)
            {
                //Pass in newPos for both here because player teleported exactly where they intended to
                onPreTeleport(prevPos, newPos, newPos);
            }
            teleport(newPos);
            mainCamCtr.recenter();
            cpc.trigger();
            GameManager.Save();
        }
    }


    public void processHoldGesture(Vector3 gpos, float holdTime, bool finished)
    {
        Debug.DrawLine(transform.position, transform.position + new Vector3(0, halfWidth, 0), Color.blue, 10);
        float reducedHoldTime = holdTime - gm.getHoldThreshold();
        if (!mainCamCtr.offsetOffPlayer())
        {
            if (Time.time > lastAutoTeleportTime + autoTeleportDelay)
            {
                lastAutoTeleportTime = Time.time;
                processTapGesture(gpos);
            }
        }
        else
        {
            if (fta.enabled) { fta.dropHoldGesture(); }
            if (sba.enabled) { sba.dropHoldGesture(); }
            tpa.processHoldGesture(gpos, reducedHoldTime, finished);
            if (finished)
            {
                processTapGesture(gpos);
            }
        }
    }
    public void dropHoldGesture()
    {
        tpa.dropHoldGesture();
        if (fta.enabled) { fta.dropHoldGesture(); }
        if (sba.enabled) { sba.dropHoldGesture(); }
    }
}


