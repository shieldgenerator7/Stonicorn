using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerConduit : SavableMonoBehaviour
{
    //2017-06-20: copied from PowerCubeController
    //These wires transfer energy from a source (such as a Shield Bubble) and convert it to energy to power other objects (such as doors)

    public GameObject lightEffect;//the object attached to it that it uses to show it is lit up
    public bool useAlpha = true;//whether to update the lightEffect with alpha value. If false, it uses height (used for power cubes)
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
    public bool takesEnergy = true;//whether this can take power from other PowerConduits
    public bool convertsToEnergy = false;//whether this can convert other sources to energy
    public bool usesEnergy = false;//whether this uses energy to power some mechanism

    private SpriteRenderer lightEffectRenderer;
    private Color lightEffectColor;
    private BoxCollider2D bc2d;
    private RaycastHit2D[] rh2ds = new RaycastHit2D[Utility.MAX_HIT_COUNT];//used for detection of other PowerConduits

    // Use this for initialization
    void Start()
    {
        initLightEffectRenderer();
        bc2d = GetComponent<BoxCollider2D>();
    }

    public void initLightEffectRenderer()
    {
        if (useAlpha)
        {
            lightEffectRenderer = lightEffect.GetComponent<SpriteRenderer>();
            if (lightEffectRenderer)
            {
                lightEffectRenderer.size = GetComponent<SpriteRenderer>().size;
                lightEffectColor = lightEffectRenderer.color;
            }
            else
            {
                Debug.LogError("UseAlpha was set but there is no SpriteRenderer on the lightEffect ("
                    + lightEffect.name + "), so switching to not use alpha. GameObject: " + gameObject.name);
                useAlpha = false;
            }
        }
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this, "currentEnergyLevel", currentEnergyLevel);
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        currentEnergyLevel = (float)savObj.data["currentEnergyLevel"];
        if (!lightEffectRenderer)
        {
            initLightEffectRenderer();
        }
        adjustEnergy(0);
    }

    void OnTriggerStay2D(Collider2D coll)
    {
        if (takesEnergy && currentEnergyLevel < maxEnergyPerSecond)
        {
            PowerConduit pc = coll.gameObject.GetComponent<PowerConduit>();
            if (pc != null)
            {
                if (pc.givesEnergy && (!pc.takesEnergy || pc.currentEnergyLevel > currentEnergyLevel || usesEnergy))
                {
                    float amountGiven = pc.giveEnergyToObject(maxEnergyPerSecond, Time.fixedDeltaTime);
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
        //Update the visuals
        if (useAlpha)
        {
            //2017-01-24: copied from my project: https://github.com/shieldgenerator7/GGJ-2017-Wave/blob/master/Assets/Script/CatTongueController.cs
            float newHigh = 2.0f;//opaque
            float newLow = 0.0f;//transparent
            float curHigh = maxEnergyLevel;
            float curLow = 0;
            float newAlpha = ((currentEnergyLevel - curLow) * (newHigh - newLow) / (curHigh - curLow)) + newLow;
            if (Mathf.Abs(lightEffectRenderer.color.a - newAlpha) > 0.01f)
            {
                lightEffectRenderer.color = new Color(lightEffectColor.r, lightEffectColor.g, lightEffectColor.b, newAlpha);
            }
        }
        else
        {//use mask height
            //2017-12-23: copied from the section above for useAlpha == true
            float newHigh = 1.0f;//full height
            float newLow = 0.0f;//no height
            float curHigh = maxEnergyLevel;
            float curLow = 0;
            float newHeight = ((currentEnergyLevel - curLow) * (newHigh - newLow) / (curHigh - curLow)) + newLow;
            if (Mathf.Abs(lightEffect.transform.localScale.y - newHeight) > 0.0001f)
            {
                Vector3 curScale = lightEffect.transform.localScale;
                lightEffect.transform.localScale = new Vector3(curScale.x, newHeight, curScale.z);
            }
        }
        return currentEnergyLevel;
    }
}
