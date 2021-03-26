using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ElectricRingDisplayer : MonoBehaviour
{
    [Header("Settings")]
    public float pointSpacing = 1;//distance between two points

    private Spline spline;
    private ElectricBeamAbility electricBeamAbility;

    private void Start()
    {
        spline = GetComponent<SpriteShapeController>().spline;
        electricBeamAbility = Managers.Player.GetComponent<ElectricBeamAbility>();
        electricBeamAbility.onTargetChanged += targetChanged;
        targetChanged(null, electricBeamAbility.Target);
    }

    void targetChanged(GameObject oldGO, GameObject newGO)
    {
        if (!newGO && electricBeamAbility.Activated)
        {
            gameObject.SetActive(true);
            generateGeometry(electricBeamAbility.range - 0.2f);
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
