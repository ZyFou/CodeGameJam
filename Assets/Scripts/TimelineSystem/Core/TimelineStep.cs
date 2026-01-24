using UnityEngine;
using UnityEngine.Events;
using System;

namespace TimelineSystem
{
    [Serializable]
    public enum StepType
    {
        Move,
        Delay,
        CustomAction
    }

    [Serializable]
    public enum InputConditionType
    {
        None,
        MouseClick,
        KeyPress,
        CustomInput
    }

    [Serializable]
    public enum SpaceType
    {
        World,
        Local
    }

    [Serializable]
    public abstract class TimelineStep
    {
        [Header("Step Configuration")]
        public string stepName = "New Step";
        public StepType stepType;
        public bool enabled = true;

        [Header("Input Conditions")]
        public InputConditionType inputCondition = InputConditionType.None;
        public KeyCode inputKey = KeyCode.Space;
        public string customInputAxis = "";

        [Header("Events")]
        public UnityEvent onStepStart;
        public UnityEvent onStepComplete;

        // Reference to the target object for this step
        public GameObject targetObject;
        [NonSerialized] public bool forceLocalSpace = false;
        [NonSerialized] public Transform spaceRootOverride;

        protected TimelineStep(StepType type)
        {
            stepType = type;
            onStepStart = new UnityEvent();
            onStepComplete = new UnityEvent();
        }

        // Check if input condition is met
        public virtual bool CheckInputCondition()
        {
            switch (inputCondition)
            {
                case InputConditionType.None:
                    return true;
                case InputConditionType.MouseClick:
                    return Input.GetMouseButtonDown(0);
                case InputConditionType.KeyPress:
                    return Input.GetKeyDown(inputKey);
                case InputConditionType.CustomInput:
                    return !string.IsNullOrEmpty(customInputAxis) && Input.GetButtonDown(customInputAxis);
                default:
                    return true;
            }
        }

        // Abstract method to be implemented by derived classes
        public abstract void Initialize(GameObject target);
        public abstract bool Update(float deltaTime);
        public abstract void Complete();
        public abstract Vector3 GetTargetPosition();
        public abstract void DrawGizmos(Vector3 currentPosition);
    }
}
