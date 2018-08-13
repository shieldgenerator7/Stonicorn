using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountDownTimer : MonoBehaviour {

    public float countDownTime = 10;//after these many seconds the game will reset

    private float startTime = 0;

    private void OnEnable()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update () {
		if (Time.time > startTime + countDownTime)
        {
            GameManager.resetGame();
        }
	}
}
