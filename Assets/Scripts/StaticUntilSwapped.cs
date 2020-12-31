using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticUntilSwapped : SavableMonoBehaviour, ISwappable
{
    [SerializeField]
    private bool rooted = true;
    public bool Rooted
    {
        get => rooted;
        set
        {
            rooted = value;
            GetComponent<Rigidbody2D>().bodyType = (rooted)
                ? RigidbodyType2D.Static
                : RigidbodyType2D.Dynamic;
            onRootedChanged?.Invoke(rooted);
        }
    }
    public delegate void OnRootedChanged(bool rooted);
    public event OnRootedChanged onRootedChanged;

    private void Start()
    {
        init();
    }
    public override void init()
    {
        //Initialize state
        Rooted = rooted;
    }

    void ISwappable.nowSwapped()
    {
        Rooted = false;
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "rooted", rooted
            );
        set
        {
            Rooted = value.Bool("rooted");
        }
    }
}
