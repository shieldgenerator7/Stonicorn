using UnityEngine;

public class TeleportRangeFragment : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    public bool Active
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }
}
