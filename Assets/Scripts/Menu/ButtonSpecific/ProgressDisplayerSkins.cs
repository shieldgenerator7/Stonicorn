using UnityEngine;

public class ProgressDisplayerSkins : ProgressDisplayer
{
    internal override int CurrentCount => Managers.Skin.FoundSkinCount;

    internal override int MaxCount => Managers.Skin.KnownSkinCount;

    protected override bool Shown => CurrentCount > 1;
}
