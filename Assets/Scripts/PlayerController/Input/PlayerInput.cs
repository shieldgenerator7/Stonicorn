using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerInput
{
    //2019-04-08: moved here from GestureManager

    public enum InputState { Begin, Hold, End, None };
    private InputState inputStateVar = InputState.None;
    public InputState inputState
    {
        get { return inputStateVar; }
        set
        {
            inputStateVar = value;
            if (inputStateVar == InputState.Begin)
            {
                inputData = new InputData();
            }
        }
    }
    protected InputData inputData = new InputData();


    public class InputData
    {
        //World Pos
        private Vector2 oldWorldPos;
        private Vector2 newWorldPos;
        //Time
        private float oldTime;
        private float newTime;
        //State
        public InputState inputState;
        public float zoomMultiplier;

        public InputData()
        {
            this.oldWorldPos = Vector2.zero;
            this.newWorldPos = Vector2.zero;
            this.inputState = InputState.None;
            this.zoomMultiplier = 1;
        }

        public void setWorldPos(Vector2 oldWorldPos, Vector2 newWorldPos)
        {
            this.oldWorldPos = oldWorldPos;
            this.newWorldPos = newWorldPos;
        }
        public void setScreenPos(Vector2 oldScreenPos, Vector2 newScreenPos)
        {
            this.oldWorldPos = Camera.main.ScreenToWorldPoint(oldScreenPos);
            this.newWorldPos = Camera.main.ScreenToWorldPoint(newScreenPos);
        }
        public void setState(InputState inputState, float zoomMultiplier = 1)
        {
            this.inputState = inputState;
            this.zoomMultiplier = zoomMultiplier;
            updateTime();
        }

        /// <summary>
        /// Calculated when called, not stored
        /// </summary>
        public Vector2 OldWorldPos
        {
            get { return oldWorldPos; }
            set { oldWorldPos = value; }
        }
        /// <summary>
        /// Calculated when called, not stored
        /// </summary>
        public Vector2 NewWorldPos
        {
            get { return newWorldPos; }
            set { newWorldPos = value; }
        }
        public Vector2 OldScreenPos
        {
            set { this.oldWorldPos = Camera.main.ScreenToWorldPoint(value); }
        }
        public Vector2 NewScreenPos
        {
            set { this.newWorldPos = Camera.main.ScreenToWorldPoint(value); }
        }
        /// <summary>
        /// The distance between the oldScreenPos and the newScreenPos
        /// </summary>
        public float PositionDelta
        {
            get
            {
                return Vector2.Distance(
                    Camera.main.WorldToScreenPoint(oldWorldPos),
                    Camera.main.WorldToScreenPoint(newWorldPos)
                    );
            }
        }

        private void updateTime()
        {
            if (inputState == InputState.Begin)
            {
                oldTime = Time.time;
            }
            newTime = Time.time;
        }
        public float HoldTime
        {
            get { return newTime - oldTime; }
        }
    }

    public abstract InputData getInput();
}
