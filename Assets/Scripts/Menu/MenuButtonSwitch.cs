using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonSwitch : MenuButton {

    public Sprite activatedSprite;
    public Sprite deactivatedSprite;

    public bool active = true;

    private SpriteRenderer sr;

    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
        updateSprite();
    }

    public override void activate()
    {
        active = !active;
        updateSprite();
    }

    private void updateSprite()
    {
        if (active)
        {
            sr.sprite = activatedSprite;
        }
        else
        {
            sr.sprite = deactivatedSprite;
        }
    }
}
