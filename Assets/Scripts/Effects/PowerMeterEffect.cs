using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerMeterEffect : MonoBehaviour
{
    public GameObject lightEffect;//the object attached to it that it uses to show it is lit up

    public float maxHeight = 1;
    public float minHeight = 0;

    private IPowerConduit conduit;
    // Start is called before the first frame update
    void Start()
    {
        initLightEffect();
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
        //use mask height
        //2017-12-23: copied from the section above for useAlpha == true
        float newHigh = maxHeight;//full height
        float newLow = minHeight;//no height
        float curHigh = maxPower;
        float curLow = 0;
        float newHeight = ((power - curLow) * (newHigh - newLow) / (curHigh - curLow)) + newLow;
        if (Mathf.Abs(lightEffect.transform.localScale.y - newHeight) > 0.0001f)
        {
            Vector3 curScale = lightEffect.transform.localScale;
            lightEffect.transform.localScale = new Vector3(curScale.x, newHeight, curScale.z);
        }
    }

    public void initLightEffect()
    {
        if (lightEffect == null)
        {
            lightEffect = gameObject;
        }
    }
}
