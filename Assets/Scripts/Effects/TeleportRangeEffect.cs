using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TeleportRangeEffect : MonoBehaviour
{

    public abstract void updateEffect(List<GameObject> fragments, float timeLeft, float duration);
}
