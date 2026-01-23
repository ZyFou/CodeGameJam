using UnityEngine;

public class DayNightCycleSmooth : MonoBehaviour
{
    [Header("Step Settings")]
    [Tooltip("Degrees added on X axis per step")]
    public float degreesPerStep = 1f;

    [Tooltip("Time (in seconds) per step")]
    public float secondsPerStep = 1f;

    float currentX;
    float targetX;
    float stepTimer;

    void Start()
    {
        currentX = transform.localEulerAngles.x;
        targetX = currentX + degreesPerStep;
    }

    void Update()
    {
        stepTimer += Time.deltaTime;

        float t = Mathf.Clamp01(stepTimer / secondsPerStep);

        // Smooth interpolation
        float smoothX = Mathf.LerpAngle(currentX, targetX, t);
        Vector3 euler = transform.localEulerAngles;
        euler.x = smoothX;
        transform.localEulerAngles = euler;

        // Move to next step
        if (stepTimer >= secondsPerStep)
        {
            stepTimer = 0f;
            currentX = targetX;
            targetX = currentX + degreesPerStep;
        }
    }
}
