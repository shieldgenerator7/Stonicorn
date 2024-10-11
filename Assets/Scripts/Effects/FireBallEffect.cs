using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallEffect : MonoBehaviour
{
    public ForceLaunchAbility forceLaunchAbility;

    private Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (!forceLaunchAbility)
        {
            Debug.LogError("FireBallEffect is missing ForceLaunchAbility!", gameObject);
        }
        sr.color = forceLaunchAbility.EffectColor.adjustAlpha(sr.color.a);
        rb2d = forceLaunchAbility.GetComponent<Rigidbody2D>();
        forceLaunchAbility.onAffectingVelocityChanged += onAbilityUsed;
        onAbilityUsed(forceLaunchAbility.AffectingVelocity);
    }

    private void OnDestroy()
    {
        forceLaunchAbility.onAffectingVelocityChanged -= onAbilityUsed;
    }

    private void onAbilityUsed(bool on)
    {
        gameObject.SetActive(on);
    }

    // Update is called once per frame
    void Update()
    {
        transform.up = -rb2d.linearVelocity;
    }
}
