using UnityEngine;

public class ResourcesManager
{
    public static Material white;
    public static Material lightred;
    public static Material red;
    public static Material blue;
    public static Material yellow;
    public static Material grey;

    public static Material wireMaterial;
    public static Material highlightWireMaterial;
    public static Material metalMaterial;
    public static Material breadboardMaterial;

    public static Material LED_default;
    public static Material LED_red;
    public static Material LED_green;
    public static Material LED_yellow;

    public static GameObject prefabBattery9V;
    public static GameObject prefabLED;
    public static GameObject prefabResistor;

    public static void LoadResources()
    {
        white = LoadMaterial("White");
        lightred = LoadMaterial("Lightred");
        red = LoadMaterial("Red");
        blue = LoadMaterial("Blue");
        yellow = LoadMaterial("Yellow");
        grey = LoadMaterial("Grey");

        wireMaterial = LoadMaterial("Wire_Material");
        highlightWireMaterial = LoadMaterial("Highlight_Wire_Material");
        metalMaterial = LoadMaterial("Breadboard_Metal_Normal_Material");
        breadboardMaterial = LoadMaterial("Breadboard_Material");

        LED_default = LoadMaterial("LED_default");
        LED_red = LoadMaterial("LED_red");
        LED_green = LoadMaterial("LED_green");
        LED_yellow = LoadMaterial("LED_yellow");

        prefabBattery9V = LoadPrefab("Battery9V");
        prefabLED = LoadPrefab("LED");
        prefabResistor = LoadPrefab("Resistor");
    }

    private static Material LoadMaterial(string name)
    {
        Material material = Resources.Load($"Materials/{name}", typeof(Material)) as Material;
        if (material == null)
            Debug.Log($"{name} -> Material not found!");
        return material;
    }

    private static GameObject LoadPrefab(string name)
    {
        GameObject gameobj = Resources.Load($"Prefabs/{name}", typeof(GameObject)) as GameObject;
        if (gameobj == null)
            Debug.Log($"{name} -> Prefab not found!");
        return gameobj;
    }
}
