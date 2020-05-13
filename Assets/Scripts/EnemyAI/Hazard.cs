using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subtypes of this class are things that can damage Merky
/// </summary>
public abstract class Hazard : SavableMonoBehaviour
{
    [SerializeField]
    private bool hazardous = true;
    public bool Hazardous
    {
        get => hazardous;
        protected set
        {
            hazardous = value;
        }
    }

    [SerializeField]
    private int damageDealt = 2;
    public int DamageDealt
    {
        get => damageDealt;
        private set
        {
            damageDealt = value;
        }
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this, "hazardous", hazardous);
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        Hazardous = (bool)savObj.data["hazardous"];
    }
}
