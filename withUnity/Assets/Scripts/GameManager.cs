using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        ResourcesManager.LoadResources();
        ComponentsManager.CreateAllObjects();
    }
}
