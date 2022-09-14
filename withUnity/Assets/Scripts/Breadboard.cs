using UnityEngine;

public class Breadboard
{
    private GameObject breadboardObject;

    public Breadboard(Vector3 size, int rows, int columns, Vector3 metalStripSize)
    {
        breadboardObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        breadboardObject.transform.SetParent(ComponentsManager.components.transform);
        breadboardObject.name = "Breadboard";
        breadboardObject.tag = "Breadboard";
        breadboardObject.transform.localScale = size;
        breadboardObject.transform.position = new Vector3(0, size.y/2, 0);

        breadboardObject.AddComponent<BoxCollider>();

        breadboardObject.AddComponent<MouseHovering>();
        ColorUtility.TryParseHtmlString("#D9D9D9", out breadboardObject.GetComponent<MouseHovering>().hoveringColor);

        GameObject metals = new GameObject("Metals");
        metals.tag = "Metals";
        metals.transform.SetParent(breadboardObject.transform);

        //create the metal strips (inside)
        float distToNextRow = 0.1f;
        float distToNextSide = 0.6f;
        float XlengthWithGap = rows * metalStripSize.x + (rows-1) * distToNextRow;
        float ZlengthWithGap = 2 * metalStripSize.z + distToNextSide;
        Vector3 offset = new Vector3(size.x/2f-metalStripSize.x/2f - (size.x-XlengthWithGap)/2f, 0, size.z/2f-metalStripSize.z/2f - (size.z - ZlengthWithGap)/2f);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < 2; j++) //2 for left and right
            {
                Vector3 position = new Vector3((distToNextRow+metalStripSize.x) * i, size.y, j * (distToNextSide+metalStripSize.z));
                new MetalStrip(position-offset, columns, metalStripSize, metals, distToNextRow);
            }
        }

        //create the metal strips (outside)

    }
}
