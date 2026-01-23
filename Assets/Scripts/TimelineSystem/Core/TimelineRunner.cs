using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TimelineSystem
{
    public enum PlaybackState
    {
        Stopped,
        Playing,
        Paused
    }

    [ExecuteInEditMode]
    public class TimelineRunner : MonoBehaviour
    {
        [Header("Timeline Configuration")]
        [SerializeReference]
        public List<TimelineStep> steps = new List<TimelineStep>();
        public bool playOnStart = false;
        public bool loop = false;

        [Header("Global Key Bindings")]
        public bool enableGlobalKeys = false;
        public KeyCode playKey = KeyCode.P;
        public KeyCode pauseKey = KeyCode.O;
        public KeyCode stopKey = KeyCode.I;
        public KeyCode skipKey = KeyCode.RightArrow;
        public KeyCode backKey = KeyCode.LeftArrow;

        [Header("Path Visualization")]
        public bool showPath = true;
        public bool useGradientColors = true;
        public Color pathColor = Color.cyan;
        public Gradient pathGradient;
        public LineRenderer pathLineRenderer;
        public bool updatePathInRuntime = true;

        [Header("Status (Read-Only)")]
        [SerializeField] private PlaybackState playbackState = PlaybackState.Stopped;
        [SerializeField] private int currentStepIndex = -1;
        [SerializeField] private Vector3 currentPosition;

        private StepExecutor stepExecutor;
        private Coroutine playbackCoroutine;

        public PlaybackState State => playbackState;
        public int CurrentStepIndex => currentStepIndex;
        public Vector3 CurrentPosition => currentPosition;

        private void Awake()
        {
            if (Application.isPlaying)
            {
                stepExecutor = gameObject.AddComponent<StepExecutor>();
            }

            // Initialize gradient with rainbow colors if not set
            if (pathGradient == null)
            {
                pathGradient = new Gradient();
                pathGradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(Color.red, 0.0f),
                        new GradientColorKey(Color.yellow, 0.25f),
                        new GradientColorKey(Color.green, 0.5f),
                        new GradientColorKey(Color.cyan, 0.75f),
                        new GradientColorKey(Color.blue, 1.0f)
                    },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                );
            }
        }

        private void Start()
        {
            if (Application.isPlaying && playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            // Update current position
            currentPosition = transform.position;

            // Handle global key bindings
            if (Application.isPlaying && enableGlobalKeys)
            {
                if (Input.GetKeyDown(playKey))
                {
                    if (playbackState == PlaybackState.Paused)
                        Resume();
                    else
                        Play();
                }
                if (Input.GetKeyDown(pauseKey))
                    Pause();
                if (Input.GetKeyDown(stopKey))
                    Stop();
                if (Input.GetKeyDown(skipKey))
                    SkipStep();
                if (Input.GetKeyDown(backKey))
                    BackStep();
            }

            // Update path visualization
            if (showPath && updatePathInRuntime && Application.isPlaying)
            {
                UpdatePathVisualization();
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                UpdatePathVisualization();
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdatePathVisualization();
            }
        }

        #region Playback Control

        public void Play()
        {
            if (playbackState == PlaybackState.Playing)
                return;

            if (steps == null || steps.Count == 0)
            {
                Debug.LogWarning("Timeline has no steps to play.");
                return;
            }

            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
            }

            playbackState = PlaybackState.Playing;
            playbackCoroutine = StartCoroutine(PlaybackRoutine());
        }

        public void Pause()
        {
            if (playbackState != PlaybackState.Playing)
                return;

            playbackState = PlaybackState.Paused;
            if (stepExecutor != null)
            {
                stepExecutor.Pause();
            }
        }

        public void Resume()
        {
            if (playbackState != PlaybackState.Paused)
                return;

            playbackState = PlaybackState.Playing;
            if (stepExecutor != null)
            {
                stepExecutor.Resume();
            }
        }

        public void Stop()
        {
            if (playbackState == PlaybackState.Stopped)
                return;

            playbackState = PlaybackState.Stopped;
            currentStepIndex = -1;

            if (stepExecutor != null)
            {
                stepExecutor.Stop();
            }

            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }
        }

        public void SkipStep()
        {
            if (playbackState != PlaybackState.Playing || stepExecutor == null)
                return;

            stepExecutor.Skip();
        }

        public void BackStep()
        {
            if (currentStepIndex <= 0)
                return;

            Stop();
            currentStepIndex -= 2; // Go back two because Play will increment
            if (currentStepIndex < -1)
                currentStepIndex = -1;
            Play();
        }

        public void JumpToStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= steps.Count)
            {
                Debug.LogWarning($"Invalid step index: {stepIndex}");
                return;
            }

            Stop();
            currentStepIndex = stepIndex - 1;
            Play();
        }

        #endregion

        #region Internal Playback

        private IEnumerator PlaybackRoutine()
        {
            // Start from next step or beginning
            if (currentStepIndex == -1)
            {
                currentStepIndex = 0;
            }
            else
            {
                currentStepIndex++;
            }

            while (currentStepIndex < steps.Count)
            {
                TimelineStep step = steps[currentStepIndex];

                if (step == null || !step.enabled)
                {
                    currentStepIndex++;
                    continue;
                }

                // Determine target object
                GameObject target = step.targetObject != null ? step.targetObject : gameObject;

                // Execute step
                yield return stepExecutor.ExecuteStep(step, target, OnStepExecuted);

                // Check if stopped during execution
                if (playbackState == PlaybackState.Stopped)
                {
                    yield break;
                }

                currentStepIndex++;
            }

            // Timeline complete
            if (loop)
            {
                currentStepIndex = -1;
                yield return PlaybackRoutine();
            }
            else
            {
                playbackState = PlaybackState.Stopped;
                currentStepIndex = -1;
            }
        }

        private void OnStepExecuted()
        {
            // Callback when a step completes
        }

        #endregion

        #region Path Visualization

        public void UpdatePathVisualization()
        {
            if (!showPath || steps == null || steps.Count == 0)
            {
                if (pathLineRenderer != null)
                {
                    pathLineRenderer.positionCount = 0;
                }
                return;
            }

            List<Vector3> pathPoints = new List<Vector3>();

            // Track per-object: last target position and whether we've added a starting point
            Dictionary<GameObject, bool> objectHasStartPoint = new Dictionary<GameObject, bool>();
            Dictionary<GameObject, Vector3> objectLastPos = new Dictionary<GameObject, Vector3>();
            Dictionary<GameObject, Quaternion> objectLastRot = new Dictionary<GameObject, Quaternion>();

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step != null && step.enabled && step is MoveStep moveStep)
                {
                    // Determine target object for this step
                    GameObject target = step.targetObject != null ? step.targetObject : gameObject;

                    // Add starting position if this is the first move step for this object
                    if (!objectHasStartPoint.ContainsKey(target) || !objectHasStartPoint[target])
                    {
                        Vector3 startPos = target.transform.position;
                        pathPoints.Add(startPos);
                        objectHasStartPoint[target] = true;
                        objectLastPos[target] = startPos;
                        objectLastRot[target] = target.transform.rotation;
                    }

                    Vector3 lastPos = objectLastPos[target];
                    Quaternion startRot = objectLastRot[target];

                    Vector3 targetPos = moveStep.targetPosition;

                    // Handle local vs world space
                    if (moveStep.space == SpaceType.Local && target != null)
                    {
                        targetPos = target.transform.parent != null
                            ? target.transform.parent.TransformPoint(targetPos)
                            : targetPos;
                    }
                    if (!moveStep.animatePosition)
                    {
                        targetPos = lastPos;
                    }

                    if (moveStep.animateRotation && moveStep.rotationPivotOffset != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.Euler(moveStep.targetRotation);
                        Vector3 pivotStart = lastPos + (startRot * moveStep.rotationPivotOffset);
                        Vector3 pivotEnd = targetPos + (targetRot * moveStep.rotationPivotOffset);
                        const int segments = 24;

                        for (int s = 1; s <= segments; s++)
                        {
                            float rawT = s / (float)segments;
                            float moveT = Easing.Ease(rawT, moveStep.easing);
                            float rotT = Easing.Ease(rawT, moveStep.rotationEasing);
                            Vector3 pivotPos = Vector3.Lerp(pivotStart, pivotEnd, moveT);
                            Quaternion rot = Quaternion.Lerp(startRot, targetRot, rotT);
                            Vector3 pos = pivotPos - (rot * moveStep.rotationPivotOffset);
                            pathPoints.Add(pos);
                        }

                        objectLastPos[target] = targetPos;
                    }
                    else
                    {
                        // Add the target position (continues from last position)
                        pathPoints.Add(targetPos);
                        objectLastPos[target] = targetPos;
                    }

                    if (moveStep.animateRotation)
                    {
                        objectLastRot[target] = Quaternion.Euler(moveStep.targetRotation);
                    }
                }
                // Delay and CustomAction steps don't affect the path
            }

            // Update LineRenderer
            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = pathPoints.Count;
                pathLineRenderer.SetPositions(pathPoints.ToArray());

                if (useGradientColors && pathGradient != null)
                {
                    // Use gradient for smooth color transitions
                    pathLineRenderer.colorGradient = pathGradient;
                }
                else
                {
                    // Use single color
                    pathLineRenderer.startColor = pathColor;
                    pathLineRenderer.endColor = pathColor;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showPath || steps == null || steps.Count == 0)
                return;

            // Track per-object: current drawing position
            Dictionary<GameObject, Vector3> objectCurrentPos = new Dictionary<GameObject, Vector3>();
            Dictionary<GameObject, bool> objectHasStart = new Dictionary<GameObject, bool>();
            Dictionary<GameObject, Quaternion> objectCurrentRot = new Dictionary<GameObject, Quaternion>();

            // Count enabled move steps for gradient calculation
            int enabledStepCount = 0;
            foreach (var s in steps)
            {
                if (s != null && s.enabled && s is MoveStep) enabledStepCount++;
            }

            int stepIndex = 0;
            for (int i = 0; i < steps.Count; i++)
            {
                TimelineStep step = steps[i];
                if (step != null && step.enabled && step is MoveStep moveStep)
                {
                    // Determine target object for this step
                    GameObject target = step.targetObject != null ? step.targetObject : gameObject;

                    // Initialize starting position for this object if first time
                    if (!objectHasStart.ContainsKey(target) || !objectHasStart[target])
                    {
                        Vector3 startPos = target.transform.position;
                        objectCurrentPos[target] = startPos;
                        objectHasStart[target] = true;
                        objectCurrentRot[target] = target.transform.rotation;

                        // Draw starting point indicator
                        if (useGradientColors && pathGradient != null)
                        {
                            Gizmos.color = pathGradient.Evaluate(0f);
                        }
                        else
                        {
                            Gizmos.color = pathColor;
                        }
                        Gizmos.DrawWireSphere(startPos, 0.3f);
                    }

                    // Get current position for this object (last target or start)
                    Vector3 currentPos = objectCurrentPos[target];
                    Quaternion currentRot = objectCurrentRot.ContainsKey(target) ? objectCurrentRot[target] : target.transform.rotation;

                    // Get target position
                    Vector3 targetPos = moveStep.targetPosition;

                    // Handle local vs world space
                    if (moveStep.space == SpaceType.Local && target != null)
                    {
                        targetPos = target.transform.parent != null
                            ? target.transform.parent.TransformPoint(targetPos)
                            : targetPos;
                    }
                    if (!moveStep.animatePosition)
                    {
                        targetPos = currentPos;
                    }
                    Quaternion targetRot = Quaternion.Euler(moveStep.targetRotation);
                    Vector3 finalPos = targetPos;

                    // Calculate gradient color based on step progress
                    float t = enabledStepCount > 1 ? (float)stepIndex / (enabledStepCount - 1) : 0f;

                    if (useGradientColors && pathGradient != null)
                    {
                        Gizmos.color = pathGradient.Evaluate(t);
                    }
                    else
                    {
                        Gizmos.color = pathColor;
                    }

                    // Draw line from current position to target position
                    Gizmos.DrawLine(currentPos, finalPos);

                    // Draw sphere at target
                    Gizmos.DrawWireSphere(finalPos, 0.2f);

                    // Draw step-specific gizmo
                    if (moveStep != null)
                    {
                        moveStep.SetPreviewTargetTransform(target.transform);
                        moveStep.SetPreviewStartRotation(currentRot);
                    }
                    step.DrawGizmos(currentPos);

                    // Update current position for this object
                    objectCurrentPos[target] = finalPos;
                    if (moveStep.animateRotation)
                    {
                        objectCurrentRot[target] = targetRot;
                    }
                    stepIndex++;
                }
                else if (step != null && step.enabled)
                {
                    // For Delay and CustomAction, draw gizmo at current position if available
                    GameObject target = step.targetObject != null ? step.targetObject : gameObject;
                    if (objectCurrentPos.ContainsKey(target))
                    {
                        step.DrawGizmos(objectCurrentPos[target]);
                    }
                }
            }
        }

        #endregion
    }
}
