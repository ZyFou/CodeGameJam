using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonMover : MonoBehaviour
{
    [System.Serializable]
    public class MoveTarget
    {
        public RectTransform target;

        [Header("Movement")]
        public DirectionMode directionMode = DirectionMode.Up;
        public Vector2 customDirection = Vector2.up;
        public float distance = 200f;

        [Header("Timing")]
        public float delay = 0f;
        public float duration = 0.5f;

        [Header("Easing")]
        public EaseType easeType = EaseType.EaseInOut;
    }

    public List<MoveTarget> targets = new List<MoveTarget>();

    private bool hasPlayed = false;

    public void Play()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        foreach (MoveTarget target in targets)
        {
            if (target.target == null) continue;
            StartCoroutine(MoveRoutine(target));
        }
    }

    private IEnumerator MoveRoutine(MoveTarget data)
    {
        yield return new WaitForSeconds(data.delay);

        RectTransform rect = data.target;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 direction = GetDirection(data);
        Vector2 endPos = startPos + direction * data.distance;

        float time = 0f;

        while (time < data.duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / data.duration);
            float easedT = Ease(t, data.easeType);

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
            yield return null;
        }

        rect.anchoredPosition = endPos;
    }

    private Vector2 GetDirection(MoveTarget data)
    {
        switch (data.directionMode)
        {
            case DirectionMode.Up: return Vector2.up;
            case DirectionMode.Down: return Vector2.down;
            case DirectionMode.Left: return Vector2.left;
            case DirectionMode.Right: return Vector2.right;
            case DirectionMode.Custom: return data.customDirection.normalized;
            default: return Vector2.up;
        }
    }

    private float Ease(float t, EaseType ease)
    {
        switch (ease)
        {
            case EaseType.Linear:
                return t;

            case EaseType.EaseIn:
                return t * t;

            case EaseType.EaseOut:
                return 1f - Mathf.Pow(1f - t, 2f);

            case EaseType.EaseInOut:
                return t < 0.5f
                    ? 2f * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

            default:
                return t;
        }
    }

    public enum DirectionMode
    {
        Up,
        Down,
        Left,
        Right,
        Custom
    }

    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }
}