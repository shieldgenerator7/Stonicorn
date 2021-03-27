using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarrotController : MonoBehaviour
{
    public float moveForce = 3;

    public List<Color> colors = new List<Color>() { Color.white };

    public Color Color => sr.color;

    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.isSolid())
        {
            launchObject(collision.rigidbody);
            changeColor();
        }
    }

    private void launchObject(Rigidbody2D rb2d)
    {
        rb2d.velocity = transform.up * moveForce;
    }

    private void changeColor()
    {
        int index = colors.IndexOf(sr.color);
        index = (index + colors.Count + 1) % colors.Count;
        sr.color = colors[index];
        //If all carrots same color, change Merky's color
        Color color = Color;
        if (FindObjectsOfType<CarrotController>().ToList()
            .All(cc => cc.Color == color)
            )
        {
            Managers.Player.GetComponent<SpriteRenderer>().color = color;
        }
    }
}
