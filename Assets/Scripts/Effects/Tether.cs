using UnityEngine;

/// <summary>
/// When you want to have effects when an object leaves an area, use this
/// 2025-01-24: This works like Astalir's spell tether
/// </summary>
public class Tether : MonoBehaviour
{
    [Tooltip("The transform to be close to")]
    public Transform origin;
    public float range;
    public bool whenLeaveRangeDeactivate = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (origin == null)
        {
            origin = transform.parent;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (whenLeaveRangeDeactivate)
        {
            if (Vector2.Distance(transform.position, origin.position) > range)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
