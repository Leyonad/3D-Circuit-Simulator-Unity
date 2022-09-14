using UnityEngine;

public class ComponentsManager : MonoBehaviour
{
    public static GameObject components;

    public static void CreateAllObjects()
    {
        //create all metal objects
        components = GameObject.FindGameObjectWithTag("Components");
        CreateBreadboard(new Vector3(10f, 1f, 5.5f), 30, 5, new Vector3(0.2f, 0.2f, 1.5f));
    }

    public static void CreateBreadboard(Vector3 boardsize, int rows, int columns, Vector3 metalStripSize)
    {
        new Breadboard(boardsize, rows, columns, metalStripSize);
    }
}
