using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressDisplayerCompletion : ProgressDisplayer
{
    [Tooltip("Number of decimal places to show. 0 is int only")]
    public int precision = 1;
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
                ? Utility.cut(percent * 100, precision)
                : Mathf.RoundToInt(percent * 100);
            return $"{percentValue}%";
        }
    }

    public override float Percent
        => ((float)progressDisplayers.Sum(pd => pd.CurrentCount)) / (float)progressDisplayers.Sum(pd => pd.MaxCount);

    internal override int CurrentCount => throw new System.NotImplementedException();

    internal override int MaxCount => throw new System.NotImplementedException();

    protected override bool Shown => true;

    [Initializer]
    private List<ProgressDisplayer> init_progressDisplayers 
        => transform.parent.GetComponentsInChildren<ProgressDisplayer>()
            .Where(pd=>pd!=this).ToList();
}
