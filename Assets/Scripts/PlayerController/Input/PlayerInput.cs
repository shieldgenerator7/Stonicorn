using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerInput
{
    //2019-04-08: moved here from GestureManager

    public enum InputState { Begin, Hold, End, None };
    public InputState inputState = InputState.None;

    //Original Positions
    public Vector3 origMP;//"original mouse position": the mouse position at the last mouse down (or tap down) event
    public Vector3 origMPWorld;//"original mouse position world" - the original mouse coordinate in the world
    public float origTime = 0f;//"original time": the clock time at the last mouse down (or tap down) event
    //Current Positions
    public Vector3 curMP;//"current mouse position"
    public Vector3 curMPWorld;//"current mouse position world" - the mouse coordinates in the world
    public float curTime = 0f;
    //Stats
    public float maxMouseMovement = 0f;//how far the mouse has moved since the last mouse down (or tap down) event
    public float holdTime = 0f;//how long the gesture has been held for    
  
    public class InputData
    {
        public Vector2 oldScreenPos;
        public Vector2 newScreenPos;
        public InputState inputState;
        public float holdTime;
        public float zoomMultiplier;

        public InputData(Vector2 oldScreenPos, Vector2 newScreenPos, InputState inputState, float holdTime, float zoomMultiplier = 1)
        {
            this.oldScreenPos = oldScreenPos;
            this.newScreenPos = newScreenPos;
            this.inputState = inputState;
            this.holdTime = holdTime;
            this.zoomMultiplier = zoomMultiplier;
        }

        /// <summary>
        /// Calculated when called, not stored
        /// </summary>
        public Vector2 OldWorldPos
        {
            get { return Camera.main.ScreenToWorldPoint(oldScreenPos); }
        }
        /// <summary>
        /// Calculated when called, not stored
        /// </summary>
        public Vector2 NewWorldPos
        {
            get { return Camera.main.ScreenToWorldPoint(newScreenPos); }
        }
        /// <summary>
        /// The distance between the oldScreenPos and the newScreenPos
        /// </summary>
        public float PositionDelta
        {
            get { return Vector2.Distance(oldScreenPos, newScreenPos); }
        }
    }

    public abstract InputData getInput();
}
