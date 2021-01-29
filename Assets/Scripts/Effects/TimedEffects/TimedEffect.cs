using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimedEffect : MonoBehaviour
{
    public abstract void processEffect(float time);
}
