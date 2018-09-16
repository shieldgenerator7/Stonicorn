using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeightSwitchActivator : MonoBehaviour
{

    public bool pressed = false;
    private ParticleSystem[] particleSystems;

    private void Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void OnCollisionStay2D()
    {
        pressed = true;
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Play();
        }
    }

    private void OnCollisionExit2D()
    {
        pressed = false;
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Pause();
            ps.Clear();
        }
    }

}
