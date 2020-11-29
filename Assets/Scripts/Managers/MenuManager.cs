using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public MenuFrame startFrame;

    public List<MenuFrame> frames = new List<MenuFrame>();

    private void Start()
    {
        foreach (MenuFrame mf in FindObjectsOfType<MenuFrame>())
        {
            if (mf.canDelegateTaps())
            {
                frames.Add(mf);
            }
        }
        GameObject player = Managers.Player.gameObject;
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
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
    public bool processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld)
    {
        foreach (MenuFrame mf in frames)
        {
            if (mf.tapInArea(origMPWorld))
            {
                if (mf.delegateDrag(origMPWorld, newMPWorld))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void opened()
    {
        //Orient camera to menu
        Managers.Camera.Up = Managers.Player.transform.up;
    }

    public static bool Open
    {
        get { return Managers.Menu != null; }
        set
        {
            bool show = value;
            if (show)
            {
                LoadingScreen.LoadScene("MainMenu");
                //Pause
                if (LoadingScreen.FinishedLoading)
                {
                    //Register pause with GameManager because MenuManager probably isn't loaded yet
                    Managers.Time.setPause(Managers.Game, true);
                }
            }
            else
            {
                SceneManager.UnloadSceneAsync("MainMenu");
                //Unpause
                Managers.Time.setPause(Managers.Game, false);
            }
        }
    }
}
