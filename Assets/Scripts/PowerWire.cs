using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerWire : SavableMonoBehaviour, IPowerTransferer, ICuttable
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
        onPowerFlowed?.Invoke(0, ThroughPut * Time.fixedDeltaTime);
    }

    bool ICuttable.Cuttable => true;

    public override SavableObject CurrentState { 
        get => new SavableObject(this);
        set { } 
    }

    public override void init()
    {
        //Autoset BoxCollider2D size
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        //TODO: get rid of this code, and/or move it to prebuild tasks (maybe?)
        if (sr)
        {
        GetComponent<BoxCollider2D>().size = sr.size;
        foreach (SpriteRenderer sr1 in GetComponentsInChildren<SpriteRenderer>())
        {
            sr1.size = sr.size;
        }
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Managers.Power.generateConnectionMap();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Managers.Power.generateConnectionMap();
    }
}
