using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Camera cam;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        ResourcesManager.LoadResources();
        ComponentsManager.CreateAllObjects();
    }
}
