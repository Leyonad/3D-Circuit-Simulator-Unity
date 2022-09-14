using UnityEngine;

public class Breadboard
{
    private GameObject breadboardObject;

    public Breadboard(Vector3 positionBreadboard, Vector3 size, int rows, int columns, Vector3 eachMetalSize, float margin)
    {
        breadboardObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        breadboardObject.transform.SetParent(ComponentsManager.components.transform);
        breadboardObject.name = "Breadboard";
        breadboardObject.tag = "Breadboard";
        breadboardObject.transform.localScale = size;
        breadboardObject.transform.position = positionBreadboard;

        breadboardObject.AddComponent<BoxCollider>();

        breadboardObject.AddComponent<MouseHovering>();
        ColorUtility.TryParseHtmlString("#D9D9D9", out breadboardObject.GetComponent<MouseHovering>().hoveringColor);

        GameObject metals = new GameObject("Metals")
        {
            tag = "Metals"
        };
        metals.transform.SetParent(breadboardObject.transform);

        //create the metal strips (inside)
        float distToNextSide = 0.6f;
        float XlengthWithGap = rows * eachMetalSize.x + (rows-1) * margin;
        float ZlengthWithGap = 2 * (eachMetalSize.z * columns + (columns-1) * margin) + distToNextSide;
        float metalStripLength = columns * eachMetalSize.z + (columns - 1) * margin;
        Vector3 offset = new Vector3(size.x/2f- eachMetalSize.x/2f - (size.x-XlengthWithGap)/2f, 0, size.z/2f- metalStripLength / 2f - (size.z - ZlengthWithGap)/2f);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < 2; j++) //2 for left and right
            {
                Vector3 position = new Vector3((margin+ eachMetalSize.x) * i, size.y, j * (distToNextSide+ metalStripLength));
                new MetalStrip(position-offset, columns, new Vector3(eachMetalSize.x, eachMetalSize.y, metalStripLength), eachMetalSize, metals, margin, breadboardObject.transform.localScale);
            }
        }

        //create the metal strips (outside)
        Vector3 outsideSize = new Vector3(size.x*0.90f, eachMetalSize.y, eachMetalSize.x);
        float distanceToNext = 0.4f;
        float innerP = size.z*0.74f;
        float outerP = innerP+ eachMetalSize.x + distanceToNext;
        float p = outerP;
        int sign = -1;
        for (int i = 0; i < 4; i++)
        {
            Vector3 position = new Vector3(positionBreadboard.x, size.y, positionBreadboard.z+p/2*sign);
            new MetalStrip(position, 25, outsideSize, eachMetalSize, metals, margin, breadboardObject.transform.localScale, false);
            sign *= -1;
            if (i > 0)
                p = innerP;
        }
    }
}
