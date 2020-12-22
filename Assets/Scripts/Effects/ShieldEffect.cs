using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    public LongTeleportAbility longTeleportAbility;
    public GameObject shieldPrefab;

    private GameObject shieldEffect;

    // Start is called before the first frame update
    void Start()
    {
        longTeleportAbility.onShieldedChanged += onShieldedChanged;
    }

    void onShieldedChanged(bool shielded)
    {
        if (shielded)
        {
            if (!shieldEffect)
            {
                shieldEffect = Instantiate(shieldPrefab);
                shieldEffect.transform.parent = transform;
                shieldEffect.transform.localPosition = Vector3.zero;
                shieldEffect.transform.up = transform.up;
                SpriteRenderer sr = shieldEffect.GetComponent<SpriteRenderer>();
                sr.color = longTeleportAbility.EffectColor.adjustAlpha(sr.color.a);
            }
        }
        shieldEffect?.SetActive(shielded);
    }
}
