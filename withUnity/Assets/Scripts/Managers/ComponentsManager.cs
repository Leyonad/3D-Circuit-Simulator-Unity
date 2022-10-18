using UnityEngine;

public class ComponentsManager : MonoBehaviour
{
    public static GameObject components;

    public static void CreateAllObjects()
    {
        //create all metal objects
        components = GameObject.FindGameObjectWithTag("Components");

        CreateBreadboard(
            new Vector3(-1.2f, 0.35f, -0.4f), 
            new Vector3(10f, 0.7f, 5.5f), 
            30, 5, 25, 
            new Vector3(0.2f, 0.07f, 0.2f), 
            0.1f);

        CreateBattery(
            new Vector3(-6.5f, 0.95f, 4.7f), 
            "9V");
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
