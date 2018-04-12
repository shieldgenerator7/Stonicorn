using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionSnapper : MonoBehaviour
{

    public bool snapToPlayerGhosts = true;

    private Vector2 startPosition;

    // Use this for initialization
    void Awake()
    {
        startPosition = transform.position;
    }

    private void OnEnable()
    {
        if (snapToPlayerGhosts)
        {
            transform.position = GameManager.getClosestPlayerGhost(startPosition)
                .transform.position;
        }
    }
}
