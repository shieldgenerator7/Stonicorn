using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonResetGame : MenuActionButton
{
    public override void activate()
    {
        Managers.Game.resetGame(true);
    }
}
