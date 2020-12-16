using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerWire : MonoBehaviour, IPowerTransferer, ICuttable
{
    public float throughPut;

    public float ThroughPut => throughPut;
    public GameObject GameObject => gameObject;

    private OnPowerFlowed onPowerFlowed;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerFlowed;
        set => onPowerFlowed = value;
    }

    private float energyThisFrame = 0;
    public void reset()
    {
        energyThisFrame = 0;
    }

    bool ICuttable.Cuttable => true;

    private void FixedUpdate()
    {
        reset();
    }

    public float transferPower(float power)
    {
        float energyLeftToTransfer = (ThroughPut * Time.fixedDeltaTime) - energyThisFrame;
        float energy = Mathf.Min(power, energyLeftToTransfer);
        energyThisFrame += energy;
        onPowerFlowed?.Invoke(energyThisFrame, ThroughPut * Time.fixedDeltaTime);
        return energy;
    }

    void ICuttable.cut(Vector2 start, Vector2 end)
    {
        Debug.Log("PowerWire " + name + " cut! " + start + ", " + end);
    }
}
