using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a glow effect when the IPowerConduit in its parent object has power
/// </summary>
public class PowerGlowEffect : MonoBehaviour
{
    private IPowerConduit conduit;
    private SpriteRenderer lightEffectRenderer;
    // Start is called before the first frame update
    void Start()
    {
        lightEffectRenderer = GetComponent<SpriteRenderer>();
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
        lightEffectRenderer.color = lightEffectRenderer.color.adjustAlpha(newAlpha);
    }

}
