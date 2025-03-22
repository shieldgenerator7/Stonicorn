using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressDisplayerCheckpoints : ProgressDisplayer
{
    public List<CheckPointChecker> checkpoints;


    internal override void init()
    {
        checkpoints = FindObjectsByType<CheckPointChecker>(FindObjectsSortMode.None).ToList();
        base.init();
    }


    internal override int CurrentCount => checkpoints.Count(cp=>cp.Discovered);

    internal override int MaxCount => checkpoints.Count;

    protected override bool Shown => CurrentCount > 1;

}
