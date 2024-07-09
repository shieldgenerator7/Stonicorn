using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour
{

    private AudioSource music;
    private Collider2D coll2d;

    // Use this for initialization
    void Start()
    {
        music = GetComponent<AudioSource>();
        music.volume = 0;
        coll2d = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.isPlayer())
        {
            playTrack();
        }
    }

    public bool checkZone(Vector2 pos)
    {
        if (coll2d.OverlapPoint(pos))
        {
            playTrack();
            return true;
        }
        return false;
    }

    public void playTrack()
    {
        Managers.Music.setCurrentSong(music);
    }
}
