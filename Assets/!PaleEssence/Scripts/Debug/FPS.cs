using UnityEngine;
using Unity.Profiling;
using System.Collections.Generic;

public class FPSProfiler : MonoBehaviour
{
    ProfilerRecorder cpuFrameTimeRecorder;
    ProfilerRecorder gpuFrameTimeRecorder;

    float deltaTime;

    Queue<float> fpsBuffer = new Queue<float>();
    float bufferTime = 5f;
    float accumulatedTime = 0f;

    void OnEnable()
    {
        cpuFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");

        gpuFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
    }

    void OnDisable()
    {
        cpuFrameTimeRecorder.Dispose();
        gpuFrameTimeRecorder.Dispose();
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        float currentFps = 1.0f / Time.unscaledDeltaTime;

        fpsBuffer.Enqueue(currentFps);
        accumulatedTime += Time.unscaledDeltaTime;

        while (accumulatedTime > bufferTime && fpsBuffer.Count > 0)
        {
            accumulatedTime -= Time.unscaledDeltaTime;
            fpsBuffer.Dequeue();
        }
    }

    float GetAverageFPS()
    {
        if (fpsBuffer.Count == 0) return 0f;
        float sum = 0f;
        foreach (var f in fpsBuffer) sum += f;
        return sum / fpsBuffer.Count;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h * 2 / 100);

        style.alignment = TextAnchor.UpperCenter;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.white;

        float fps = 1.0f / deltaTime;
        float avgFps = GetAverageFPS();

        double cpuTimeMs = cpuFrameTimeRecorder.Valid ?
            cpuFrameTimeRecorder.LastValue / 1000000.0 : 0;

        double gpuTimeMs = gpuFrameTimeRecorder.Valid ?
            gpuFrameTimeRecorder.LastValue / 1000000.0 : 0;

        string text =
            $"{fps:0.} FPS (Avg {avgFps:0.})\n" +
            $"CPU: {cpuTimeMs:0.0} ms | GPU: {gpuTimeMs:0.0} ms";

        GUI.Label(rect, text, style);
    }
}
