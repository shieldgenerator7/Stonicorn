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

    [AutoInitialize(SearchChildren =true,AllowUnfound =true), SerializeField, HideInInspector]
    private SpriteRenderer sr;
    [AutoInitialize, SerializeField, HideInInspector]
    private MenuActionSwitch mas;

    public override void init()
    {
        base.init();
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

    public override int setup()
    {
        int changeCount = base.setup();
        if (sr)
        {
            if (activatedSprite == null && deactivatedSprite == null)
            {
                if (!useColor)
                {
                    useColor = true;
                    changeCount++;
                }
            }
            if (activatedSprite == null)
            {
                activatedSprite = sr.sprite;
                changeCount++;
            }
            if (deactivatedSprite == null)
            {
                deactivatedSprite = sr.sprite;
                changeCount++;
            }
            if (useColor)
            {
                if (activeColor == deactiveColor)
                {
                    activeColor = sr.color;
                    deactiveColor = (sr.color == Color.white) ? Color.black : Color.white;
                    changeCount++;
                }
            }
        }

        return changeCount;
    }
}
