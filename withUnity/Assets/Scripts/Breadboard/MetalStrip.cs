using UnityEngine;

public class MetalStrip
{
    private Vector3 position;
    private Vector3 size;
    private GameObject parent;
    public GameObject metalStripObject;

    public MetalStrip(Vector3 position, int amountOfMetals, Vector3 size, Vector3 eachMetalSize, GameObject parent, float margin, Vector3 breadboardsize, Material material, bool inside=true)
    {
        this.position = position;
        this.size = size;
        this.parent = parent;
        
        metalStripObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        metalStripObject.name = "MetalStrip";
        metalStripObject.tag = "MetalStrip";
        metalStripObject.transform.position = this.position;
        metalStripObject.transform.localScale = this.size;
        metalStripObject.transform.SetParent(this.parent.transform);
        metalStripObject.GetComponent<MeshRenderer>().material = ResourcesManager.breadboardMetalStrip;
        
        metalStripObject.AddComponent<Properties>();

        float metalSize = eachMetalSize.x;

        //create each metal (inside)
        if (inside)
        {
            float leftOrigin = this.position.z - this.size.z / 2 + metalSize / 2 - 0.01f;
            for (int i = 0; i < amountOfMetals; i++)
            {
                Vector3 metalPosition = new Vector3(this.position.x, this.position.y + 0.03f, leftOrigin + (metalSize + margin+0.004f) * i);
                new Metal(metalPosition, new Vector3(metalSize, this.size.y, metalSize), metalStripObject, material);
            }
        }
        //create each metal (outside)
        else
        {
            float leftOrigin = this.position.x - this.size.x / 2 + metalSize / 2;
            float partLeft = breadboardsize.x - this.size.x;
            float incr = (breadboardsize.x - ((metalSize + margin) * amountOfMetals + partLeft)) / 5;
            float a = -incr;
            for (int i = 0; i < amountOfMetals; i++)
            {
                if (i%5 == 0) {
                    a+=incr;
                }
                Vector3 metalPosition = new Vector3(leftOrigin + (metalSize + margin) * i + a + metalSize, this.position.y + 0.03f, this.position.z);
                new Metal(metalPosition, new Vector3(metalSize, this.size.y, metalSize), metalStripObject, material);
            }
        }
        
        
    }
}
