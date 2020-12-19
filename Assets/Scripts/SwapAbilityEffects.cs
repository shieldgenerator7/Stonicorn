using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapAbilityEffects : MonoBehaviour
{
    public SwapAbility swapAbility;

    // Start is called before the first frame update
    void Start()
    {
        swapAbility.onRotate += onSwapRotate;
        swapAbility.onStasis += onSwapStasis;
    }

    void onSwapRotate(GameObject player, GameObject target)
    {
        Managers.Effect.showSwapRotate(player, true);
        Managers.Effect.showSwapRotate(target, false);
    }

    void onSwapStasis(GameObject player, GameObject target)
    {
        Managers.Effect.showSwapStasis(target, true);
        target.GetComponent<StaticUntilTouched>().onRootedChanged +=
            (rooted) =>
            {
                if (!rooted)
                {
                    Managers.Effect.showSwapStasis(target, false);
                }
            };
    }
}
