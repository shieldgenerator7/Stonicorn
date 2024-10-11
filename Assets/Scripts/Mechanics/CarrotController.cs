using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarrotController : MonoBehaviour
{
    public float moveForce = 3;

    public bool glowing = false;
    public Color costumeColor = Color.white;

    public GameObject glowEffect;

    public AudioClip boingOn;
    public AudioClip boingOff;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.isSolid())
        {
            launchObject(collision.rigidbody);
            toggleEffect();
        }
    }

    private void launchObject(Rigidbody2D rb2d)
    {
        rb2d.linearVelocity = transform.up * moveForce;
    }

    private void toggleEffect()
    {
        glowing = !glowing;
        glowEffect.SetActive(glowing);
        if (glowing)
        {
            AudioSource.PlayClipAtPoint(boingOn, transform.position);
        }
        else
        {
            AudioSource.PlayClipAtPoint(boingOff, transform.position);
        }
        //If all carrots are glowing, change Merky's color
        if (FindObjectsByType<CarrotController>(FindObjectsSortMode.None).ToList()
            .All(cc => cc.glowing))
        {
            Managers.Player.GetComponent<SpriteRenderer>().color = costumeColor;
        }
        else if (FindObjectsByType<CarrotController>(FindObjectsSortMode.None).ToList()
            .All(cc => !cc.glowing))
        {
            Managers.Player.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
