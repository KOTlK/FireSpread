using UnityEngine;

public class Plant : Entity {
    public int   FieldPosition;
    public float GrowStage = 0f;
    public float GrowSpeed = 2f;

    public Color BurnColor;
    public Color BurnedColor;

    public Material Material;

    public override void OnCreate() {
        Material = GetComponent<MeshRenderer>().material;
    }

    public void AssignPosition(int fieldPos) {
        FieldPosition = fieldPos;

        var r = Random.Range(0, 100);

        if (r <= 1) {
            Handle.AddComponent<Fire>(Systems.MakeFire(FieldPosition, Position));
            FireUp();
        } else {
            Handle.AddComponent<Burnable>();
        }
    }

    public void FireUp() {
        Material.color = BurnColor;
    }

    public void Burn() {
        Material.color = BurnedColor;
    }
}