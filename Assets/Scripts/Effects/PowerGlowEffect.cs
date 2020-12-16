using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a glow effect when the IPowerConduit in its parent object has power
/// </summary>
public class PowerGlowEffect : MonoBehaviour
{
    public GameObject lightEffect;//the object attached to it that it uses to show it is lit up

    private IPowerConduit conduit;
    private SpriteRenderer lightEffectRenderer;
    private Color lightEffectColor;
    // Start is called before the first frame update
    void Start()
    {
        initLightEffectRenderer();
        conduit = GetComponent<IPowerConduit>();
        if (conduit == null)
        {
            conduit = GetComponentInParent<IPowerConduit>();
        }
        conduit.OnPowerFlowed += onPowerFlowed;
    }

    void onPowerFlowed(float power, float maxPower)
    {
        //Update the visuals
        //2017-01-24: copied from my project: https://github.com/shieldgenerator7/GGJ-2017-Wave/blob/master/Assets/Script/CatTongueController.cs
        float newHigh = 1.0f;//opaque
        float newLow = 0.0f;//transparent
        float curHigh = maxPower;
        float curLow = 0;
        float newAlpha = ((power - curLow) * (newHigh - newLow) / (curHigh - curLow)) + newLow;
        lightEffectRenderer.color = new Color(
            lightEffectColor.r,
            lightEffectColor.g,
            lightEffectColor.b,
            newAlpha
            );
    }

    public void initLightEffectRenderer()
    {
        if (lightEffect == null)
        {
            lightEffect = gameObject;
        }
        lightEffectRenderer = lightEffect.GetComponent<SpriteRenderer>();
        if (lightEffectRenderer)
        {
            lightEffectRenderer.size = GetComponent<SpriteRenderer>().size;
            lightEffectColor = lightEffectRenderer.color;
        }
        else
        {
            Debug.LogError(
                "UseAlpha was set but there is no SpriteRenderer on the lightEffect ("
                + lightEffect.name + "), so switching to not use alpha.",
                gameObject
                );
        }
    }

}
