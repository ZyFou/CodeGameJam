using UnityEngine;
using UnityEngine.Events;
using System;

namespace TimelineSystem
{
    [Serializable]
    public class CustomActionStep : TimelineStep
    {
        [Header("Custom Action Configuration")]
        public UnityEvent customAction;
        public bool waitForCompletion = false;
        public float timeout = 0f; // 0 means instant, >0 means wait for timeout

        // Internal state
        private float elapsedTime;
        private bool actionInvoked;
        private Vector3 cachedPosition;

        public CustomActionStep() : base(StepType.CustomAction)
        {
            customAction = new UnityEvent();
        }

        public override void Initialize(GameObject target)
        {
            elapsedTime = 0f;
            actionInvoked = false;

            if (target != null)
            {
                cachedPosition = target.transform.position;
            }
        }

        public override bool Update(float deltaTime)
        {
            // Invoke action on first update
            if (!actionInvoked)
            {
                customAction?.Invoke();
                actionInvoked = true;

                // If not waiting for completion, complete immediately
                if (!waitForCompletion || timeout <= 0f)
                {
                    return true;
                }
            }

            // Wait for timeout if specified
            if (waitForCompletion && timeout > 0f)
            {
                elapsedTime += deltaTime;
                return elapsedTime >= timeout;
            }

            return true;
        }

        public override void Complete()
        {
            // Ensure action was invoked
            if (!actionInvoked)
            {
                customAction?.Invoke();
                actionInvoked = true;
            }
        }

        public override Vector3 GetTargetPosition()
        {
            // Custom action doesn't move, return cached position
            return cachedPosition;
        }

        public override void DrawGizmos(Vector3 currentPosition)
        {
            // Draw a gear/cog icon to represent custom action
            float radius = 0.3f;
            int teeth = 8;

            // Draw gear teeth
            for (int i = 0; i < teeth; i++)
            {
                float angle1 = (i / (float)teeth) * Mathf.PI * 2;
                float angle2 = ((i + 0.5f) / (float)teeth) * Mathf.PI * 2;
                float angle3 = ((i + 1f) / (float)teeth) * Mathf.PI * 2;

                Vector3 innerPoint1 = currentPosition + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius * 0.7f;
                Vector3 outerPoint = currentPosition + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
                Vector3 innerPoint2 = currentPosition + new Vector3(Mathf.Cos(angle3), Mathf.Sin(angle3), 0) * radius * 0.7f;

                Gizmos.DrawLine(innerPoint1, outerPoint);
                Gizmos.DrawLine(outerPoint, innerPoint2);
                if (i < teeth - 1 || teeth == 1)
                {
                    float nextAngle = ((i + 1f) / (float)teeth) * Mathf.PI * 2;
                    Vector3 nextInner = currentPosition + new Vector3(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle), 0) * radius * 0.7f;
                    Gizmos.DrawLine(innerPoint2, nextInner);
                }
            }

            // Draw center circle
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;

                Vector3 point1 = currentPosition + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius * 0.3f;
                Vector3 point2 = currentPosition + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius * 0.3f;

                Gizmos.DrawLine(point1, point2);
            }
        }
    }
}
