using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static int MENU_SCENE_ID = 2;

    public MenuFrame startFrame;

    public List<MenuFrame> frames = new List<MenuFrame>();

    private void Awake()
    {
        GetComponent<Follow>().followObject = Managers.Player.gameObject;
    }

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
        Managers.Time.setPause(this, true);
    }

    private void OnDestroy()
    {
        Managers.Time.setPause(this, false);
    }//

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

    void Update()
    {
        //Orient camera to menu
        Managers.Camera.Up = Managers.Player.transform.up;
    }

    public static bool Open
    {
        get
        {
            //2021-01-20: copied from SceneLoader.IsLoaded
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).buildIndex == MENU_SCENE_ID)
                {
                    return true;
                }
            }
            return false;
        }
        set
        {
            //Don't open menu twice
            if (value == Open)
            {
                return;
            }
            //
            bool show = value;
            if (show)
            {
                LoadingScreen.LoadScene(MENU_SCENE_ID);
                //Pause
                if (LoadingScreen.FinishedLoading)
                {
                    onOpenedChanged?.Invoke(true);
                }
            }
            else
            {
                SceneManager.UnloadSceneAsync(MENU_SCENE_ID);
                onOpenedChanged?.Invoke(false);
            }
        }
    }
    public delegate void OnOpenedChanged(bool open);
    public static event OnOpenedChanged onOpenedChanged;
}
