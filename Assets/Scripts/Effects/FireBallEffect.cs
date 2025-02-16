using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallEffect : MonoBehaviour, ISetupable
{
    public ForceLaunchAbility forceLaunchAbility;

    [AutoInitialize(Container = "forceLaunchAbility"),SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
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

#if UNITY_EDITOR
    public int checkForErrors()
    {
        int errors = 0;
        if (!gameObject.isPrefab())
        {
            if (!forceLaunchAbility)
            {
                Debug.LogError("FireBallEffect is missing ForceLaunchAbility!", gameObject);
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
            Color color = forceLaunchAbility.EffectColor.adjustAlpha(sr.color.a);
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
