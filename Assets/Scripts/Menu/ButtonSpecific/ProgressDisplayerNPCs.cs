using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressDisplayerNPCs : ProgressDisplayer
{
    [Tooltip("When you talk to an NPC for the first time, a variable will be updated. Enter the name of that variable into this list for each NPC.")]
    public List<string> npcVariableNames;

    internal override int CurrentCount => npcVariableNames.Count(name => Managers.Progress.get(name) > 0);

    internal override int MaxCount => npcVariableNames.Count;

    [Initializer]
    private List<string> init_npcVariableNames 
        => FindObjectsByType<Character>(FindObjectsSortMode.InstanceID).ToList()
            .ConvertAll(chr => $"{chr.characterName.ToLower()}intro")
            .Distinct().ToList();
}
