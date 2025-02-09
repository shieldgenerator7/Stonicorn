using TMPro;
using UnityEngine;

/**
 * Activates its GameObject when its parent IPowerConduit has power
 */
public class PoweredActivator : MonoBehaviour
{
    [Range(0f, 1f)]
    public float percentRequired = 1f;
    public bool allowTurnOff = true;

    [AutoInitialize(SearchParent =true),SerializeField, HideInInspector]
    private IPowerConduit conduit;

    void Start()
    {
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
        bool active = power >= maxPower * percentRequired;
        if ((active || allowTurnOff) && gameObject.activeSelf != active) {
            gameObject.SetActive(active);
        }
    }
}
