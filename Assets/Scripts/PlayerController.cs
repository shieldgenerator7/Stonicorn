﻿using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float range = 3;
    public float baseRange = 3;
    public float exhaustRange = 1;
    public int maxAirPorts = 0;
    public float exhaustCoolDownTime = 0.5f;//the cool down time for teleporting while exhausted in seconds
    public float teleportTime = 0f;//the earliest time that Merky can teleport

    public int teleportXP = 0;
    public int txpLevelUpRequirement = 1;
    public int lastLevel = 1;

    public GameObject teleportStreak;

    public int airPorts = 0;
    private Rigidbody2D rb2d;
    private int exceptionFrame = 0;//true if this frame it should not count grounded (CODE HAZARD)

    private bool isTeleportGesture;

    public AudioClip teleportSound;

    Vector3[] dirs = new Vector3[]
            {//for checking if Merky is grounded
                //Vector3.up,
                Vector3.down,
                //Vector3.left,
                //Vector3.right,
                //new Vector3(1,1),
                //new Vector3(-1,1),
                new Vector3(0.75f,-1),
                new Vector3(-0.75f,-1),

                //new Vector3(1,.5f),
                //new Vector3(-1,.5f),
                //new Vector3(1,-.5f),
                //new Vector3(-1,-.5f),
                //new Vector3(.5f,1),
                //new Vector3(-.5f,1),
                //new Vector3(.5f,-1),
                //new Vector3(-.5f,-1),
            };
    Vector3[] checkDirs = new Vector3[]
                {//for checking area around teleport target point
                Vector3.up,
                Vector3.down,
                Vector3.left,
                Vector3.right,
                new Vector3(1,1),
                new Vector3(-1,1),
                new Vector3(1,-1),
                new Vector3(-1,-1),

                new Vector3(1,.5f),
                new Vector3(-1,.5f),
                new Vector3(1,-.5f),
                new Vector3(-1,-.5f),
                new Vector3(.5f,1),
                new Vector3(-.5f,1),
                new Vector3(.5f,-1),
                new Vector3(-.5f,-1),
                };

    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        Input.simulateMouseWithTouches = false;
	}

    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        Vector2 pos2 = new Vector2(pos.x, pos.y);
        //foreach (Vector3 dir in dirs)
        //{
        //    Vector2 dir2 = new Vector2(dir.x, dir.y);
        //    float length = 1.3f;
        //    dir2 = dir2.normalized * length;
        //    Vector2 start = (pos2 + dir2);
        //    Debug.DrawLine(pos2, start, Color.black);
        //}
        if (exceptionFrame <= 0)
        {
            checkGroundedState();
        }
        //else
        //{
        //    exceptionFrame--;
        //}
    }
	
	// Update is called once per frame
	void Update () {
        if (exceptionFrame > 0)
        {
            exceptionFrame--;
        }

        if (Input.touchCount == 0)
        {
            isTeleportGesture = true;
        }
        else if (Input.touchCount >= 2)
        {
            isTeleportGesture = false;
        }
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (isTeleportGesture)//don't let the pinch zoom gesture count as a teleport gesture
                {
                    teleport(false);
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
            }
            else if (Input.GetMouseButtonUp(0))
            {
                teleport(true);                
            }
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

    void teleport(bool mouseInput)
    {
        if (teleportTime <= Time.time)
        {
            if (airPorts > maxAirPorts)
            {
                teleportTime = Time.time + exhaustCoolDownTime;
            }
            //Get new position
            Vector3 click;
            if (mouseInput)
            {
                click = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                click = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            Vector3 newPos = new Vector3(click.x, click.y);

            //Determine if new position is in range
            Vector3 oldPos = transform.position;
            int bonusTXP = 0;
            if (Vector3.Distance(newPos, transform.position) <= range)
            {
            }
            else
            {
                if (range >= baseRange)
                {
                    if (Vector3.Distance(newPos, transform.position) <= range + 2)
                    {
                        bonusTXP = 1;
                    }
                }
                else //teleporting under confinements, such as used up the airports
                {
                    bonusTXP = -1;//don't give any txp for teleporting beyond max air ports
                }
                newPos = ((newPos - oldPos).normalized * range) + oldPos;
            }

            //Determine if you can even teleport to the position (i.e. is it occupied or not?)
            {
                if (isOccupied(newPos))//test the current newPos first
                {
                    //Back-tracking
                    Vector3 btNewPos = newPos;
                    float distance = Vector3.Distance(oldPos, newPos);
                    int pointsToTry = 10;//default to trying 10 points along the line at first
                    float difference = -1 * 1.00f / pointsToTry;//how much the previous jump was different by
                    float percent = 1.00f;
                    bool keepTrying = true;
                    Vector3 norm = (newPos - oldPos).normalized;
                    while (keepTrying)
                    {
                        percent += difference;//actually subtraction in usual case, b/c "difference" is usually negative
                        Vector3 testPos = (norm * distance * percent) + oldPos;
                        if (isOccupied(testPos))
                        {
                        }
                        else
                        {
                            //found an open spot (tho it might not be optimal)
                            keepTrying = false;
                            btNewPos = testPos;
                        }
                    }

                    //Try a cardinal direction
                    //Figure out which cardinal direction is closest to the one they're trying to go to: up, down, left, or right
                    //whichever difference is less, is the one that's closer
                    Vector3 cdNewPos = newPos;
                    if (Mathf.Abs(oldPos.x - newPos.x) < Mathf.Abs(oldPos.y - newPos.y))
                    {//it is closer in x direction, go up or down
                        if (oldPos.y > newPos.y)
                        {//go down
                            cdNewPos = oldPos + Vector3.down * distance;
                        }
                        else if (oldPos.y < newPos.y)
                        {//go up
                            cdNewPos = oldPos + Vector3.up * distance;
                        }
                    }
                    else if (Mathf.Abs(oldPos.x - newPos.x) >= Mathf.Abs(oldPos.y - newPos.y))//default: left or right
                    {//it is closer in y direction, go left or right
                        if (oldPos.x > newPos.x)
                        {//go left
                            cdNewPos = oldPos + Vector3.left * distance;
                        }
                        else if (oldPos.x < newPos.x)
                        {//go right
                            cdNewPos = oldPos + Vector3.right * distance;
                        }
                    }
                    bool btOcc = isOccupied(btNewPos);
                    bool cdOcc = isOccupied(cdNewPos);
                    if (btOcc && ! cdOcc)
                    {
                        newPos = cdNewPos;
                    }
                    else if ( ! btOcc && cdOcc)
                    {
                        newPos = btNewPos;
                    }
                    else if (btOcc && cdOcc)
                    {
                        return;//the back up plan failed, just return, can't teleport
                    }
                    else if ( ! btOcc && ! cdOcc)
                    {
                        //Whichever new pos goes further is the winner.
                        float btDist = Vector3.Distance(oldPos, btNewPos);
                        float cdDist = Vector3.Distance(oldPos, cdNewPos);
                        if (cdDist > btDist)
                        {
                            newPos = cdNewPos;
                        }
                        else //default to btNewPos
                        {
                            newPos = btNewPos;
                        }
                    }
                    else
                    {
                        //ERROR! It should not be able to come here!
                    }
                }
            }
            
            //Actually Teleport
            transform.position = newPos;
            showStreak(oldPos, newPos);
            AudioSource.PlayClipAtPoint(teleportSound, oldPos);
            //Give teleport xp
            teleportXP += 1 + bonusTXP;
            if (teleportXP >= txpLevelUpRequirement)
            {
                int lls = txpLevelUpRequirement;
                txpLevelUpRequirement += txpLevelUpRequirement - lastLevel + 1;
                lastLevel = lls;
                baseRange += 0.1f;
                //setRange(baseRange);
            }
        }
        if ( ! isGrounded())
        {
            airPorts++;
        }
        exceptionFrame = 5;
    }

    void showStreak(Vector3 oldp, Vector3 newp)
    {
        GameObject newTS = (GameObject)Instantiate(teleportStreak);
        newTS.GetComponent<TeleportStreakUpdater>().start = oldp;
        newTS.GetComponent<TeleportStreakUpdater>().end = newp;
        newTS.GetComponent<TeleportStreakUpdater>().position();
        newTS.GetComponent<TeleportStreakUpdater>().turnOn(true);
    }

    void setRange(float newRange)
    {
        range = newRange;
        TeleportRangeIndicatorUpdater tri = GetComponentInChildren<TeleportRangeIndicatorUpdater>();
        tri.updateRange();
    }

    void checkGroundedState()
    {        
        if (isGrounded())
        {
            airPorts = 0;
            setRange(baseRange);
        }
        else {
            if (airPorts >= maxAirPorts)
            {
                setRange(exhaustRange);
            }
        }

    }

    bool isGrounded()
    {
        bool isGrounded = false;

        Vector3 pos = transform.position;
        Vector2 pos2 = new Vector2(pos.x, pos.y);
        int numberOfLines = 5;
        Bounds bounds = GetComponent<PolygonCollider2D>().bounds;
        float width = bounds.max.x - bounds.min.x;
        float increment = width / (numberOfLines-1);//-1 because the last one doesn't take up any space
        Vector3 startV = bounds.min;
        float length = 1.7f;
        for (int i = 0; i < numberOfLines; i++)
        {
            Vector2 start = new Vector2(startV.x + i*increment, pos.y-length);
            Vector2 dir2 = new Vector2(0, length);
            Debug.DrawLine(start, start+dir2, Color.black, 0.1f);
            RaycastHit2D rch2d = Physics2D.Raycast(start, dir2, length);// -1*(start), 1f);
            if (rch2d && rch2d.collider != null)
            {
                GameObject ground = rch2d.collider.gameObject;
                if (ground != null && !ground.Equals(transform.gameObject))
                {
                    isGrounded = true;
                    break;
                }
            }
        }
        return isGrounded;
    }

    /**
    * Determines whether the given position is occupied or not
    */
    bool isOccupied(Vector3 pos)
    {
        Vector2 pos2 = new Vector2(pos.x, pos.y);
        foreach (Vector3 checkDir in checkDirs)
        {
            Vector2 dir2 = new Vector2(checkDir.x, checkDir.y);
            float length = 0.1f;// 1.7f;
            dir2 = dir2.normalized * length;
            Vector2 start = (pos2 + dir2);
            //Debug.DrawLine(pos2, start, Color.black, 1);
            RaycastHit2D rch2d = Physics2D.Raycast(start, -1 * dir2, length);// -1*(start), 1f);
            if (rch2d && rch2d.collider != null)
            {
                GameObject ground = rch2d.collider.gameObject;
                if (ground != null && !ground.Equals(transform.gameObject))
                {
                    return true;//yep, it's occupied
                }
            }
        }
        return false;//nope, it's not occupied
    }
}
