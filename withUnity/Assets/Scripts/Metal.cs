using UnityEngine;

public class Metal
{
    private float elec;
    private Vector3 position;
    private Vector3 size;
    private GameObject metalObject;
    private GameObject parent;

    public Metal(Vector3 position, Vector3 size, GameObject parent, float elec = 0f)
    {
        this.position = position;
        this.size = size;
        this.elec = elec;
        this.parent = parent;

        metalObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        metalObject.transform.localPosition = this.position;
        metalObject.transform.localScale = this.size;
        metalObject.transform.SetParent(parent.transform);
    }
}
