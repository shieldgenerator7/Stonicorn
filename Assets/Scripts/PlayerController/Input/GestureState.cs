


public enum GestureState
{
    START,
    ONGOING,
    FINISH,
}

public static class GestureStateUtil
{
    public static GestureState FromBool(bool finished)
        => (finished) ? GestureState.FINISH : GestureState.ONGOING;

    public static bool Finished(this GestureState state)
        => state == GestureState.FINISH;
}
