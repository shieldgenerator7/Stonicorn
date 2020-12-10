using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ElectricRingController : MonoBehaviour
{
    [Header("Settings")]
    public float pointSpacing = 1;//distance between two points

    private SpriteShapeController ssc;
    private Spline spline;
    private ElectricRingAbility electricRingAbility;

    private void Start()
    {
        ssc = GetComponent<SpriteShapeController>();
        spline = ssc.spline;
        electricRingAbility = Managers.Player.GetComponent<ElectricRingAbility>();
        electricRingAbility.onEnergyChanged += energyChanged;
        gameObject.SetActive(false);
    }

    void energyChanged(float energy)
    {
        if (energy > 0)
        {
            gameObject.SetActive(true);
            generateGeometry(electricRingAbility.Range);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void generateGeometry(float range)
    {
        spline.Clear();
        float circumference = 2 * range * Mathf.PI;
        float spacing = pointSpacing;
        int pointCount = Mathf.RoundToInt(circumference / spacing);
        float angleSpacing = 2 * Mathf.PI / (float)pointCount;
        Vector2 startPos = Vector2.up * range;
        Vector2 placer = startPos;
        for (int i = 0; i < pointCount; i++)
        {
            spline.InsertPointAt(0, placer);
            placer = Utility.RotateZ(placer, angleSpacing);
        }
    }
}
