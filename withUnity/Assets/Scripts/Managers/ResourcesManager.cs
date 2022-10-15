using UnityEngine;

public class ResourcesManager
{
    public static Material white;
    public static Material lightred;
    public static Material red;
    public static Material blue;
    public static Material yellow;
    public static Material grey;
    public static Material black;

    public static Material wireMaterial;
    public static Material highlightWireMaterial;
    public static Material highlightItemMaterial;
    public static Material metalMaterial;
    public static Material breadboardMaterial;
    public static Material breadboardMetalStrip;

    public static Material LED_default;
    public static Material LED_red;
    public static Material LED_green;
    public static Material LED_yellow;
    public static Material LED_blue;

    public static GameObject prefabBattery9V;
    public static GameObject prefabLED;
    public static GameObject prefabResistor;

    public static void LoadResources()
    {
        white = LoadMaterial("PrimaryColors/White");
        lightred = LoadMaterial("PrimaryColors/Lightred");
        red = LoadMaterial("PrimaryColors/Red");
        blue = LoadMaterial("PrimaryColors/Blue");
        yellow = LoadMaterial("PrimaryColors/Yellow");
        grey = LoadMaterial("PrimaryColors/Grey");
        black = LoadMaterial("PrimaryColors/Black");

        wireMaterial = LoadMaterial("Wire/Wire_Material");
        highlightWireMaterial = LoadMaterial("Wire/Highlight_Wire_Material");
        highlightItemMaterial = LoadMaterial("Wire/Highlight_Item_Material");
        metalMaterial = LoadMaterial("Breadboard/Breadboard_Metal_Normal_Material");
        breadboardMaterial = LoadMaterial("Breadboard/Breadboard_Material");
        breadboardMetalStrip = LoadMaterial("Breadboard/Breadboard_MetalStrip");

        LED_default = LoadMaterial("LED/LED_default");
        LED_red = LoadMaterial("LED/LED_red");
        LED_green = LoadMaterial("LED/LED_green");
        LED_yellow = LoadMaterial("LED/LED_yellow");
        LED_blue = LoadMaterial("LED/LED_blue");

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
