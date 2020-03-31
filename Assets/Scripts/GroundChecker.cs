using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GravityAccepter))]
public class GroundChecker : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 0.5f)]
    public float groundTestDistance = 0.25f;//how far from Merky the ground test should go

    [Header("Components")]
    [SerializeField]
    private Collider2D coll2d;

    private GravityAccepter gravity;
    public GravityAccepter Gravity
    {
        get
        {
            if (gravity == null)
            {
                gravity = GetComponent<GravityAccepter>();
            }
            return gravity;
        }
    }

    [SerializeField]
    private float lastGroundedTime = 0;
    /// <summary>
    /// Returns the last time that the Grounded check succeeded.
    /// Note that if the Grounded check was not done, this value may be old
    /// </summary>
    public float LastGroundedTime
    {
        get
        {
            return lastGroundedTime;
        }
        private set
        {
            lastGroundedTime = Mathf.Clamp(value, 0, Managers.Time.Time);
        }
    }
    void updateLastGroundedTime(bool groundedCheckSuccess)
    {
        if (groundedCheckSuccess)
        {
            LastGroundedTime = Managers.Time.Time;
        }
    }

    //
    //Grounded state variables
    //
    internal bool grounded { get; private set; }//true if grounded at all
    public bool Grounded
    {
        get
        {
            GroundedPrev = grounded;
            grounded = GroundedNormal;
            //If it's grounded normally,
            if (grounded)
            {
                //It's not going to even check the abilities
                GroundedAbilityPrev = groundedAbility;
                groundedAbility = false;
            }
            else
            {
                //Else, check the abilities
                grounded = GroundedAbility;
            }
            updateLastGroundedTime(grounded);
            return grounded;
        }
    }

    private bool groundedNormal = true;//true if grounded to the direction of gravity
    public bool GroundedNormal
    {
        get
        {
            GroundedNormalPrev = groundedNormal;
            groundedNormal = isGroundedInDirection(Gravity.Gravity);
            updateLastGroundedTime(groundedNormal);
            return groundedNormal;
        }
    }

    private bool groundedAbility = false;//true if grounded to a wall
    public bool GroundedAbility
    {
        get
        {
            GroundedAbilityPrev = groundedAbility;
            groundedAbility = false;
            //Check isGroundedCheck delegates
            if (isGroundedCheck != null)
            {
                foreach (IsGroundedCheck igc in isGroundedCheck.GetInvocationList())
                {
                    bool result = igc.Invoke();
                    //If at least 1 returns true,
                    if (result == true)
                    {
                        //Merky is grounded
                        groundedAbility = true;
                        break;
                    }
                }
            }
            updateLastGroundedTime(groundedAbility);
            return groundedAbility;
        }
    }
    public delegate bool IsGroundedCheck();
    public IsGroundedCheck isGroundedCheck;

    //Grounded Previously
    private bool groundedPrev;
    public bool GroundedPrev
    {
        get { return groundedPrev; }
        private set { groundedPrev = value; }
    }
    //Grounded Previously in gravity direction
    private bool groundedNormalPrev;
    public bool GroundedNormalPrev
    {
        get { return groundedNormalPrev; }
        private set { groundedNormalPrev = value; }
    }
    //Grounded Previously by an ability
    private bool groundedAbilityPrev;
    public bool GroundedAbilityPrev
    {
        get { return groundedAbilityPrev; }
        private set { groundedAbilityPrev = value; }
    }

    /// <summary>
    /// Returns true if there is ground in the given direction relative to Merky
    /// </summary>
    /// <param name="direction">The direction to check for ground in</param>
    /// <returns>True if there is ground in the given direction</returns>
    public bool isGroundedInDirection(Vector3 direction)
    {
        //Find objects in the given direction
        Utility.RaycastAnswer answer;
        answer = coll2d.CastAnswer(direction, groundTestDistance, true);
        //Process the found objects
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            //If the object is a solid object,
            if (!rch2d.collider.isTrigger)
            {
                //There is ground in the given direction
                return true;
            }
        }
        //Else, There is no ground in the given direction
        return false;
    }
}
