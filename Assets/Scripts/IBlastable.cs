using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBlastable {
    /// <summary>
    /// Tells the Blastable object to react to being blasted by an explosion
    /// </summary>
    /// <param name="force">The force applied to the Blastable</param>
    /// <param name="direction">The unnormalized direction the force is applied in</param>
    /// <returns>The amount of damage done
    /// (positive value means damage dealt, negative means HP healed)</returns>
    float checkForce(float force, Vector2 direction);
    float getDistanceFromExplosion(Vector2 explosionPos);
}
