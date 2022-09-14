using UnityEngine;

public class MetalStrip
{
    private Vector3 position;
    private Vector3 size;
    private GameObject parent;
    private GameObject metalStripObject;

    public MetalStrip(Vector3 position, int amountOfMetals, Vector3 size, GameObject parent, float margin)
    {
        this.position = position;
        this.size = size;
        this.parent = parent;
        
        metalStripObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        metalStripObject.name = "MetalStrip";
        metalStripObject.transform.position = this.position;
        metalStripObject.transform.localScale = this.size;
        metalStripObject.transform.SetParent(this.parent.transform);
        metalStripObject.GetComponent<BoxCollider>().enabled = false;

        //dont render the metalstrip object and disable the boxcollider
        //metalStripObject.GetComponent<MeshRenderer>().enabled = false;

        //create each metal
        /*
        float metalSize = (this.size.z - margin * (amountOfMetals - 1)) / amountOfMetals;
        float leftOrigin = metalStripObject.transform.position.z-this.size.z/2+metalSize/2;
        for (int i = 0; i < amountOfMetals; i++)
        {
            Vector3 metalPosition = new Vector3(this.position.x, this.position.y, leftOrigin + (metalSize + margin) * i);
            new Metal(metalPosition, new Vector3(metalSize, this.size.y, metalSize), metalStripObject);
        }
        */
    }
}
