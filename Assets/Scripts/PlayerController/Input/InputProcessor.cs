using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InputProcessor
{
    void processTapGesture(Vector2 tapPos);

    void processHoldGesture(Vector2 holdPos, float holdTime, InputData.InputState state);

    void processDragGesture(Vector2 oldPos, Vector2 newPos, InputData.InputState state);

    void processZoomGesture(float zoomMultiplier, InputData.InputState state);

    //Defaults
    //public virtual void processTapGesture(Vector2 tapPos)
    //{
    //    throw new System.NotImplementedException("" + GetType() + ".processTapGesture() (from interface InputProcessor) not implemented!");
    //}

    //public virtual void processHoldGesture(Vector2 holdPos, float holdTime, PlayerInput.InputState state)
    //{
    //    throw new System.NotImplementedException("" + GetType() + ".processHoldGesture() (from interface InputProcessor) not implemented!");
    //}

    //public virtual void processDragGesture(Vector2 oldPos, Vector2 newPos, PlayerInput.InputState state)
    //{
    //    throw new System.NotImplementedException("" + GetType() + ".processDragGesture() (from interface InputProcessor) not implemented!");
    //}

    //public virtual void processZoomGesture(float zoomMultiplier, PlayerInput.InputState state)
    //{
    //    throw new System.NotImplementedException("" + GetType() + ".processZoomGesture() (from interface InputProcessor) not implemented!");
    //}
}
