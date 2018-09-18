using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonSwitch : MenuButton {

    public Sprite activatedSprite;
    public Sprite deactivatedSprite;

    public bool active = true;

    private SpriteRenderer sr;
    private MenuActionSwitch mas;

    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
        mas = GetComponent<MenuActionSwitch>();
        updateSprite();
    }

    public override void activate()
    {
        active = !active;
        mas.doAction(active);
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
