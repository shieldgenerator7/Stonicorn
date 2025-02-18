using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SavableMonoBehaviour : MonoBehaviour
{
    [AutoInitialize, SerializeField, HideInInspector]
    private SavableObjectInfo soi;
    public SavableObjectInfo SavableObjectInfo => soi;

    public abstract void init();

    /// <summary>
    /// The SavableObject that contains this object's configuration state
    /// </summary>
    public abstract SavableObject CurrentState { get; set; }

    /// <summary>
    /// True if this script was spawned during runtime
    /// If a subtype overrides this to true, make sure to call register() within it
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSpawnedScript => false;

    /// <summary>
    /// If a subscript overrides IsSpawnedScript, they need to call this method when they get created and destroyed
    /// </summary>
    /// <param name="register"></param>
    protected void register(bool register = true)
    {
        //have to call GetComponent() here bc the added script might not have it set
        soi ??= GetComponent<SavableObjectInfo>();
        //
        if (register)
        {
            if (!soi.savables.Contains(this))
            {
                soi.savables.Add(this);
            }
        }
        else
        {
            soi.savables.Remove(this);
        }
    }
}
