using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Camera cam;
    public static int tabItem = 0;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        ResourcesManager.LoadResources();
        ComponentsManager.CreateAllObjects();
    }
}
