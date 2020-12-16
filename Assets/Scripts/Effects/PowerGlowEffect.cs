using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a glow effect when the IPowerConduit in its parent object has power
/// </summary>
public class PowerGlowEffect : MonoBehaviour
{
    public GameObject lightEffect;//the object attached to it that it uses to show it is lit up
    public bool useAlpha = true;//whether to update the lightEffect with alpha value. If false, it uses height (used for power cubes)

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
        if (useAlpha)
        {
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
        else
        {//use mask height
            //2017-12-23: copied from the section above for useAlpha == true
            float newHigh = 1.0f;//full height
            float newLow = 0.0f;//no height
            float curHigh = maxPower;
            float curLow = 0;
            float newHeight = ((power - curLow) * (newHigh - newLow) / (curHigh - curLow)) + newLow;
            if (Mathf.Abs(lightEffect.transform.localScale.y - newHeight) > 0.0001f)
            {
                Vector3 curScale = lightEffect.transform.localScale;
                lightEffect.transform.localScale = new Vector3(curScale.x, newHeight, curScale.z);
            }
        }
    }

    public void initLightEffectRenderer()
    {
        if (lightEffect == null)
        {
            lightEffect = gameObject;
        }
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
                Debug.LogError(
                    "UseAlpha was set but there is no SpriteRenderer on the lightEffect ("
                    + lightEffect.name + "), so switching to not use alpha.",
                    gameObject
                    );
                useAlpha = false;
            }
        }
    }

}
