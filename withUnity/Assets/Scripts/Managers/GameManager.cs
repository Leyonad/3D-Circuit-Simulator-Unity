using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Camera cam;
    public static int tabItem = 0;
    public static float mapLimit = 12f;

    private void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        ResourcesManager.LoadResources();
        ComponentsManager.CreateAllObjects();
    }
}
