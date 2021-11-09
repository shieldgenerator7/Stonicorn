using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    public int levelId;

    private void OnTriggerEnter2D(Collider2D coll2d)
    {
        if (coll2d.isPlayerSolid())
        {
            onGoalReached?.Invoke();
        }
    }
    public delegate void OnGoalReached();
    public event OnGoalReached onGoalReached;
}
