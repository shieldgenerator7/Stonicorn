using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerWire : SavableMonoBehaviour, IPowerTransferer, ICuttable, ISetupable
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

    public override SavableObject CurrentState
    {
        get => new SavableObject(this);
        set { }
    }

    [AutoInitialize, SerializeField, HideInInspector]
    private Collider2D coll2d;
    public Collider2D Collider2D => coll2d;

    public override void init()
    {
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

    public int setup()
    {
        int changeCount = 0;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr)
        {
            Vector2 size = sr.size;
            //Autoset BoxCollider2D size
            BoxCollider2D bc2d = GetComponent<BoxCollider2D>();
            if (bc2d.size != size)
            {
                bc2d.size = size;
                changeCount++;
            }
            foreach (SpriteRenderer sr1 in GetComponentsInChildren<SpriteRenderer>())
            {
                if (sr1.size != size)
                {
                sr1.size = size;
                    changeCount++;
                }
            }
        }

        return changeCount;
    }
}
