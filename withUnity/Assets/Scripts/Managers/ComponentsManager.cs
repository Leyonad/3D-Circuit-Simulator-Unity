using UnityEngine;

public class ComponentsManager : MonoBehaviour
{
    public static GameObject components;

    public static void CreateAllObjects()
    {
        //create all metal objects
        components = GameObject.FindGameObjectWithTag("Components");

        CreateBreadboard(
            new Vector3(3f, 0.35f, -2), 
            new Vector3(10f, 0.7f, 5.5f), 
            30, 5, 25, 
            new Vector3(0.2f, 0.07f, 0.2f), 
            0.1f);

        CreateBattery(
            new Vector3(-3.2f, 0.5f, 6.6f), 
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
