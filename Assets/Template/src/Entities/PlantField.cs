using UnityEngine;

using static Context;

public class PlantField {
    public EntityHandle[] Plants;
    public Vector2Int     Size;

    public static PlantField Make(Vector2Int size) {
        var em       = GetGameplayEntityManager();
        var field    = new PlantField();
        field.Size   = size;
        field.Plants = new EntityHandle[size.x * size.y];

        var zSpace = 0f;
        for (var z = 0; z < size.x; ++z) {
            var xSpace = 0f;
            for (var x = 0; x < size.y; ++x) {
                var pos = new Vector3(xSpace, 0, zSpace);
                var e   = em.CreateEntity<Plant>("plant", pos, Quaternion.identity);
                e.AssignPosition(x + z * size.x);
                field.Plants[x + z * size.x] = e.Handle;

                xSpace += 1.1f;
            }
            zSpace += 1.1f;
        }

        return field;
    }
}