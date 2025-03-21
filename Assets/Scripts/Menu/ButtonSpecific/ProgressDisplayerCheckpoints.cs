using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressDisplayerCheckpoints : ProgressDisplayer
{
    public List<CheckPointChecker> checkpoints;


    protected override void init()
    {
        checkpoints = FindObjectsByType<CheckPointChecker>(FindObjectsSortMode.None).ToList();
        base.init();
    }


    protected override int CurrentCount => checkpoints.Count(cp=>cp.Discovered);

    protected override int MaxCount => checkpoints.Count;

}
