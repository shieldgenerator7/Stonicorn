using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour
{

    private AudioSource music;

    // Use this for initialization
    void Start()
    {
        music = GetComponent<AudioSource>();
        music.volume = 0;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.isPlayer())
        {
            playTrack();
        }
    }

    public void checkZone(Vector2 pos)
    {
        if (GetComponent<Collider2D>().OverlapPoint(pos))
        {
            playTrack();
        }
    }

    public void playTrack()
    {
        Managers.Music.setCurrentSong(music);
    }
}
