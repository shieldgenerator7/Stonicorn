using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTimerTrigger : MonoBehaviour
{

    /// <summary>
    /// How long until the game resets (seconds)
    /// </summary>
    public float timerAmount = 15 * 60;//x min * y sec/min = z sec

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.setResetTimer(timerAmount);
            GameManager.resetGame();
        }
    }
}
