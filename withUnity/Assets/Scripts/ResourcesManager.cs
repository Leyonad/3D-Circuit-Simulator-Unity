using UnityEngine;

public class ResourcesManager
{
    public static Material wireMaterial;
    public static Material highlightWireMaterial;
    public static Material metalMaterial;
    public static Material red;
    public static Material blue;
    public static Material breadboardMaterial;

    public static void LoadResources()
    {
        wireMaterial = LoadMaterial("Wire_Material");
        highlightWireMaterial = LoadMaterial("Highlight_Wire_Material");
        metalMaterial = LoadMaterial("Breadboard_Metal_Normal_Material");
        red = LoadMaterial("Red");
        blue = LoadMaterial("Blue");
        breadboardMaterial = LoadMaterial("Breadboard_Material");
    }

    private static Material LoadMaterial(string name)
    {
        Material material = Resources.Load($"Materials/{name}", typeof(Material)) as Material;
        if (material == null)
            Debug.Log($"{name} -> Material not found!");
        return material;
    }
}