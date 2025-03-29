using UnityEngine;

/// <summary>
/// This script scoots the moon so merky doesnt get hit by the sun during the end credits
/// </summary>
public class MoonScooter : MonoBehaviour
{
    public Vector2 direction = Vector2.up;
    public float speed = 1;

    [AutoInitialize(SearchParent =true), SerializeField, HideInInspector]
    private Rigidbody2D rb2d;

    private void OnEnable()
    {
        rb2d.bodyType = RigidbodyType2D.Dynamic;
    }
    private void OnDisable()
    {
        rb2d.bodyType = RigidbodyType2D.Static;
    }

    // Update is called once per frame
    void Update()
    {
        rb2d.linearVelocity = direction.normalized * speed;
    }
}
