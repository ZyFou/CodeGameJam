using UnityEngine;
using System.Collections;

namespace TimelineSystem
{
    public class StepExecutor : MonoBehaviour
    {
        private TimelineStep currentStep;
        private GameObject currentTarget;
        private bool isExecuting = false;
        private bool isPaused = false;
        private bool skipRequested = false;

        public bool IsExecuting => isExecuting;
        public bool IsPaused => isPaused;

        public delegate void StepCompleteCallback();
        private StepCompleteCallback onStepComplete;

        public IEnumerator ExecuteStep(TimelineStep step, GameObject target, StepCompleteCallback callback)
        {
            if (step == null || target == null)
            {
                callback?.Invoke();
                yield break;
            }

            currentStep = step;
            currentTarget = target;
            onStepComplete = callback;
            isExecuting = true;
            isPaused = false;
            skipRequested = false;

            // Initialize the step
            step.Initialize(target);

            // Invoke start event
            step.onStepStart?.Invoke();

            // Wait for input condition if specified
            if (step.inputCondition != InputConditionType.None)
            {
                yield return WaitForInput(step);
            }

            // Execute step update loop
            while (isExecuting && !skipRequested)
            {
                // Handle pause
                while (isPaused && !skipRequested)
                {
                    yield return null;
                }

                if (skipRequested)
                {
                    break;
                }

                // Update step (returns true when complete)
                bool isComplete = step.Update(Time.deltaTime);

                if (isComplete)
                {
                    break;
                }

                yield return null;
            }

            // Complete the step
            step.Complete();
            step.onStepComplete?.Invoke();

            isExecuting = false;
            currentStep = null;
            currentTarget = null;

            onStepComplete?.Invoke();
        }

        private IEnumerator WaitForInput(TimelineStep step)
        {
            while (!step.CheckInputCondition())
            {
                // Handle pause during input wait
                while (isPaused)
                {
                    yield return null;
                }

                if (skipRequested)
                {
                    yield break;
                }

                yield return null;
            }
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
        }

        public void Skip()
        {
            skipRequested = true;
        }

        public void Stop()
        {
            if (isExecuting)
            {
                StopAllCoroutines();
                isExecuting = false;
                isPaused = false;
                skipRequested = false;
                currentStep = null;
                currentTarget = null;
            }
        }
    }
}
