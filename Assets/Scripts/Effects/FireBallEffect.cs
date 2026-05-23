using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallEffect : MonoBehaviour, ISetupable
{
    public FlameDashAbility flameDashAbility;

    [AutoInitialize(Container = "flameDashAbility"),SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        flameDashAbility.onAffectingVelocityChanged += onAbilityUsed;
        onAbilityUsed(flameDashAbility.AffectingVelocity);
    }

    private void OnDestroy()
    {
        flameDashAbility.onAffectingVelocityChanged -= onAbilityUsed;
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

#if UNITY_EDITOR
    public int checkForErrors()
    {
        int errors = 0;
        if (!gameObject.isPrefab())
        {
            if (!flameDashAbility)
            {
                Debug.LogError("FireBallEffect is missing FlameDashAbility!", gameObject);
                errors++;
            }
        }
        return errors;
    }
    public int setup()
    {
        int changeCount = 0;
        if (!gameObject.isPrefab())
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color color = flameDashAbility.EffectColor.adjustAlpha(sr.color.a);
            if (sr.color != color)
            {
                sr.color = color;
                changeCount++;
            }
        }
        return changeCount;
    }
#endif
}
