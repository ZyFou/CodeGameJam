using UnityEngine;
using System;

namespace TimelineSystem
{
    [Serializable]
    public class MoveStep : TimelineStep
    {
        [Header("Movement Configuration")]
        public bool animatePosition = true;
        public Vector3 targetPosition = Vector3.zero;
        public SpaceType space = SpaceType.World;
        public float duration = 1f;
        public EasingType easing = EasingType.Linear;

        [Header("Shake (Optional)")]
        public bool useShake = false;
        public float shakeIntensity = 1f;
        public float shakeAmplitude = 0.1f;
        public float shakeFrequency = 20f;
        public Vector3 shakeAxis = Vector3.one;

        [Header("Optional Rotation")]
        public bool animateRotation = false;
        public Vector3 targetRotation = Vector3.zero;
        public EasingType rotationEasing = EasingType.Linear;
        public Vector3 rotationPivotOffset = Vector3.zero;
        [HideInInspector] public bool rotationPivotAffectsPosition = false;

        [Header("Optional Scale")]
        public bool animateScale = false;
        public Vector3 targetScale = Vector3.one;

        // Internal state
        private Vector3 startPosition;
        private Vector3 startLocalPosition;
        private Quaternion startRotation;
        private Vector3 startScale;
        private float elapsedTime;
        private Transform targetTransform;
        [NonSerialized] private Quaternion previewStartRotation = Quaternion.identity;
        [NonSerialized] private bool hasPreviewStartRotation = false;
        [NonSerialized] private Transform previewTargetTransform;
        private Vector3 shakeSeed;

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
            Transform localRoot = ResolveLocalSpaceRoot(targetTransform);
            if (localRoot != null)
            {
                startLocalPosition = localRoot.InverseTransformPoint(startPosition);
            }
            else
            {
                startLocalPosition = targetTransform.localPosition;
            }
            startRotation = targetTransform.rotation;
            startScale = targetTransform.localScale;
            shakeSeed = new Vector3(
                UnityEngine.Random.value * 1000f,
                UnityEngine.Random.value * 1000f,
                UnityEngine.Random.value * 1000f
            );
        }

        public void SetPreviewStartRotation(Quaternion rotation)
        {
            previewStartRotation = rotation;
            hasPreviewStartRotation = true;
        }

        public void ClearPreviewStartRotation()
        {
            hasPreviewStartRotation = false;
        }

        public void SetPreviewTargetTransform(Transform target)
        {
            previewTargetTransform = target;
        }

        public void ClearPreviewTargetTransform()
        {
            previewTargetTransform = null;
        }

