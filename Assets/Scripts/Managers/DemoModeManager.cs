using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoModeManager : MonoBehaviour
{
    [Header("Demo Mode")]
    [SerializeField]
    private bool demoBuild = false;//true to not load on open or save with date/timestamp in filename
    public bool DemoMode
    {
        get => demoBuild;
        set => demoBuild = value;
    }
    [SerializeField]
    private float restartDemoDelay = 10;//how many seconds before the game can reset after the demo ends
    [SerializeField]
    private Text txtDemoTimer;//the text that shows much time is left in the demo
    [SerializeField]
    private GameObject endDemoScreen;//the picture to show the player after the game resets


    //
    // Runtime variables
    //
    private float resetGameTimer;//the time that the game will reset at
    private float gamePlayTime;//how long the game can be played for, 0 for indefinitely


    public void init()
    {
        //If a limit has been set on the demo playtime,
        if (GameDemoLength > 0)
        {
            //Auto-enable demo mode
            demoBuild = true;
            //Tell the gesture manager to start the timer when the player taps in game
            //Managers.Gesture.tapGesture += startDemoTimer;
            //Show the timer
            txtDemoTimer.transform.parent.gameObject.SetActive(true);
        }
    }

    public void processDemoMode()
    {
        float timeLeft = 0;
        //And the timer has started,
        if (resetGameTimer > 0)
        {
            //If the timer has stopped,
            if (Time.time >= resetGameTimer)
            {
                //Show the end demo screen
                showEndDemoScreen(true);
                //If the ignore-input buffer period has ended,
                if (Time.time >= resetGameTimer + restartDemoDelay)
                {
                    //And user has given input,
                    if (Input.GetMouseButton(0)
                        || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                        )
                    {
                        //Reset game
                        showEndDemoScreen(false);
                        Managers.Game.resetGame();
                    }
                }
            }
            //Else if the timer is ticking,
            else
            {
                //Show the time remaining
                timeLeft = resetGameTimer - Time.time;
            }
        }
        //Else if the timer has not started,
        else
        {
            //Show the max play time of the demo
            timeLeft = GameDemoLength;
        }
        //Update the timer on screen
        txtDemoTimer.text = string.Format("{0:0.00}", timeLeft);
    }

    #region Demo Mode Methods
    /// <summary>
    /// How long the demo lasts, in seconds
    /// 0 to have no time limit
    /// </summary>
    public float GameDemoLength
    {
        get { return gamePlayTime; }
        set { gamePlayTime = Mathf.Max(value, 0); }
    }

    /// <summary>
    /// Start the demo timer
    /// </summary>
    void startDemoTimer()
    {
        //If the menu is not open,
        if (Managers.Camera.ZoomLevel > Managers.Camera.toZoomLevel(CameraController.CameraScalePoints.PORTRAIT))
        {
            //Start the timer
            resetGameTimer = GameDemoLength + Time.time;
            //Unregister this delegate
            //Managers.Gesture.tapGesture -= startDemoTimer;
        }
    }

    /// <summary>
    /// Shows the "Thanks for Playing" screen when the demo timer stops
    /// </summary>
    /// <param name="show">True to show the screen, false to hide it</param>
    private void showEndDemoScreen(bool show)
    {
        //Update the screen's active state
        endDemoScreen.SetActive(show);
        //If it should be shown,
        if (show)
        {
            //Also update its position and rotation
            //to keep it in front of the camera
            endDemoScreen.transform.position = (Vector2)Camera.main.transform.position;
            endDemoScreen.transform.localRotation = Camera.main.transform.localRotation;
        }
    }
    #endregion
}
