using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressDisplayerUpgrades : ProgressDisplayer
{
    public List<string> abilityNames;
    public List<string> excludeNames;
    private List<PlayerAbility> abilities;

    protected override void init()
    {
        //get info
        abilities = abilityNames
            .ConvertAll(name => (PlayerAbility)Managers.Player.GetComponent(name));

        //update label
        base.init();
    }
    protected override int CurrentCount => abilities.Sum(ability => ((ability.Unlocked) ? 1 : 0) + ability.UpgradeLevel);

    protected override int MaxCount => abilities.Sum(ability => 1 + ability.upgradeLevels.Count);



    [Initializer]
    private List<string> init_abilityNames
        => FindObjectsByType<PlayerController>(FindObjectsSortMode.InstanceID)
            .First(pc => pc.CompareTag("Player"))
            .GetComponents<PlayerAbility>().ToList()
            .ConvertAll(pa => pa.GetType().Name)
            .FindAll(name => !excludeNames.Contains(name));

}
