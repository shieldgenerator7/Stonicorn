using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonResetGame : MenuActionButton
{
    public override void activate()
    {
        GameManager.resetGame(true);
    }
}
