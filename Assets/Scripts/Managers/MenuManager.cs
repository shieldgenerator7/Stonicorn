using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static int MENU_SCENE_ID = 2;

    public MenuFrame startFrame;

    private IEnumerable<MenuFrame> frames;

    private void Awake()
    {
        GetComponent<Follow>().followObject = Managers.Player.gameObject;
    }

    private void Start()
    {
        frames = FindObjectsByType<MenuFrame>(FindObjectsSortMode.None)
            .Where(mf => mf.canDelegateTaps());
        GameObject player = Managers.Player.gameObject;
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
        startFrame.frameCamera();
        //init menu buttons
        gameObject.GetComponentsInChildren<MenuFrame>().ToList().ForEach((mf) => mf.init());
        //pause game
        Managers.Time.setPause(this, true);
    }

    private void OnDestroy()
    {
        Managers.Time.setPause(this, false);
    }

    public void processTapGesture(Vector3 pos) =>
        frames.First(mf => mf.tapInArea(pos))?
            .delegateTap(pos);
    public bool processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld) =>
        frames.First(mf => mf.tapInArea(origMPWorld))?
            .delegateDrag(origMPWorld, newMPWorld)
            ?? false;

    void Update()
    {
        //Orient camera to menu
        Managers.Camera.Up = Managers.Player.transform.up;
    }

    internal void AddFrame(MenuFrame mf)
    {
        if (!frames.Contains(mf))
        {
            frames = frames.Concat(new[] { mf });
        }
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
