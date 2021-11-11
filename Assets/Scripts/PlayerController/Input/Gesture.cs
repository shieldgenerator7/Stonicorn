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
    public Vector2 startPosition;
    public Vector2 position;
}
