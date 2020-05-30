using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subtypes of this class are things that can damage Merky
/// </summary>
public abstract class Hazard : SavableMonoBehaviour
{
    public virtual bool Hazardous => true;

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
        return new SavableObject(this);
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
    }
}
