using UnityEngine;

public struct Burnable {}

public struct Burned {}

public struct Fire {
    public int      Position;
    public Particle Particle;
    public float BurnStage;
    public float BurnSpeed;
}