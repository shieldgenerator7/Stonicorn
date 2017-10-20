using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Diagnostics;
using UnityEditor;

public class CustomMenu {
    
    [MenuItem("SG7/Save Game State %e")]
    public static void saveGameState()
    {
        Debug.Log("SAVED");
        GameManager.Save();
    }

    [MenuItem("SG7/Load Game State %#e")]
    public static void loadGameState()
    {
        Debug.Log("LOADED");
        GameManager.LoadState();
    }

    [MenuItem("SG7/Reload Game %#r")]
    public static void reloadGame()
    {
        GameManager.resetGame();
    }
}
