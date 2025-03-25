using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class JointedUntilDisturbed : SavableMonoBehaviour, ISwappable
{
    [AutoInitialize, SerializeField, HideInInspector]
    private List<Joint2D> joints;

    [SerializeField]
    private bool jointed = true;
    public bool Jointed
    {
        get => jointed;
        set
        {
            jointed = value;
            joints.ForEach(joint => joint.enabled = jointed);
            onJointedChanged?.Invoke(jointed);
        }
    }
    public event Action<bool> onJointedChanged;

    private void Start()
    {
        init();
    }
    public override void init()
    {
        //Initialize state
        Jointed = jointed;
    }

    void ISwappable.nowSwapped()
    {
        Jointed = false;
    }

    static short key_jointed = 0;
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            key_jointed, jointed
            );
        set
        {
            Jointed = value.Bool(key_jointed);
        }
    }
}
