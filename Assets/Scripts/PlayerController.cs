using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Settings
    public float range = 3;
    public float baseRange = 3;
    public float exhaustRange = 1;
    public int maxAirPorts = 0;
    public float exhaustCoolDownTime = 0.5f;//the cool down time for teleporting while exhausted in seconds
    [Range(0, 1)]
    public float gravityImmuneTimeAmount = 0.2f;//amount of time Merky is immune to gravity after landing (in seconds)

    //Processing
    public float teleportTime = 0f;//the earliest time that Merky can teleport
    private float gravityImmuneTime = 0f;//Merky is immune to gravity until this time
    
    
    public GameObject teleportStreak;
    public bool useStreak = false;
    public GameObject teleportStar;
    public bool useStar = true;
    public GameObject teleportRangeParticalObject;
    private ParticleSystemController teleportRangeParticalController;
    public GameObject wallJumpAbilityIndicator;

    public int airPorts = 0;
    private bool grounded = true;//set in isGrounded()
    private bool groundedWall = false;//true if grounded exclusively to a wall; set in isGrounded()
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

    public AudioClip teleportSound;

    private CameraController mainCamCtr;//the camera controller for the main camera
    private GestureManager gm;
    private HardMaterial hm;

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
        halfWidth = GetComponent<SpriteRenderer>().bounds.extents.magnitude;
        hm = GetComponent<HardMaterial>();
        hm.shattered += shattered;
        fta = GetComponent<ForceTeleportAbility>();
        wca = GetComponent<WallClimbAbility>();
        sba = GetComponent<ShieldBubbleAbility>();
        teleportRangeParticalController = teleportRangeParticalObject.GetComponent<ParticleSystemController>();
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
        else {
            gravity.AcceptsGravity = true;
            if (velocityNeedsReloaded)
            {
                rb2d.velocity = savedVelocity;
                rb2d.angularVelocity = savedAngularVelocity;
                velocityNeedsReloaded = false;
            }
        }
        if (grounded && !rb2d.isKinematic && !isMoving())
        {
            mainCamCtr.discardMovementDelay();
        }
    }

    //void OnCollisionEnter2D(Collision2D coll)
    //{
    //    //airPorts = 0;
    //    //setRange(baseRange);
    //}
    //void OnCollisionExit2D(Collision2D coll)
    //{
    //}

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
        int closestRotationIndex = 0;
        //Figure out current rotation
        float gravityRot = Utility.RotationZ(gravity.Gravity, Vector3.down);
        float currentRotation = transform.localEulerAngles.z - gravityRot;
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
        transform.localEulerAngles = new Vector3(0, 0, angle);
    }

    private bool teleport(Vector3 targetPos)//targetPos is in world coordinations (NOT UI coordinates)
    {
        return teleport(targetPos, true);
    }
    private bool teleport(Vector3 targetPos, bool playSound)//targetPos is in world coordinations (NOT UI coordinates)
    {
        if (teleportTime <= Time.time)
        {
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
            showTeleportEffect(oldPos, newPos);
            if (playSound)
            {
                if (groundedWall && wca.enabled)
                {
                    AudioSource.PlayClipAtPoint(wca.wallClimbSound, oldPos);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(teleportSound, oldPos);
                }
            }
            teleportRangeParticalController.activateTeleportParticleSystem(true, 0);
            //Health Regen
            hm.addIntegrity(Vector2.Distance(oldPos,newPos));
            //Momentum Dampening
            if (rb2d.velocity.magnitude > 0.001f)//if Merky is moving
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
            grounded = false;
            velocityNeedsReloaded = false;//discards previous velocity if was in gravity immunity bubble
            gravityImmuneTime = 0f;
            shouldGrantGIT = true;
            mainCamCtr.delayMovement(0.3f);
            checkGroundedState(true);//have to call it again because state has changed
            return true;
        }
        return false;
    }

    /// <summary>
    /// Finds the teleportable position closest to the given targetPos
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns>targetPos if it is teleportable, else the closest teleportable position to it</returns>
    Vector3 findTeleportablePosition(Vector3 targetPos)
    {
        //TSFS: Teleport Spot Finding System
        Vector3 newPos = targetPos;
        //Determine if new position is in range
        Vector3 oldPos = transform.position;
        if (Vector3.Distance(newPos, transform.position) <= range
            || (GestureManager.CHEATS_ALLOWED && gm.cheatsEnabled))//allow unlimited range while cheat is active
        {
        }
        else
        {
            if (range >= baseRange)
            {
                if (Vector3.Distance(newPos, transform.position) <= range + 2)
                {
                }
            }
            else //teleporting under confinements, such as used up the airports
            {
            }
            newPos = ((newPos - oldPos).normalized * range) + oldPos;
        }

        //Determine if you can even teleport to the position (i.e. is it occupied or not?)
        {
            if (isOccupied(newPos))//test the current newPos first
            {
                List<Vector3> possibleOptions = new List<Vector3>();
                const int pointsToTry = 5;//default to trying 10 points along the line at first
                const float difference = -1 * 1.00f / pointsToTry;//how much the previous jump was different by
                const float variance = 0.4f;//max amount to adjust angle by
                const int anglesToTry = 7;//default to trying 10 points along the line at first
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
                //Evaluate viability of other options
                List<Vector3> viableOptions = new List<Vector3>();
                foreach (Vector3 option in possibleOptions)
                {
                    if (!isOccupied(option))
                    {
                        viableOptions.Add(option);
                    }
                    else {
                        Vector3 adjustedOption = adjustForOccupant(option);
                        if (!isOccupied(adjustedOption))
                        {
                            viableOptions.Add(adjustedOption);
                        }
                    }
                }
                //Choose the closest option 
                float closestDistance = Vector3.Distance(newPos, oldPos);
                Vector3 closestOption = oldPos;
                foreach (Vector3 option in viableOptions)
                {
                    float distance = Vector3.Distance(newPos, option);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
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
        if (useStreak)
        {
            showStreak(oldp, newp);
        }
        if (useStar)
        {
            showTeleportStar(oldp, newp);
        }
        //Check for wall jump
        if (wca.enabled && groundedWall)
        {
            //Play jump effect in addition to teleport star
            wca.playWallClimbEffects(oldp);
        }
    }
    void showStreak(Vector3 oldp, Vector3 newp)
    {
        GameObject newTS = (GameObject)Instantiate(teleportStreak);
        newTS.GetComponent<TeleportStreakUpdater>().start = oldp;
        newTS.GetComponent<TeleportStreakUpdater>().end = newp;
        newTS.GetComponent<TeleportStreakUpdater>().position();
        newTS.GetComponent<TeleportStreakUpdater>().turnOn(true);
    }
    void showTeleportStar(Vector3 oldp, Vector3 newp)
    {
        GameObject newTS = (GameObject)Instantiate(teleportStar);
        newTS.GetComponent<TeleportStarUpdater>().start = oldp;
        newTS.GetComponent<TeleportStarUpdater>().end = newp;
        newTS.GetComponent<TeleportStarUpdater>().position();
        newTS.GetComponent<TeleportStarUpdater>().turnOn(true);
    }



    void setRange(float newRange)
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
            setRange(baseRange);
        }
        else {
            if (exhaust && airPorts >= maxAirPorts)
            {
                setRange(exhaustRange);
            }
        }

    }

    bool isGrounded()
    {
        groundedWall = false;
        bool isgrounded = isGrounded(gravity.Gravity);
        if (!isgrounded && wca.enabled)//if nothing found yet and wall jump is enabled
        {
            isgrounded = isGrounded(Utility.PerpendicularRight(-gravity.Gravity));//right side
            if (!isgrounded)
            {
                isgrounded = isGrounded(Utility.PerpendicularLeft(-gravity.Gravity));//left side
            }
            if (isgrounded)
            {
                groundedWall = true;//grounded exclusively to a wal
            }
        }
        grounded = isgrounded;
        return isgrounded;
    }
    bool isGrounded(Vector3 direction)
    {
        float length = 0.25f;
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        pc2d.Cast(direction, rh2ds, length, true);
        foreach (RaycastHit2D rch2d in rh2ds)
        {
            if (rch2d && rch2d.collider != null && !rch2d.collider.isTrigger)
            {
                GameObject ground = rch2d.collider.gameObject;
                if (ground != null && !ground.Equals(transform.gameObject))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /**
    * Determines whether the given position is occupied or not
    */
    bool isOccupied(Vector3 pos)
    {
        //Debug.DrawLine(pos, pos + new Vector3(0,0.25f), Color.green, 5);
        Vector3 savedOffset = pc2d.offset;
        Vector3 offset = pos - transform.position;
        float angle = transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html
        pc2d.offset = rOffset;
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        pc2d.Cast(Vector2.zero, rh2ds, 0, true);
        //Debug.DrawLine(pc2d.offset+(Vector2)transform.position, pc2d.bounds.center, Color.grey, 10);
        pc2d.offset = savedOffset;
        foreach (RaycastHit2D rh2d in rh2ds)
        {
            if (rh2d.collider == null)
            {
                break;//reached the end of the valid RaycastHit2Ds
            }
            GameObject go = rh2d.collider.gameObject;
            if (!rh2d.collider.isTrigger)
            {
                if (!go.Equals(transform.gameObject))
                {
                    //Debug.Log("Occupying object: " + go.name);
                    return true;
                }

            }
            if (go.tag.Equals("HidableArea") || (go.transform.parent != null && go.transform.parent.gameObject.tag.Equals("HideableArea")))
            {
                if (go.GetComponent<SecretAreaTrigger>() == null)
                {
                    return true;//yep, it's occupied by a hidden area
                }
            }
        }
        return false;//nope, it's not occupied
    }

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
        pc2d.Cast(Vector2.zero, rh2ds, 0, true);
        pc2d.offset = savedOffset;
        foreach (RaycastHit2D rh2d in rh2ds)
        {
            if (rh2d.collider == null)
            {
                break;//reached the end of the valid RaycastHit2Ds
            }
            GameObject go = rh2d.collider.gameObject;
            if (!rh2d.collider.isTrigger)
            {
                if (!go.Equals(transform.gameObject))
                {
                    Vector3 closPos = rh2d.point;
                    Vector3 dir = pos - closPos;
                    Vector3 size = pc2d.bounds.size;
                    float d2 = (size.magnitude / 2) - Vector3.Distance(pos, closPos);
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
            transform.position = ((Vector2)transform.position + GameManager.getLatestSafeRewindGhostPosition())/2;
        }
    }
    public bool isIntact()
    {
        return hm.isIntact();
    }

    public void setIsInCheckPoint(bool iicp)
    {
        inCheckPoint = iicp;
        rb2d.isKinematic = iicp;
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
        return Vector3.Distance(pos, transform.position) < halfWidth;
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
        teleport(newPos);
        mainCamCtr.checkForAutoMovement(gpos, prevPos);
    }
    public void processTapGesture(GameObject checkPoint)
    {
        CheckPointChecker cpc = checkPoint.GetComponent<CheckPointChecker>();
        if (cpc != null)
        {
            Vector3 newPos = checkPoint.transform.position;
            teleport(newPos);
            mainCamCtr.refocus();
            cpc.trigger();
        }
    }


    public void processHoldGesture(Vector3 gpos, float holdTime, bool finished)
    {
        Debug.DrawLine(transform.position, transform.position + new Vector3(0, halfWidth, 0), Color.blue, 10);
        float reducedHoldTime = holdTime - gm.getHoldThreshold();
        //Check Shield Bubble
        if (sba.enabled && gestureOnPlayer(gpos))
        {
            if (fta.enabled) { fta.dropHoldGesture(); }
            sba.processHoldGesture(gpos, reducedHoldTime, finished);
        }
        //Check Force Wave
        else if (fta.enabled)
        {
            if (sba.enabled) { sba.dropHoldGesture(); }
            Vector3 newPos = gpos;
            if (finished)
            {
                fta.processHoldGesture(newPos, reducedHoldTime, finished);
            }
            else {
                fta.processHoldGesture(newPos, reducedHoldTime, finished);
            }
        }
        else
        {//neither force wave nor shield bubble are active, probably meant to tap
            if (finished)
            {
                gm.adjustHoldThreshold(holdTime);
                processTapGesture(gpos);
            }
        }
    }
    public void dropHoldGesture()
    {
        if (fta.enabled) { fta.dropHoldGesture(); }
        if (sba.enabled) { sba.dropHoldGesture(); }
    }
}

   
