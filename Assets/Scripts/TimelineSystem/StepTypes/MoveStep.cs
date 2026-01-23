using UnityEngine;
using System;

namespace TimelineSystem
{
    [Serializable]
    public class MoveStep : TimelineStep
    {
        [Header("Movement Configuration")]
        public Vector3 targetPosition = Vector3.zero;
        public SpaceType space = SpaceType.World;
        public float duration = 1f;
        public EasingType easing = EasingType.Linear;

        [Header("Optional Rotation")]
        public bool animateRotation = false;
        public Vector3 targetRotation = Vector3.zero;

        [Header("Optional Scale")]
        public bool animateScale = false;
        public Vector3 targetScale = Vector3.one;

        // Internal state
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 startScale;
        private float elapsedTime;
        private Transform targetTransform;

        public MoveStep() : base(StepType.Move)
        {
        }

        public override void Initialize(GameObject target)
        {
            if (target == null) return;

            targetTransform = target.transform;
            elapsedTime = 0f;

            // Store starting values
            startPosition = targetTransform.position;
            startRotation = targetTransform.rotation;
            startScale = targetTransform.localScale;
        }

        public override bool Update(float deltaTime)
        {
            if (targetTransform == null) return true;

            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = Easing.Ease(t, easing);

            // Calculate target position in world space
            Vector3 worldTargetPosition = targetPosition;
            if (space == SpaceType.Local && targetTransform.parent != null)
            {
                worldTargetPosition = targetTransform.parent.TransformPoint(targetPosition);
            }

            // Lerp position
            targetTransform.position = Vector3.Lerp(startPosition, worldTargetPosition, easedT);

            // Lerp rotation if enabled
            if (animateRotation)
            {
                Quaternion targetQuat = Quaternion.Euler(targetRotation);
                targetTransform.rotation = Quaternion.Lerp(startRotation, targetQuat, easedT);
            }

            // Lerp scale if enabled
            if (animateScale)
            {
                targetTransform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
            }

            // Return true when complete
            return t >= 1f;
        }

        public override void Complete()
        {
            if (targetTransform == null) return;

            // Snap to final values
            Vector3 worldTargetPosition = targetPosition;
            if (space == SpaceType.Local && targetTransform.parent != null)
            {
                worldTargetPosition = targetTransform.parent.TransformPoint(targetPosition);
            }

            targetTransform.position = worldTargetPosition;

            if (animateRotation)
            {
                targetTransform.rotation = Quaternion.Euler(targetRotation);
            }

            if (animateScale)
            {
                targetTransform.localScale = targetScale;
            }
        }

        public override Vector3 GetTargetPosition()
        {
            return targetPosition;
        }

        public override void DrawGizmos(Vector3 currentPosition)
        {
            // Draw arrow indicating movement direction
            Vector3 worldTarget = targetPosition;

            // Note: In DrawGizmos we don't have access to targetTransform,
            // so we can't properly convert local to world space here.
            // The conversion is handled in TimelineRunner.OnDrawGizmos

            Vector3 direction = worldTarget - currentPosition;
            if (direction.magnitude > 0.1f)
            {
                // Draw direction arrow
                Gizmos.DrawLine(currentPosition, worldTarget);

                // Draw arrowhead
                Vector3 arrowDir = direction.normalized;
                Vector3 right = Vector3.Cross(Vector3.up, arrowDir).normalized;
                Vector3 arrowPoint1 = worldTarget - arrowDir * 0.3f + right * 0.15f;
                Vector3 arrowPoint2 = worldTarget - arrowDir * 0.3f - right * 0.15f;

                Gizmos.DrawLine(worldTarget, arrowPoint1);
                Gizmos.DrawLine(worldTarget, arrowPoint2);
            }
        }
    }
}
