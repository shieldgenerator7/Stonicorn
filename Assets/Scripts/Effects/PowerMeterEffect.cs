using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerMeterEffect : MonoBehaviour
{
    public float maxHeight = 1;
    public float minHeight = 0;
    [Tooltip("True: expand right. False: expand up.")]
    public bool expandRight = false;

    private IPowerConduit conduit;
    // Start is called before the first frame update
    void Start()
    {
        conduit = GetComponent<IPowerConduit>();
        if (conduit == null)
        {
            conduit = GetComponentInParent<IPowerConduit>();
        }
        conduit.OnPowerFlowed += onPowerFlowed;
    }
    private void OnDestroy()
    {
        if (conduit != null)
        {
            conduit.OnPowerFlowed -= onPowerFlowed;
        }
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
        if (expandRight)
        {
            if (Mathf.Abs(transform.localScale.x - newHeight) > 0.0001f)
            {
                Vector3 curScale = transform.localScale;
                transform.localScale = new Vector3(newHeight, curScale.y, curScale.z);
            }
        }
        else
        {
            if (Mathf.Abs(transform.localScale.y - newHeight) > 0.0001f)
            {
                Vector3 curScale = transform.localScale;
                transform.localScale = new Vector3(curScale.x, newHeight, curScale.z);
            }
        }
    }
}
