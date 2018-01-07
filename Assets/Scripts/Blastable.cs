using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Blastable {
    /// <summary>
    /// Tells the Blastable object to react to being blasted by an explosion
    /// </summary>
    /// <param name="force"></param>
    /// <returns>The amount of damage done
    /// (positive value means damage dealt, negative means HP healed)</returns>
    float checkForce(float force);
    float getDistanceFromExplosion(Vector2 explosionPos);
}
