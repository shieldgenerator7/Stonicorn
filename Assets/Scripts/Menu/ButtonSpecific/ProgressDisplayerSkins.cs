using UnityEngine;

public class ProgressDisplayerSkins : ProgressDisplayer
{
    protected override int CurrentCount => Managers.Skin.FoundSkinCount;

    protected override int MaxCount => Managers.Skin.KnownSkinCount;
}
