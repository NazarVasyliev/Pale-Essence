using UnityEngine;

public class TargetFPS : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90; // или -1, но лучше явно 60
    }

}
