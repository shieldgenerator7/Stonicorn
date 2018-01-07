using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Blastable {
    void checkForce(float force);
    float getDistanceFromExplosion(Vector2 explosionPos);
}
