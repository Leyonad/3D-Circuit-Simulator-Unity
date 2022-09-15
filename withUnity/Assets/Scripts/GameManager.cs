using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        ResourcesManager.LoadResources();
        ComponentsManager.CreateAllObjects();
    }
}
