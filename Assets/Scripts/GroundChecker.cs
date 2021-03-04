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

    public GravityAccepter Gravity;

    private List<PlayerAbility> groundedAbilities = new List<PlayerAbility>();
    private List<PlayerAbility> groundedAbilitiesPrev = new List<PlayerAbility>();

    private void Start()
    {
        init();
    }
    public override void init()
    {
        Gravity = GetComponent<GravityAccepter>();
        Grounded = false;
        GroundedNormal = false;
        GroundedAbility = false;
        GroundedPrev = false;
        GroundedNormalPrev = false;
        GroundedAbilityPrev = false;
    }

    //
    //Grounded state variables
    //
    /// <summary>
    /// True if grounded at all
    /// </summary>
    public bool Grounded { get; private set; }

    /// <summary>
    /// True if grounded in the direction of gravity.
    /// You might want to use isGroundedWithoutAbility() instead
    /// if you want to detect if grounded by other abilities
    /// </summary>
    public bool GroundedNormal { get; private set; }

    /// <summary>
    /// True if an ability says it's grounded,
    /// such as to a wall or after a swap
    /// </summary>
    public bool GroundedAbility { get; private set; }

    //Grounded Previously
    public bool GroundedPrev { get; private set; }
    /// <summary>
    /// Grounded Previously in gravity direction
    /// You might want to use isGroundedWithoutAbility() instead
    /// if you want to detect if grounded by other abilities
    /// </summary>
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
    }
    private void checkGroundedStateNormal()
    {
        GroundedNormalPrev = GroundedNormal;
        GroundedNormal = isGroundedInDirection(Gravity.Gravity);
    }
    private void checkGroundedStateAbility()
    {
        GroundedAbilityPrev = GroundedAbility;
        groundedAbilitiesPrev.Clear();
        groundedAbilitiesPrev.AddRange(groundedAbilities);
        groundedAbilities.Clear();
        if (isGroundedCheck != null)
        {
            //If at least 1 delegate returns true,
            //Merky is grounded
            isGroundedCheck.GetInvocationList()
                .Cast<IsGroundedCheck>().ToList()
                .FindAll(igc => igc.Invoke())
                .ForEach(igc => groundedAbilities.Add((PlayerAbility)igc.Target));
            GroundedAbility = groundedAbilities.Count > 0;
        }
    }
    public delegate bool IsGroundedCheck();
    public event IsGroundedCheck isGroundedCheck;

    /// <summary>
    /// Returns true if there is ground in the given direction relative to Merky
    /// </summary>
    /// <param name="direction">The direction to check for ground in</param>
    /// <returns>True if there is ground in the given direction</returns>
    public bool isGroundedInDirection(Vector3 direction, float distance = -1)
    {
        if (distance < 0)
        {
            distance = groundTestDistance;
        }
        //Find objects in the given direction
        Utility.RaycastAnswer answer;
        answer = coll2d.CastAnswer(direction, distance, true);
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

    /// <summary>
    /// Returns true if there was a reason for being grounded 
    /// other than the given ability
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public bool isGroundedWithoutAbility(params PlayerAbility[] abilities)
        => GroundedNormal
        || groundedAbilities.Any(gpa => !abilities.Contains(gpa));

    /// <summary>
    /// Returns true if there was a reason for being grounded previously
    /// other than the given ability
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public bool isGroundedPrevWithoutAbility(PlayerAbility ability)
        => GroundedNormalPrev || groundedAbilitiesPrev.Any(gpa => gpa != ability);

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
