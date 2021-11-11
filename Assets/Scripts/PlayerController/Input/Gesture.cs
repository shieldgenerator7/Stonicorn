using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Gesture
{
    public float startTime;
    public float time;
    public GestureType type;
    public GestureDragType dragType;
    public GestureState state;
    public Vector2 startPosition;//start position in world coordinates
    public Vector2 position;//position in world coordinates

    public float HoldTime => time - startTime;

    public Gesture(Vector2 position)
    {
        this.startTime = Managers.Time.Time;
        this.time = startTime;
        this.type = GestureType.TAP;
        this.dragType = GestureDragType.UNKNOWN;
        this.state = GestureState.FINISHED;
        this.startPosition = position;
        this.position = position;
    }

    public Gesture(Vector2 position, float holdTime, GestureState state)
    {
        this.time = Managers.Time.Time;
        this.startTime = this.time - holdTime;
        this.type = GestureType.HOLD;
        this.dragType = GestureDragType.UNKNOWN;
        this.state = state;
        this.startPosition = position;
        this.position = position;
    }

    public Gesture(Vector2 startPosition, Vector2 endPosition, GestureDragType dragType, GestureState state)
    {
        this.startTime = Managers.Time.Time;
        this.time = startTime;
        this.type = GestureType.DRAG;
        this.dragType = dragType;
        this.state = state;
        this.startPosition = startPosition;
        this.position = endPosition;
    }
}
