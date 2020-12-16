using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceLaunchProjectile : SavableMonoBehaviour
{

    public GameObject bouncinessIndicatorPrefab;//prefab
    private GameObject bouncinessIndicator;
    private Rigidbody2D rb2d;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        updateBouncingVisuals();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.isSolid())
        {
            Managers.Object.destroyObject(gameObject);
        }
    }

    private void Update()
    {
        updateBouncingVisuals();
    }

    void updateBouncingVisuals()
    {
        if (bouncinessIndicator == null)
        {
            bouncinessIndicator = Instantiate(bouncinessIndicatorPrefab);
            bouncinessIndicator.transform.parent = transform;
            bouncinessIndicator.transform.localPosition = Vector2.zero;
            SpriteRenderer bounceSR = bouncinessIndicator.GetComponent<SpriteRenderer>();
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            bounceSR.color = sr.color.adjustAlpha(bounceSR.color.a);
            //Fixes error when Force Launch not used before rewind in a session
            if (!rb2d)
            {
                rb2d = GetComponent<Rigidbody2D>();
            }
            bouncinessIndicator.SetActive(true);
        }
        bouncinessIndicator.transform.up = -rb2d.velocity;
    }

    public override bool IsSpawnedObject => true;
    public override string PrefabName => "Projectile_Merky";

    public override SavableObject CurrentState
    {
        get => new SavableObject(this);
        set { }
    }

}
