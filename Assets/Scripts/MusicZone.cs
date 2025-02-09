using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour
{

    [AutoInitialize, SerializeField, HideInInspector]
    private AudioSource music;
    [AutoInitialize, SerializeField, HideInInspector]
    private Collider2D coll2d;

    // Use this for initialization
    void Start()
    {
        init();
    }

    public void init()
    {
        music.volume = 0;
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
        if (conditionsMet(pos))
        {
            playTrack();
            return true;
        }
        return false;
    }
    protected virtual bool conditionsMet(Vector2 pos)
    {
        return coll2d.OverlapPoint(pos);
    }

    public void playTrack()
    {
        Managers.Music.setCurrentSong(music);
    }
}
