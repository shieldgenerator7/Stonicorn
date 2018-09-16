using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public MenuFrame startFrame;

    private List<MenuButton> buttons = new List<MenuButton>();

    private void Start()
    {
        foreach (MenuButton mb in FindObjectsOfType<MenuButton>())
        {
            buttons.Add(mb);
        }
        transform.position = GameManager.getPlayerObject().transform.position;
        startFrame.frameCamera();
    }

    public void processTapGesture(Vector3 pos)
    {
        foreach(MenuButton mb in buttons)
        {
            if (mb.tapInArea(pos))
            {
                mb.activate();
                return;
            }
        }
    }
}
