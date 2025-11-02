using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    public int fps_limit = 60;

    void Awake()
    {
        Application.targetFrameRate = fps_limit;
    }
}
