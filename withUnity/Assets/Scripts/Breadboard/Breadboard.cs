using UnityEngine;

public class Breadboard
{
    private GameObject breadboardObject;

    public Breadboard(Vector3 positionBreadboard, Vector3 size, int rows, int columns, int outsiderows, Vector3 eachMetalSize, float margin)
    {
        breadboardObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        breadboardObject.transform.SetParent(ComponentsManager.components.transform);
        breadboardObject.name = "Breadboard";
        breadboardObject.tag = "Breadboard";
        breadboardObject.transform.localScale = size;
        breadboardObject.transform.position = positionBreadboard;

        breadboardObject.GetComponent<MeshRenderer>().material = ResourcesManager.breadboardMaterial;

        //breadboardObject.AddComponent<MouseHovering>();
        //ColorUtility.TryParseHtmlString("#7E7E7E", out breadboardObject.GetComponent<MouseHovering>().hoveringColor);

        GameObject metals = new GameObject("Metals")
        {
            tag = "Metals"
        };
        metals.transform.SetParent(breadboardObject.transform);

        //create the metal strips (inside)
        float distToNextSide = 0.4f;
        float XlengthWithGap = rows * eachMetalSize.x + (rows - 1) * margin;
        float ZlengthWithGap = 2 * (eachMetalSize.z * columns + (columns - 1) * margin) + distToNextSide;
        float metalStripLength = columns * eachMetalSize.z + (columns - 1) * margin;
        Vector3 offset = new Vector3(size.x / 2f - eachMetalSize.x / 2f - (size.x - XlengthWithGap) / 2f, -eachMetalSize.y / 2f, size.z / 2f - metalStripLength / 2f - (size.z - ZlengthWithGap) / 2f);
        Material material = ResourcesManager.metalMaterial;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < 2; j++) //2 for left and right
            {
                Vector3 position = new Vector3(positionBreadboard.x + (margin + eachMetalSize.x) * i, size.y - 0.03f, positionBreadboard.z + j * (distToNextSide + metalStripLength));
                new MetalStrip(position - offset, columns, new Vector3(eachMetalSize.x - 0.08f, eachMetalSize.y, metalStripLength), eachMetalSize, metals, margin, breadboardObject.transform.localScale, material);
            }
        }

        //create the metal strips (outside)
        Vector3 outsideSize = new Vector3(size.x * 0.90f, 0.07f, 0.13f);
        float distanceToNext = 0.4f;
        float innerP = size.z * 0.74f;
        float outerP = innerP + eachMetalSize.x + distanceToNext;
        float p = outerP;
        int sign = -1;
        for (int i = 0; i < 4; i++)
        {
            if (sign == -1 && p == outerP || sign == 1 && p == innerP)
                material = ResourcesManager.lightred;
            else material = ResourcesManager.blue;
            Vector3 position = new Vector3(positionBreadboard.x, size.y + eachMetalSize.y / 2f - 0.03f, positionBreadboard.z + p / 2 * sign);
            new MetalStrip(position, outsiderows, outsideSize, eachMetalSize, metals, margin, breadboardObject.transform.localScale, material, false);
            sign *= -1;
            if (i > 0)
                p = innerP;
        }
    }
}
