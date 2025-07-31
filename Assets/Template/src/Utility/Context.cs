using UnityEngine;

public static class Context {
    public static Arena         SingleFrameArena;
    public static EntityManager GameplayEntityManager;
    public static PlantField    Field;

    public static void InitContext(EntityManager gameplayEm) {
        SingleFrameArena = new Arena(65536);
        GameplayEntityManager = gameplayEm;
    }

    public static void DestroyContext() {
        SingleFrameArena.Dispose();
    }

    public static EntityManager GetGameplayEntityManager() {
        return GameplayEntityManager;
    }

    public static PlantField GetPlantField() {
        return Field;
    }
}