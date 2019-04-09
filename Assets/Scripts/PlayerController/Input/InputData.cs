
using UnityEngine;

public class InputData
{
    public enum InputState { Begin, Hold, End, None };
    //World Pos
    private Vector2 oldWorldPos;
    private Vector2 newWorldPos;
    //Screen Pos
    private Vector2 oldScreenPos;
    private Vector2 newScreenPos;
    //Time
    private float oldTime;
    private float newTime;
    //State
    public InputState inputState;
    public float zoomMultiplier;

    public InputData()
    {
        clear();
    }

    public void clear()
    {
        this.oldWorldPos = Vector2.zero;
        this.newWorldPos = Vector2.zero;
        this.oldScreenPos = Vector2.zero;
        this.newScreenPos = Vector2.zero;
        this.oldTime = 0;
        this.newTime = 0;
        this.inputState = InputState.None;
        this.zoomMultiplier = 1;
    }
    public void process()
    {
        if (inputState == InputState.Begin)
        {
            if (newWorldPos != Vector2.zero)
            {
                NewWorldPos = newWorldPos;
            }
            if (newScreenPos != Vector2.zero)
            {
                NewScreenPos = newScreenPos;
            }
        }
        updateTime();
    }

    /// <summary>
    /// Calculated when called, not stored
    /// </summary>
    public Vector2 OldWorldPos
    {
        get
        {
            if (oldWorldPos == Vector2.zero)
            {
                return Camera.main.ScreenToWorldPoint(oldScreenPos);
            }
            return oldWorldPos;
        }
        private set { oldWorldPos = value; }
    }
    /// <summary>
    /// Calculated when called, not stored
    /// </summary>
    public Vector2 NewWorldPos
    {
        get
        {
            if (newWorldPos == Vector2.zero)
            {
                return Camera.main.ScreenToWorldPoint(newScreenPos);
            }
            return newWorldPos;
        }
        set
        {
            newWorldPos = value;
            if (inputState == InputState.Begin)
            {
                OldWorldPos = NewWorldPos;
            }
        }
    }
    public Vector2 OldScreenPos
    {
        get
        {
            if (oldScreenPos == Vector2.zero)
            {
                return Camera.main.WorldToScreenPoint(oldWorldPos);
            }
            return oldScreenPos;
        }
        private set { oldScreenPos = value; }
    }
    public Vector2 NewScreenPos
    {
        get
        {
            if (newScreenPos == Vector2.zero)
            {
                return Camera.main.WorldToScreenPoint(newWorldPos);
            }
            return newScreenPos;
        }
        set
        {
            newScreenPos = value;
            if (inputState == InputState.Begin)
            {
                OldScreenPos = newScreenPos;
            }
        }
    }
    /// <summary>
    /// The distance between the oldScreenPos and the newScreenPos
    /// </summary>
    public float PositionDelta
    {
        get { return Vector2.Distance(OldScreenPos, NewScreenPos); }
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
