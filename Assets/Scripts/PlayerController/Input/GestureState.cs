

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GestureState
{
    STARTED,
    ONGOING,
    FINISHED,
}

public static class GestureStateConversion
{
    public static GestureState toGestureState(this TouchPhase touchPhase)
    {
        switch (touchPhase)
        {
            case TouchPhase.Began: return GestureState.STARTED;
            case TouchPhase.Moved: return GestureState.ONGOING;
            case TouchPhase.Stationary: return GestureState.ONGOING;
            case TouchPhase.Ended: return GestureState.FINISHED;
            case TouchPhase.Canceled: return GestureState.FINISHED;
            default: throw new UnityException("TouchPhase type not recognized: " + touchPhase);
        }
    }
    public static GestureState toGestureState(this List<Touch> touches)
    {
        List<GestureState> gestureStates = touches
            .ConvertAll(t => t.phase.toGestureState());
        if (gestureStates.All(gs => gs == GestureState.STARTED))
        {
            return GestureState.STARTED;
        }
        else if (gestureStates.All(gs => gs == GestureState.FINISHED))
        {
            return GestureState.FINISHED;
        }
        else
        {
            return GestureState.ONGOING;
        }
    }
}