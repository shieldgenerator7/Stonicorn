using UnityEngine;
using UnityEngine.UI;

public class PupilEffect : MonoBehaviour
{
    public float minDistanceFromCenter = 0.1f;
    public float maxDistanceFromCenter = 1;

    [SerializeField,HideInInspector]
    private Vector2 originalScale;

    public Transform origin;


    // Update is called once per frame
    void Update()
    {
        float scaleFactor = origin.localScale.magnitude/originalScale.magnitude;
        Vector2 dir = Utility.ScreenToWorldPoint(Input.mousePosition) - (Vector2)origin.position;
        transform.position = dir.normalized * Mathf.Clamp(
            dir.magnitude,
            minDistanceFromCenter*scaleFactor, 
            maxDistanceFromCenter*scaleFactor
            ) + (Vector2)origin.position;
    }

    [Initializer]
    private Vector2 init_originalScale=> origin.localScale;
}
