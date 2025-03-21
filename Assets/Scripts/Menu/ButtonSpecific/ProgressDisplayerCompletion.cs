using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressDisplayerCompletion : ProgressDisplayer
{
    public List<ProgressDisplayer> progressDisplayers;

    internal override void init()
    {
        progressDisplayers.ForEach(pd => pd.init());
        base.init();
    }

    protected override string Progress
    {
        get
        {
            float percent = Percent;
            float percentValue = (percent < 1)
                ? Utility.cut(percent * 100, 1)
                : Mathf.RoundToInt(percent * 100);
            return $"{percentValue}%";
        }
    }

    public override float Percent
        => ((float)progressDisplayers.Sum(pd => pd.Percent)) / (float)progressDisplayers.Count;

    protected override int CurrentCount => throw new System.NotImplementedException();

    protected override int MaxCount => throw new System.NotImplementedException();

    [Initializer]
    private List<ProgressDisplayer> init_progressDisplayers => FindObjectsByType<ProgressDisplayer>(FindObjectsSortMode.InstanceID)
        .Where(pd=>pd!=this).ToList();
}
