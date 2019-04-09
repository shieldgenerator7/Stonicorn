
using UnityEngine;

public class InputData
{
    public enum InputState { Begin, Hold, End, None };
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
        this.OldScreenPos = oldScreenPos;
        this.NewScreenPos = newScreenPos;
    }
    public void process()
    {
        NewWorldPos = NewWorldPos;//refresh the new world pos
        updateTime();
    }

    /// <summary>
    /// Calculated when called, not stored
    /// </summary>
    public Vector2 OldWorldPos
    {
        get { return oldWorldPos; }
        private set { oldWorldPos = value; }
    }
    /// <summary>
    /// Calculated when called, not stored
    /// </summary>
    public Vector2 NewWorldPos
    {
        get { return newWorldPos; }
        set
        {
            newWorldPos = value;
            if (inputState == InputState.Begin)
            {
                OldWorldPos = newWorldPos;
            }
        }
    }
    public Vector2 OldScreenPos
    {
        get { return Camera.main.WorldToScreenPoint(this.oldWorldPos); }
        private set { this.oldWorldPos = Camera.main.ScreenToWorldPoint(value); }
    }
    public Vector2 NewScreenPos
    {
        set { NewWorldPos = Camera.main.ScreenToWorldPoint(value); }
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
