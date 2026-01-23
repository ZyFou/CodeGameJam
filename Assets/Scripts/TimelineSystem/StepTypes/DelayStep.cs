using UnityEngine;
using System;

namespace TimelineSystem
{
    [Serializable]
    public class DelayStep : TimelineStep
    {
        [Header("Delay Configuration")]
        public float duration = 1f;

        // Internal state
        private float elapsedTime;
        private Vector3 cachedPosition;

        public DelayStep() : base(StepType.Delay)
        {
        }

        public override void Initialize(GameObject target)
        {
            elapsedTime = 0f;
            if (target != null)
            {
                cachedPosition = target.transform.position;
            }
        }

        public override bool Update(float deltaTime)
        {
            elapsedTime += deltaTime;
            return elapsedTime >= duration;
        }

        public override void Complete()
        {
            // Nothing to do on complete for delay
        }

        public override Vector3 GetTargetPosition()
        {
            // Delay doesn't move, return cached position or zero
            return cachedPosition;
        }

        public override void DrawGizmos(Vector3 currentPosition)
        {
            // Draw a small indicator for delay (clock icon approximation)
            float radius = 0.3f;
            int segments = 16;

            // Draw circle
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;

                Vector3 point1 = currentPosition + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 point2 = currentPosition + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;

                Gizmos.DrawLine(point1, point2);
            }

            // Draw clock hands
            Gizmos.DrawLine(currentPosition, currentPosition + new Vector3(0, radius * 0.5f, 0));
            Gizmos.DrawLine(currentPosition, currentPosition + new Vector3(radius * 0.3f, 0, 0));
        }
    }
}
