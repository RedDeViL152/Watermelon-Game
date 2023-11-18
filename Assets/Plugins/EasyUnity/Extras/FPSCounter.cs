using Sirenix.OdinInspector;

using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [Tooltip("How often (in seconds) the FPS will be recalculated")]
    public float updateInterval = 1;

    [Tooltip("FPS accumulated over the interval")]
    [ShowInInspector, ReadOnly] private float accum = 0;
    [Tooltip("Frames drawn over the interval")]
    [ShowInInspector, ReadOnly] private int frames = 0;
    [Tooltip("Left time for current interval and the last calculated fps")]
    [ShowInInspector, ReadOnly] private float timeleft = 0;

    [ShowInInspector, ReadOnly] public float FPS { get; private set; } = 0;

    void Update()
    {
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if ((timeleft += Time.deltaTime) >= updateInterval)
        {
            FPS = accum / frames;
            timeleft = accum = frames = 0;
        }
    }
}