        public override bool Update(float deltaTime)
        {
            if (targetTransform == null) return true;

            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Standard movement
            float easedT = Easing.Ease(t, easing);

            Transform localRoot = ResolveLocalSpaceRoot(targetTransform);
            bool useLocal = UseLocalSpace();
            Vector3 worldStartPosition = startPosition;
            Vector3 worldTargetPosition = targetPosition;
            if (useLocal && localRoot != null)
            {
                worldStartPosition = localRoot.TransformPoint(startLocalPosition);
                worldTargetPosition = localRoot.TransformPoint(targetPosition);
            }
            if (!animatePosition)
            {
                worldTargetPosition = worldStartPosition;
            }

            Quaternion currentRotation = startRotation;
            Quaternion targetQuat = startRotation;
            if (animateRotation)
            {
                float rotEasedT = Easing.Ease(t, rotationEasing);
                targetQuat = Quaternion.Euler(targetRotation);
                currentRotation = Quaternion.Lerp(startRotation, targetQuat, rotEasedT);
                targetTransform.rotation = currentRotation;
            }

            Vector3 basePosition = Vector3.Lerp(worldStartPosition, worldTargetPosition, easedT);
            if (animateRotation && rotationPivotOffset != Vector3.zero)
            {
                Vector3 pivotStart = worldStartPosition + (startRotation * rotationPivotOffset);
                Vector3 pivotEnd = worldTargetPosition + (targetQuat * rotationPivotOffset);
                Vector3 pivotPosition = Vector3.Lerp(pivotStart, pivotEnd, easedT);
                basePosition = pivotPosition - (currentRotation * rotationPivotOffset);
            }

            if ((useShake || easing == EasingType.Shake) && animatePosition)
            {
                float time = elapsedTime * Mathf.Max(0f, shakeFrequency);
                Vector3 noise = new Vector3(
                    Mathf.PerlinNoise(shakeSeed.x, time) * 2f - 1f,
                    Mathf.PerlinNoise(shakeSeed.y, time) * 2f - 1f,
                    Mathf.PerlinNoise(shakeSeed.z, time) * 2f - 1f
                );
                float amplitude = Mathf.Max(0f, shakeAmplitude);
                float intensity = Mathf.Clamp01(shakeIntensity);
                Vector3 axis = new Vector3(
                    Mathf.Clamp01(Mathf.Abs(shakeAxis.x)),
                    Mathf.Clamp01(Mathf.Abs(shakeAxis.y)),
                    Mathf.Clamp01(Mathf.Abs(shakeAxis.z))
                );
                basePosition += Vector3.Scale(noise, axis) * amplitude * intensity;
            }

            if (animatePosition || animateRotation)
            {
                targetTransform.position = basePosition;
            }

            // Lerp scale if enabled
            if (animateScale)
            {
                targetTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            // Return true when complete
            return t >= 1f;
        }

        public override void Complete()
        {
            if (targetTransform == null) return;

            // Snap to final values
            Transform localRoot = ResolveLocalSpaceRoot(targetTransform);
            bool useLocal = UseLocalSpace();
            Vector3 worldStartPosition = startPosition;
            Vector3 worldTargetPosition = targetPosition;
            if (useLocal && localRoot != null)
            {
                worldStartPosition = localRoot.TransformPoint(startLocalPosition);
                worldTargetPosition = localRoot.TransformPoint(targetPosition);
            }
            if (!animatePosition)
            {
                worldTargetPosition = worldStartPosition;
            }

            if (animateRotation)
            {
                Quaternion targetQuat = Quaternion.Euler(targetRotation);
                targetTransform.rotation = targetQuat;
                targetTransform.position = worldTargetPosition;
            }
            else
            {
                if (animatePosition)
                {
                    targetTransform.position = worldTargetPosition;
                }
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
            // Draw standard movement arrow
            Vector3 worldTarget = GetWorldTargetPosition();

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

            if (animateRotation)
            {
                Quaternion startRotation = GetPreviewStartRotation();
                Quaternion targetQuat = Quaternion.Euler(targetRotation);
                Vector3 pivotStart = currentPosition + (startRotation * rotationPivotOffset);
                Vector3 pivotEnd = worldTarget + (targetQuat * rotationPivotOffset);

                Gizmos.DrawWireSphere(pivotStart, 0.15f);
                Gizmos.DrawLine(currentPosition, pivotStart);

                Vector3 axis = (startRotation * Vector3.up).normalized;
                Gizmos.DrawLine(pivotStart - axis * 0.6f, pivotStart + axis * 0.6f);

                if ((pivotEnd - pivotStart).sqrMagnitude > 0.0001f)
                {
                    Gizmos.DrawWireSphere(pivotEnd, 0.12f);
                    Gizmos.DrawLine(pivotStart, pivotEnd);
                }

                if (rotationPivotOffset != Vector3.zero)
                {
                    Vector3 prev = pivotStart;
                    const int segments = 24;
                    for (int i = 1; i <= segments; i++)
                    {
                        float rawT = i / (float)segments;
                        float moveT = Easing.Ease(rawT, easing);
                        float rotT = Easing.Ease(rawT, rotationEasing);
                        Quaternion rot = Quaternion.Lerp(startRotation, targetQuat, rotT);
                        Vector3 pivotPos = Vector3.Lerp(pivotStart, pivotEnd, moveT);
                        Vector3 next = pivotPos;
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                }
            }
        }

        private Quaternion GetPreviewStartRotation()
        {
            if (hasPreviewStartRotation)
            {
                return previewStartRotation;
            }

            Transform target = GetPreviewTargetTransform();
            if (target != null)
            {
                return target.rotation;
            }

            return Quaternion.identity;
        }

        private Transform GetPreviewTargetTransform()
        {
            if (previewTargetTransform != null)
            {
                return previewTargetTransform;
            }

            if (targetObject != null)
            {
                return targetObject.transform;
            }

            return null;
        }

        private Vector3 GetWorldTargetPosition()
        {
            Vector3 worldTarget = targetPosition;
            Transform target = GetPreviewTargetTransform();

            bool useLocal = UseLocalSpace();
            Transform localRoot = ResolveLocalSpaceRoot(target);
            if (useLocal && localRoot != null)
            {
                worldTarget = localRoot.TransformPoint(targetPosition);
            }

            return worldTarget;
        }

        private bool UseLocalSpace()
        {
            return forceLocalSpace || space == SpaceType.Local;
        }

        private Transform ResolveLocalSpaceRoot(Transform target)
        {
            if (spaceRootOverride != null)
            {
                return spaceRootOverride;
            }

            if (target != null)
            {
                return target.parent;
            }

            return null;
        }
    }
}
