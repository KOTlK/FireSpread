using System;
using UnityEngine;

public class Particle : MonoBehaviour {
    public event Action<Particle> Stopped = delegate {};

    public string         Name;
    public ParticleSystem System;

    public void Play() {
        System.Play();
    }

    public void Stop() {
        System.Stop();
    }

    private void OnParticleSystemStopped() {
        Stopped(this);
    }
}