using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Skin : MonoBehaviour
{

    public static Skin attachedSkin;

    private Collider2D coll2d;

    private void Start()
    {
        coll2d = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
            if (attachedSkin != null)
            {
                attachedSkin.attach(false);
            }
            attach(true);
        }
    }

    void attach(bool attach = true)
    {
        if (attach)
        {
            transform.SetParent(Managers.Player.transform,false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            transform.SetParent(null, true);
        }
        coll2d.enabled = !attach;
    }
}
