using UnityEngine;
using Unity.Profiling;

using static Context;

public static class Systems {
    private static uint[] EntityBuffer = new uint[128];

    private static int[] Neighbours = new int[8];
    private const int    MaxSpreadCount = 5;
    private const int    SpreadChance = 1;

    public static Fire MakeFire(int fieldPosition, Vector3 position) {
        var fire = new Fire();
        fire.Position     = fieldPosition;
        fire.Particle     = Particles.PlayParticle("fire_particle", position);
        fire.BurnStage    = 0f;
        fire.BurnSpeed    = 0.1f;

        return fire;
    }

    private static ProfilerMarker Marker = new ProfilerMarker("UpdateFire");

    public static void UpdateFire(Fire[] components, int count) {
        Marker.Begin();
        var em = GetGameplayEntityManager();
        var field       = GetPlantField();
        var max         = field.Size.x * field.Size.y;

        for (var i = 1; i < count; ++i) {
            ref var c      = ref components[i];
            c.BurnStage += c.BurnSpeed * Clock.Delta;

            if (c.BurnStage >= 1f) {
                c.Particle.Stop();
                var h = ComponentSystem<Fire>.GetHandle(i);
                h.RemoveComponent<Fire>();
                i--;
                count--;
                h.AddComponent<Burned>();
                em.GetEntity(h, out Plant p);
                p.Burn();
                continue;
            }

            var spreadCount = 0;
            var neighboursCount = 0;

            int x = c.Position % field.Size.x;
            int z = c.Position / field.Size.x;

            // Debug.Log($"{c.Position}, ({x},{z})");

            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0)
                        continue;

                    int nx = x + dx;
                    int nz = z + dz;

                    if (nx >= 0 && nx < field.Size.y && nz >= 0 && nz < field.Size.x)
                    {
                        int index = nx + nz * field.Size.x;
                        var h     = field.Plants[index];
                        if (h.HasComponent<Burnable>()) {
                            Neighbours[neighboursCount++] = index;
                        }
                    }
                }
            }

            for (var n = 0; n < neighboursCount; ++n) {
                var index  = Neighbours[n];
                var random = Random.Range(0, 100);
                var h      = field.Plants[index];
                if (random < SpreadChance) {
                    h.RemoveComponent<Burnable>();
                    em.GetEntity(h, out Plant e);
                    h.AddComponent<Fire>(MakeFire(index, e.Position));
                    e.FireUp();
                    spreadCount++;
                }

                if (spreadCount == MaxSpreadCount) break;
            }
        }

        Marker.End();
    }
}