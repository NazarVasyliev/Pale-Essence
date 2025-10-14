using UnityEngine;

public class LockFPS : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 0; // Vsync

        Application.targetFrameRate = 90; // FPS lock
    }
}
