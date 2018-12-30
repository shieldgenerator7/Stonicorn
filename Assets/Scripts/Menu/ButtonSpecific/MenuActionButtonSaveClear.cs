using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonSaveClear : MenuActionButton
{
    public override void activate()
    {
        if (ES2.Exists("merky.txt")){
            ES2.Delete("merky.txt");
        }
        GameManager.resetGame(false);
    }
}
