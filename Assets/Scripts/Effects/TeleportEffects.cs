using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportEffects : MonoBehaviour
{
    public PlayerAbility playerAbility;
    public enum PlayPosition
    {
        OLD_POS,
        NEW_POS,
        NEITHER
    }
    public PlayPosition playPosition = PlayPosition.OLD_POS;

    private ParticleSystemController effectParticleController;
    private ParticleSystem effectParticleSystem;

    private void OnEnable()
    {
        effectParticleController = GetComponent<ParticleSystemController>();
        effectParticleSystem = GetComponent<ParticleSystem>();
        ParticleSystem.MainModule psmm = effectParticleSystem.main;
        psmm.startColor = playerAbility.EffectColor.adjustAlpha(psmm.startColor.color.a);
        playerAbility.onEffectedTeleport += processTeleport;
    }
    private void OnDisable()
    {
        playerAbility.onEffectedTeleport -= processTeleport;
    }

    private void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        switch (playPosition)
        {
            case PlayPosition.OLD_POS:
                playAtPos(oldPos);
                break;
            case PlayPosition.NEW_POS:
                playAtPos(newPos);
                break;
            case PlayPosition.NEITHER: break;
            default: throw new System.Exception("Option not possible! playPosition: " + playPosition);
        }
    }

    private void playAtPos(Vector2 pos, bool play = true)
    {
        effectParticleSystem.transform.position = pos;
        if (play)
        {
            effectParticleSystem.Play();
        }
        else
        {
            effectParticleSystem.Pause();
            effectParticleSystem.Clear();
        }
    }
}
