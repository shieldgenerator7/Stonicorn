using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoorOpener : SavableMonoBehaviour, IPowerable
{
    public float maxEnergyPerSecond = 3;
    public float moveForce = 10;//magnitude
    private Vector2 moveVector;//direction

    public Collider2D moveColl;

    public float ThroughPut => maxEnergyPerSecond;
    public GameObject GameObject => gameObject;

    private OnPowerFlowed onPowerGiven;
    public OnPowerFlowed OnPowerFlowed
    {
        get => onPowerGiven;
        set => onPowerGiven = value;
    }
    public override SavableObject CurrentState
    {
        get => new SavableObject(this);
        set { }
    }

    private RaycastHit2D[] rch2dStartup = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    public override void init()
    {
        moveVector = transform.up;
        //Error checking
        if (!moveColl || !moveColl.isTrigger)
        {
            Debug.LogError("PoweredDoorOpener.moveColl requires a collider that is a trigger!", gameObject);
        }
    }

    public float acceptPower(float power)
    {
        float maxEnergy = maxEnergyPerSecond * Time.fixedDeltaTime;
        float energyToUse = Mathf.Min(power, maxEnergy);
        if (energyToUse > 0)
        {
            Vector3 forceVector = (energyToUse / maxEnergy) * moveForce * moveVector;
            //Push objects in zone
            int count = Utility.Cast(moveColl, Vector2.zero, rch2dStartup);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D rb2d = rch2dStartup[i].collider.gameObject.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    rb2d.linearVelocity = forceVector;
                }
            }
        }
        onPowerGiven?.Invoke(energyToUse, maxEnergy);
        return power - energyToUse;
    }
}
