using UnityEngine;
using UnityEngine.UI;

public class PupilEffect : MonoBehaviour
{
    public float minDistanceFromCenter = 0.1f;
    public float maxDistanceFromCenter = 1;

    public Transform origin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 dir = Utility.ScreenToWorldPoint(Input.mousePosition) - (Vector2)origin.position;
        transform.position = dir.normalized * Mathf.Clamp(dir.magnitude, minDistanceFromCenter, maxDistanceFromCenter) + (Vector2)origin.position;
    }
}
