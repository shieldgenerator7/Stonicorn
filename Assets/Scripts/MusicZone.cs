using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour {

    private AudioSource music;
    private MusicManager musicManager;

	// Use this for initialization
	void Start () {
        music = GetComponent<AudioSource>();
        music.volume = 0;
        musicManager = FindObjectOfType<MusicManager>();
	}
    
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == GameManager.playerTag)
        {
            musicManager.setCurrentSong(music);
        }
    }
}
