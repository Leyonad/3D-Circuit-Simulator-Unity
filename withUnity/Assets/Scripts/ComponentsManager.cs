using UnityEngine;

public class ComponentsManager : MonoBehaviour
{
    public static GameObject components;

    public static void CreateAllObjects()
    {
        //create all metal objects
        components = GameObject.FindGameObjectWithTag("Components");

        CreateBreadboard(new Vector3(3f, 0.5f, -2), new Vector3(12f, 1f, 6.5f), 30, 5, 25, new Vector3(0.25f, 0.25f, 0.25f), 0.1f);
        CreateBattery(new Vector3(-3.2f, 0.75f, 6.6f), "9V");
    }

    public static void CreateBreadboard(Vector3 positionBreadboard, Vector3 boardsize, int rows, int columns, int outsiderows, Vector3 eachMetalSize, float margin)
    {
        new Breadboard(positionBreadboard, boardsize, rows, columns, outsiderows, eachMetalSize, margin);
    }

    public static void CreateBattery(Vector3 batteryPosition, string type)
    {
        new Battery(batteryPosition, type);
    }
}
