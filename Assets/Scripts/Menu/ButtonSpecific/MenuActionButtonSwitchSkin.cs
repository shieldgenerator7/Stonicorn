using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonSwitchSkin : MenuActionButton
{
    public int direction = 1;
    public MenuButtonSlide slider;

    public override void activate()
    {
        Managers.Skin.nextSkin(direction);
        slider.updateSlider();
    }
}
