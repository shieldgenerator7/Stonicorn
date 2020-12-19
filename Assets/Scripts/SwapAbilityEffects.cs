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
    }

    void onSwapRotate(GameObject player, GameObject target)
    {
        Managers.Effect.showSwapRotate(player, true);
        Managers.Effect.showSwapRotate(target, false);
    }
}
