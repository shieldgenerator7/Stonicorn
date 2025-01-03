using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class JointedUntilDisturbed : SavableMonoBehaviour, ISwappable
{

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
        joints = GetComponents<Joint2D>().ToList();
        Jointed = jointed;
    }

    void ISwappable.nowSwapped()
    {
        Jointed = false;
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "jointed", jointed
            );
        set
        {
            Jointed = value.Bool("jointed");
        }
    }
}
