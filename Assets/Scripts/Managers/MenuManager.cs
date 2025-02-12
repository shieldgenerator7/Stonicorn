using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour, ISetupable
{
    public static int MENU_SCENE_ID = 2;

    public MenuFrame startFrame;

    [SerializeField]
    private List<MenuFrame> frames;

    private MenuButton currentButton;

    private void Awake()
    {
        GetComponent<Follow>().followObject = Managers.Player.gameObject;
    }

    private void Start()
    {
        init();
    }

    void init()
    {
        //init frames
        frames.ForEach(frame => frame.init());
        //Lock on player
        GameObject player = Managers.Player.gameObject;
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
        Physics2D.SyncTransforms();
        //focus camera on start frame
        startFrame.frameCamera();
        //pause game
        Managers.Time.setPause(this, true);
    }

    private void OnDestroy()
    {
        Managers.Time.setPause(this, false);
    }

    public void processTapGesture(Vector3 pos) =>
        frames.FirstOrDefault(mf => mf.tapInArea(pos))?
            .delegateTap(pos);
    public bool processDragGesture(Vector3 origMPWorld, Vector3 newMPWorld, GestureState state)
    {
        //if (state == GestureState.START)
        //{
        //    currentButton = findButton(origMPWorld);
        //}
        return
        frames.FirstOrDefault(mf => mf.tapInArea(origMPWorld))?
            .delegateDrag(origMPWorld, newMPWorld, state)
            ?? false;
    }

    private MenuButton findButton(Vector2 tapPosWorld)
    {
        return frames.FirstOrDefault(mf => mf.tapInArea(tapPosWorld))?.findButton(tapPosWorld)
            ?? null;
    }

    internal void AddFrame(MenuFrame mf)
    {
        if (!frames.Contains(mf))
        {
            frames.Add(mf);
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

    public int setup()
    {
        int changeCount = 0;

        //populate frames
        int prevFrameCount = frames.Count;
        frames.Clear();
        frames = FindObjectsByType<MenuFrame>(FindObjectsSortMode.None)
            .Where(mf => mf.canDelegateTaps()).ToList();
        if (prevFrameCount != frames.Count)
        {
            changeCount++;
        }

        //set start frame
        if (!startFrame)
        {
            startFrame = frames.First();
            changeCount++;
        }

        //init menu buttons
        gameObject.GetComponentsInChildren<MenuFrame>().ToList().ForEach((mf) =>
        {
            changeCount += mf.setup();
        });

        return changeCount;
    }
}
