

public enum Stat
{
    #region Touch Input
    TAP,//how many times the player has tapped
    HOLD,//how many times the player has done the hold gesture
    DRAG,//how many times the player has done the drag gesture
    PINCH,//how many times the player has done the pinch gesture (including mouse wheel)
    #endregion

    #region Abilities
    TELEPORT,//how many times the player has teleported
    FORCE_LAUNCH_LAUNCH,//how many times the player has launched with the force launch ability
    SWAP,//how many times the player has swapped during a teleport
    SWAP_OBJECT,//how many objects the player has swapped with
    WALL_CLIMB,//how many times the player has teleported off a wall
    WALL_CLIMB_STICKY,//how many sticky pads the player has made
    ELECTRIC_BEAM,//how many times the player has activated the electric field ability
    ELECTRIC_BEAM_OBJECT,//how many objects the player has charged
    AIR_SLICE,//how many times the player has teleported in the air with the air slice ability
    AIR_SLICE_OBJECT,//how many objects the player has sliced
    LONG_TELEPORT,//how many times the player has teleported with an extended range
    #endregion

    #region Rewind
    DAMAGED,//how many times the player has been damaged
    REWIND,//how many times the rewind ability has been activated
    REWIND_PLAYER//how many times the player used the rewind ability
    #endregion
}
