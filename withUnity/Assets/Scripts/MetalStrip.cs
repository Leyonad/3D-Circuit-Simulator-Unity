using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalStrip
{
    private int amountOfMetals;
    private Vector3 position;
    private Vector3 size;
    private GameObject parent;
    private GameObject metalStripObject;

    public MetalStrip(Vector3 position, int amountOfMetals, Vector3 size, GameObject parent, float margin)
    {
        this.position = position;
        this.amountOfMetals = amountOfMetals;
        this.size = size;
        this.parent = parent;
        
        metalStripObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        metalStripObject.transform.position = this.position;
        metalStripObject.transform.localScale = this.size;
        metalStripObject.transform.SetParent(this.parent.transform);

        //create each metal
        float metalSize = (this.size.z - margin * (amountOfMetals - 1)) / amountOfMetals;
        float leftOrigin = metalStripObject.transform.position.z-this.size.z/2+metalSize/2;
        for (int i = 0; i < amountOfMetals; i++)
        {
            Vector3 metalPosition = new Vector3(this.position.x, this.position.y+2f, leftOrigin + (metalSize + margin) * i);
            new Metal(metalPosition, new Vector3(metalSize, this.size.y, metalSize), metalStripObject);
        }
    }
}
