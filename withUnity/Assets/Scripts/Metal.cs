using UnityEngine;

public class Metal
{
    private float elec;
    private Vector3 position;
    private Vector3 size;
    private GameObject metalObject;

    public Metal(Vector3 position, Vector3 size, GameObject parent, float elec = 0f)
    {
        this.position = position;
        this.size = size;
        this.elec = elec;

        metalObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        metalObject.name = "Metal";
        metalObject.GetComponent<MeshRenderer>().material = ResourcesManager.metalMaterial;
        metalObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        metalObject.transform.position = this.position;
        metalObject.transform.localScale = this.size;
        metalObject.transform.SetParent(parent.transform);
    }
}
