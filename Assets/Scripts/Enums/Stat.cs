

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
    ROTATE,//how many times the player has rotated
    FLASH_LIGHT,//how many times the player has used the flashlight ability
    FORCE_LAUNCH_LAUNCH,//how many times the player has launched with the force launch ability
    FORCE_LAUNCH_FIRE_DROP,//how many times the player has lost the on fire state to low momentum
    FORCE_LAUNCH_FIRE_CANCEL,//how many times the player has intentionally ended the on fire state (usually by teleporting)
    FORCE_LAUNCH_EXPLOSION,//how many explosions a player has made with force launch
    FORCE_LAUNCH_PROJECTILE,//how many fire clones the player has launched
    SWAP,//how many times the player has swapped during a teleport
    SWAP_OBJECT,//how many objects the player has swapped with
    SWAP_ROTATE,//how many times the player has rotated any object during a swap
    SWAP_ROTATE_OBJECT,//how many times the player has rotated an object
    SWAP_STASIS,//how many times the player has activated stasis
    SWAP_STASIS_OBJECT,//how many objects a player has put into stasis
    WALL_CLIMB,//how many times the player has teleported off a wall
    WALL_CLIMB_CEILING,//how many times the player has activated wall climb on a ceiling
    WALL_CLIMB_STICKY,//how many sticky pads the player has made
    ELECTRIC_BEAM,//how many times the player has activated the electric field ability
    ELECTRIC_BEAM_OBJECT,//how many objects the player has charged
    ELECTRIC_BEAM_WIRE,//how many wires the player has created
    AIR_SLICE,//how many times the player has teleported in the air with the air slice ability
    AIR_SLICE_OBJECT,//how many objects the player has sliced
    AIR_SLICE_WIND,//how many winds the player generated
    LONG_TELEPORT,//how many times the player has teleported with an extended range
    LONG_TELEPORT_PORTAL,//how many portals the player generated
    LONG_TELEPORT_PORTALED,//how many times the player used a portal
    #endregion

    #region Menu Buttons
    MENU_BUTTON_PLAY//how many times the player pressed the play button
    #endregion
}
