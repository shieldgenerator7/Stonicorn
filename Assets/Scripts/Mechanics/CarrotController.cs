using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarrotController : MonoBehaviour
{
    public float moveForce = 3;

    public bool glowing = false;

    public Skin skin;
    public float skinUpDistance = 5;

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
        if (!skin || skin.gameObject.activeSelf)
        {
            AudioSource.PlayClipAtPoint(boingOn, transform.position);
            return;
        }
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
        List<CarrotController> carrots = FindObjectsByType<CarrotController>(FindObjectsSortMode.None).ToList();
        if (carrots.All(cc => cc.glowing))
        {
            skin.transform.position = transform.position + transform.up * skinUpDistance;
            skin.transform.up = transform.up;
            skin.gameObject.SetActive(true);

            carrots.ForEach(cc =>
            {
                cc.Glowing = false;
            });
        }
    }

    private bool Glowing
    {
        get => glowing;
        set
        {
            glowing = value;
            glowEffect?.SetActive(glowing);
        }
    }
}
