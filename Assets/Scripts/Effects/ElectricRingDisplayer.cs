using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ElectricRingDisplayer : MonoBehaviour, ISetupable
{
    [Header("Settings")]
    public float pointSpacing = 1;//distance between two points
    public float rangeOffset = -0.2f;

    [SerializeField,HideInInspector]
    private SpriteShapeController ssc;
    [SerializeField]
    private ElectricBeamAbility electricBeamAbility;
    public MonoBehaviour fullChargeEffect;

    private void Start()
    {
        electricBeamAbility.onActivatedChanged += updateOn;
        electricBeamAbility.onRangeChanged += updateRange;
        electricBeamAbility.onChargeChanged += updateCharge;
        updateOn(electricBeamAbility.Activated);
    }

    void updateOn(bool active)
    {
        if (active)
        {
            gameObject.SetActive(true);
            generateGeometry();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void updateRange(float range)
    {
        generateGeometry(range + rangeOffset, electricBeamAbility.Charge / electricBeamAbility.maxCharge);
    }

    void updateCharge(float charge)
    {
        float percent = charge / electricBeamAbility.maxCharge;
        generateGeometry(electricBeamAbility.range + rangeOffset, percent);
        bool fullCharge = (percent == 1);
        if (fullChargeEffect)
        {
            fullChargeEffect.enabled = fullCharge;
        }
    }

    void generateGeometry()
    {
        generateGeometry(electricBeamAbility.range + rangeOffset, electricBeamAbility.Charge / electricBeamAbility.maxCharge);
    }

    void generateGeometry(float range, float percent)
    {
        Spline spline = ssc.spline;
        spline.Clear();
        float arc = 2 * Mathf.PI * percent;
        float circumference = range * arc;
        float spacing = circumference / Mathf.Ceil(circumference / pointSpacing);
        int pointCount = Mathf.FloorToInt(circumference / spacing);
        float angleSpacing = arc / pointCount;
        Vector2 startPos = Vector2.up * range;
        Vector2 placer = startPos;
        Mathf.RoundToInt(pointCount);
        for (int i = 0; i <= pointCount; i++)
        {
            spline.InsertPointAt(0, placer);
            placer = Utility.RotateZ(placer, angleSpacing);
        }
    }

    public int checkForErrors()
    {
        int errors = 0;
        if (!gameObject.isPrefab())
        {
            if (electricBeamAbility == null)
            {
                Debug.LogError("ElectricRingDisplayer needs an EelectricBeamAbility!", this);
                errors++;
            }
        }
        return errors;
    }

    public int setup()
    {
        return 0;
    }
}
