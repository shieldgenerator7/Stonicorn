
using System.Collections.Generic;
using UnityEngine;

/**
 * Activates its GameObject when its parent IPowerConduit has power
 */
public class PoweredEnabler : MonoBehaviour
{
    [Range(0f, 1f)]
    public float percentRequired = 1f;
    public bool allowTurnOff = true;

    public List<MonoBehaviour> compList;

    private bool currentlyOn = false;
    private IPowerConduit conduit;

    void Start()
    {
        conduit = GetComponent<IPowerConduit>() ?? GetComponentInParent<IPowerConduit>();
        conduit.OnPowerFlowed += onPowerFlowed;
        turnOn(false);
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
        bool enable = power >= maxPower * percentRequired;
        if ((enable || allowTurnOff) && currentlyOn != enable) {
            currentlyOn = enable;
            turnOn(enable);
        }
    }

    void turnOn(bool enable)
    {
        compList.ForEach(go => go.enabled = enable);
    }
}
