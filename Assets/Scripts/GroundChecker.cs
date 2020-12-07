using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GravityAccepter))]
public class GroundChecker : SavableMonoBehaviour
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
    /// <summary>
    /// True if grounded at all
    /// </summary>
    public bool Grounded { get; private set; }

    /// <summary>
    /// True if grounded in the direction of gravity
    /// </summary>
    public bool GroundedNormal { get; private set; }

    /// <summary>
    /// True if an ability says it's grounded,
    /// such as to a wall or after a swap
    /// </summary>
    public bool GroundedAbility { get; private set; }

    public delegate bool IsGroundedCheck();
    public event IsGroundedCheck isGroundedCheck;

    //Grounded Previously
    public bool GroundedPrev { get; private set; }
    //Grounded Previously in gravity direction
    public bool GroundedNormalPrev { get; private set; }
    //Grounded Previously by an ability
    public bool GroundedAbilityPrev { get; private set; }

    public void checkGroundedState()
    {
        //Grounded at all
        GroundedPrev = Grounded;
        checkGroundedStateNormal();
        Grounded = GroundedNormal;
        //If it's grounded normally,
        if (Grounded)
        {
            //It's not going to even check the abilities
            GroundedAbilityPrev = GroundedAbility;
            GroundedAbility = false;
        }
        else
        {
            //Else, check the abilities
            checkGroundedStateAbility();
            Grounded = GroundedAbility;
        }
        updateLastGroundedTime(Grounded);
    }
    private void checkGroundedStateNormal()
    {
        GroundedNormalPrev = GroundedNormal;
        GroundedNormal = isGroundedInDirection(Gravity.Gravity);
    }
    private void checkGroundedStateAbility()
    {
        GroundedAbilityPrev = GroundedAbility;
        if (isGroundedCheck != null)
        {
            //If at least 1 delegate returns true,
            //Merky is grounded
            GroundedAbility = isGroundedCheck.GetInvocationList().ToList()
                .Any(del => ((IsGroundedCheck)del).Invoke());
        }
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
            if (!rch2d.collider.isTrigger
                && !rch2d.collider.gameObject.isPlayer())
            {
                //There is ground in the given direction
                return true;
            }
        }
        //Else, There is no ground in the given direction
        return false;
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "Grounded", Grounded,
            "GroundedNormal", GroundedNormal,
            "GroundedAbility", GroundedAbility,
            "GroundedPrev", GroundedPrev,
            "GroundedNormalPrev", GroundedNormalPrev,
            "GroundedAbilityPrev", GroundedAbilityPrev
            );
        set
        {
            Grounded = value.Bool("Grounded");
            GroundedNormal = value.Bool("GroundedNormal");
            GroundedAbility = value.Bool("GroundedAbility");
            GroundedPrev = value.Bool("GroundedPrev");
            GroundedNormalPrev = value.Bool("GroundedNormalPrev");
            GroundedAbilityPrev = value.Bool("GroundedAbilityPrev");
        }
    }
}
