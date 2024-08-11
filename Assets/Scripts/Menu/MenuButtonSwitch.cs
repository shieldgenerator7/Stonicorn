using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonSwitch : MenuButton
{

    public Sprite activatedSprite;
    public Sprite deactivatedSprite;
    public bool useColor = false;
    public Color activeColor = Color.white;
    public Color deactiveColor = Color.white;

    private bool active = true;

    [SerializeField]
    private SpriteRenderer sr;
    [SerializeField]
    private MenuActionSwitch mas;

    public override void init()
    {
        base.init();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
        if (sr)
        {
            if (activatedSprite == null && deactivatedSprite == null)
            {
                useColor = true;
            }
            if (activatedSprite == null)
            {
                activatedSprite = sr.sprite;
            }
            if (deactivatedSprite == null)
            {
                deactivatedSprite = sr.sprite;
            }
            if (useColor)
            {
                if (activeColor == deactiveColor)
                {
                    if (sr.color == Color.white)
                    {
                        activeColor = sr.color;
                        deactiveColor = Color.black;
                    }
                    else
                    {
                        activeColor = sr.color;
                        deactiveColor = Color.white;
                    }
                }
            }
        }
        mas = GetComponent<MenuActionSwitch>();

    }

    private void Start()//TODO: call this in true init method
    {
        active = mas.getActiveState();
        updateSprite();
    }

    public override void activate()
    {
        active = !active;
        mas.doAction(active);
        active = mas.getActiveState();
        updateSprite();
    }

    private void updateSprite()
    {
        if (sr)
        {
            if (active)
            {
                sr.sprite = activatedSprite;
            }
            else
            {
                sr.sprite = deactivatedSprite;
            }
            if (useColor)
            {
                if (active)
                {
                    sr.color = activeColor;
                }
                else
                {
                    sr.color = deactiveColor;
                }
            }
        }
    }
}
