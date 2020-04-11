using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subtypes of this class are things that can damage Merky
/// </summary>
public abstract class Hazard : MonoBehaviour
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
        protected set
        {
            damageDealt = value;
        }
    }
}
