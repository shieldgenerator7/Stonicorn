using UnityEngine;
using UnityEngine.UI;

public class PupilEffect : MonoBehaviour
{
    public float maxDistanceFromCenter = 1;

    private Vector2 origPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 dir = (Vector2)transform.InverseTransformPoint( Utility.ScreenToWorldPoint(Input.mousePosition)) - origPos;
        transform.localPosition = dir.normalized * Mathf.Clamp(dir.magnitude, 0, maxDistanceFromCenter) + origPos;
    }
}
