using UnityEngine;

public class FPSLimitation : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 80;
    }
}
