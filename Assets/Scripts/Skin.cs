using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Skin : MonoBehaviour
{
    public GameObject outline;

    public static Skin attachedSkin;

    private Collider2D coll2d;

    private bool canBePickedUp = true;

    private void Start()
    {
        coll2d = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBePickedUp && collision.isPlayerSolid())
        {
            if (attachedSkin != null)
            {
                attachedSkin.attach(false);
            }
            attach(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
            canBePickedUp = true;
        }
    }

    void attach(bool attach = true)
    {
        if (attach)
        {
            transform.SetParent(Managers.Player.transform,false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            attachedSkin = this;
            canBePickedUp = false;
        }
        else
        {
            transform.SetParent(null, true);
            if (attachedSkin == this)
            {
                attachedSkin = null;
            }
            canBePickedUp = false;
        }
        outline.SetActive(!attach);
        coll2d.enabled = !attach;
    }
}
