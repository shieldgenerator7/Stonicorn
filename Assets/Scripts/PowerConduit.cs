using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerConduit : SavableMonoBehaviour
{
    //2017-06-20: copied from PowerCubeController
    //These wires transfer energy from a source (such as a Shield Bubble) and convert it to energy to power other objects (such as doors)

    public float maxEnergyLevel = 3;//how much energy this conduit can store
    public float maxEnergyPerSecond = 2;//(energy units per second) the max amount of energy that it can produce/move per second
    [SerializeField]
    private float currentEnergyLevel = 0;//the current amount of energy it has to spend
    public float Energy
    {
        get { return currentEnergyLevel; }
        private set { }
    }
    public bool givesEnergy = true;//whether this can give power to other PowerConduits
    public virtual bool takesEnergy => true;//whether this can take power from other PowerConduits
    public virtual bool convertsToEnergy => false;//whether this can convert other sources to energy
    public virtual bool usesEnergy => true;//whether this uses energy to power some mechanism

    private BoxCollider2D bc2d;
    private RaycastHit2D[] rh2ds = new RaycastHit2D[Utility.MAX_HIT_COUNT];//used for detection of other PowerConduits

    // Use this for initialization
    protected virtual void Start()
    {
        bc2d = GetComponent<BoxCollider2D>();
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "currentEnergyLevel", currentEnergyLevel
            );
        set
        {
            currentEnergyLevel = value.Float("currentEnergyLevel");
            adjustEnergy(0);
        }
    }

    void OnTriggerStay2D(Collider2D coll)
    {
        if (takesEnergy && currentEnergyLevel < maxEnergyPerSecond)
        {
            PowerConduit pc = coll.gameObject.GetComponent<PowerConduit>();
            if (pc)
            {
                if (pc.givesEnergy
                    && (!pc.takesEnergy
                        || pc.currentEnergyLevel > currentEnergyLevel
                        || usesEnergy
                        )
                    )
                {
                    float amountGiven = pc.giveEnergyToObject(
                        maxEnergyPerSecond,
                        Time.fixedDeltaTime
                        );
                    adjustEnergy(amountGiven);
                }
            }
        }
    }

    /// <summary>
    /// Given the max amount of energy available,
    /// it returns the amount of energy it takes
    /// </summary>
    /// <param name="maxAvailable"></param>
    /// <param name="deltaTime">so it can convert from per second to per frame</param>
    /// <returns></returns>
    public float takeEnergyFromSource(float maxAvailable, float deltaTime)
    {
        if (!takesEnergy)
        {
            throw new System.MethodAccessException("PowerConduit.takeEnergyFromSource(..) should not be called on this object because its takesEnergy var is: " + takesEnergy);
        }
        float amountTaken = Mathf.Min(maxEnergyPerSecond * deltaTime, maxAvailable);
        adjustEnergy(amountTaken);
        return amountTaken;
    }
    /// <summary>
    /// Given the amount of energy requested (per second),
    /// it returns how much it can give
    /// </summary>
    /// <param name="amountRequested"></param>
    /// <param name="deltaTime">so it can convert from per second to per frame</param>
    /// <returns></returns>
    public float giveEnergyToObject(float amountRequested, float deltaTime)
    {
        if (!givesEnergy)
        {
            throw new System.MethodAccessException("PowerConduit.giveEnergyToObject(..) should not be called on this object because its givesEnergy var is: " + givesEnergy);
        }
        float amountGiven = Mathf.Min(amountRequested * deltaTime, currentEnergyLevel);
        adjustEnergy(-amountGiven);
        return amountGiven;
    }
    /// <summary>
    /// Given the max amount of energy available to convert,
    /// it returns the amount of energy it converts
    /// Used to create energy for a power system
    /// </summary>
    /// <param name="maxAvailable"></param>
    /// <param name="deltaTime">so it can convert from per second to per frame</param>
    /// <returns></returns>
    public float convertSourceToEnergy(float maxAvailable, float deltaTime)
    {
        if (!convertsToEnergy)
        {
            throw new System.MethodAccessException("PowerConduit.convertSourceToEnergy(..) should not be called on this object because its convertsToEnergy var is: " + convertsToEnergy);
        }
        float amountTaken = Mathf.Min(maxEnergyPerSecond * deltaTime, maxAvailable);
        adjustEnergy(amountTaken);
        return amountTaken;
    }
    /// <summary>
    /// Given the amount of energy requested (per second),
    /// it returns how much it can give to be used
    /// Used to power a mechanism, reducing the energy in a power system
    /// </summary>
    /// <param name="amountRequested"></param>
    /// <param name="deltaTime">so it can convert from per second to per frame</param>
    /// <returns></returns>
    public float useEnergy(float amountRequested, float deltaTime)
    {
        if (!usesEnergy)
        {
            throw new System.MethodAccessException("PowerConduit.useEnergy(..) should not be called on this object because its usesEnergy var is: " + usesEnergy);
        }
        float amountGiven = Mathf.Min(amountRequested * deltaTime, currentEnergyLevel);
        adjustEnergy(-amountGiven);
        return amountGiven;
    }

    /// <summary>
    /// Adds the given delta to the current energy level
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public float adjustEnergy(float delta)
    {
        //Adjust the value
        currentEnergyLevel += delta;
        //Clamp the value
        currentEnergyLevel = Mathf.Clamp(currentEnergyLevel, 0, maxEnergyLevel);
        //Return its current energy
        return currentEnergyLevel;
    }
}
