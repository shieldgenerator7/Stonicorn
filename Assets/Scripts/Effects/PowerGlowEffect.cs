using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Creates a glow effect when the IPowerConduit in its parent object has power
/// </summary>
public class PowerGlowEffect : MonoBehaviour
{
    public Color effectColor = Color.white;

    private IPowerConduit conduit;
    private SpriteRenderer lightEffectRenderer;
    private SpriteShapeRenderer spriteShapeRenderer;
    private TMP_Text text;

    private Color startColor = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        lightEffectRenderer = GetComponent<SpriteRenderer>();
        spriteShapeRenderer = GetComponent<SpriteShapeRenderer>();
        text = GetComponent<TMP_Text>();
        conduit = GetComponent<IPowerConduit>() ?? GetComponentInParent<IPowerConduit>();
        conduit.OnPowerFlowed += onPowerFlowed;

        if (spriteShapeRenderer)
        {
            startColor = spriteShapeRenderer.color;
        }
    }

    private void OnDestroy()
    {
        if (conduit != null)
        {
            conduit.OnPowerFlowed -= onPowerFlowed;
        }
    }

    void onPowerFlowed(float power, float maxPower)
    {
        //Update the visuals
        //2017-01-24: copied from my project: https://github.com/shieldgenerator7/GGJ-2017-Wave/blob/master/Assets/Script/CatTongueController.cs
        float newHigh = 1.0f;//opaque
        float newLow = 0.0f;//transparent
        float curHigh = maxPower;
        float curLow = 0;
        float newAlpha = ((power - curLow) * (newHigh - newLow) / (curHigh - curLow)) + newLow;

        if (lightEffectRenderer)
        {
            lightEffectRenderer.color = lightEffectRenderer.color.adjustAlpha(newAlpha);
        }
        if (spriteShapeRenderer)
        {
            Color color = new Color(0, 0, 0, 0);
            float percent = Mathf.Clamp(power / maxPower, 0, 1);
            color.r = Mathf.Lerp(startColor.r, effectColor.r, percent);
            color.g = Mathf.Lerp(startColor.g, effectColor.g, percent);
            color.b = Mathf.Lerp(startColor.b, effectColor.b, percent);
            color.a = Mathf.Lerp(startColor.a, effectColor.a, percent);
            spriteShapeRenderer.color = color;
        }
        if (text)
        {
            text.color = text.color.adjustAlpha(newAlpha);
        }
    }

}
