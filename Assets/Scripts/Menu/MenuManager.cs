using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public MenuFrame startFrame;

    private List<MenuFrame> frames = new List<MenuFrame>();

    private void Start()
    {
        foreach (MenuFrame mf in FindObjectsOfType<MenuFrame>())
        {
            frames.Add(mf);
        }
        transform.position = GameManager.getPlayerObject().transform.position;
        startFrame.frameCamera();
    }

    public void processTapGesture(Vector3 pos)
    {
        foreach (MenuFrame mf in frames)
        {
            if (mf.tapInArea(pos))
            {
                mf.delegateTap(pos);
                return;
            }
        }
    }
}
