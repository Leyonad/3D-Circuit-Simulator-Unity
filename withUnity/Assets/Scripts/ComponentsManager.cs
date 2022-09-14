using UnityEngine;

public class ComponentsManager : MonoBehaviour
{
    public static GameObject components;

    public static void CreateAllObjects()
    {
        //create all metal objects
        components = GameObject.FindGameObjectWithTag("Components");
        CreateBreadboard(new Vector3(0, 0.5f, 0), new Vector3(12f, 1f, 6.5f), 30, 5, new Vector3(0.25f, 0.25f, 0.25f), 0.1f);
    }

    public static void CreateBreadboard(Vector3 positionBreadboard, Vector3 boardsize, int rows, int columns, Vector3 eachMetalSize, float margin)
    {
        new Breadboard(positionBreadboard, boardsize, rows, columns, eachMetalSize, margin);
    }
}
