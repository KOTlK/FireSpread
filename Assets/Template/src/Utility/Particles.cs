using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using static ResourceManager;

public static class Particles {
    public static Dictionary<string, Stack<Particle>> ParticlePool = new();
    public static Transform Parent;
    public static List<Particle> AllParticles = new();

    private const int InitialStackSize = 256;

    public static void Initialize(Transform parent) {
        Parent = parent;
        AllParticles = new();
        ParticlePool = new();
        AllParticles.Clear();
        ParticlePool.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Particle PlayParticle(string name, Vector3 position) {
        if(ParticlePool.ContainsKey(name)) {
            if(ParticlePool[name].Count > 0) {
                var particle = ParticlePool[name].Pop();
                particle.transform.position = position;
                particle.Play();
                return particle;
            } else {
                var particle = MakeParticle(name, position);
                particle.Play();
                return particle;
            }
        } else {
            ParticlePool.Add(name, new Stack<Particle>(InitialStackSize));
            var particle = MakeParticle(name, position);
            particle.Play();
            return particle;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear() {
        foreach(var (name, stack) in ParticlePool) {
            stack.Clear();
        }

        ParticlePool.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Release(Particle p) {
        ParticlePool[p.Name].Push(p);
    }

    public static void DestroyAll() {
        for(var i = 0; i < AllParticles.Count; ++i) {
            if(AllParticles[i]) {
                Object.Destroy(AllParticles[i].gameObject);
            }
        }

        AllParticles.Clear();
        Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Particle MakeParticle(string name, Vector3 position) {
        var particle = Instantiate<Particle>(name, position, Quaternion.identity, Parent);

        particle.Name     = name;
        particle.Stopped += Release;

        AllParticles.Add(particle);
        return particle;
    }
}