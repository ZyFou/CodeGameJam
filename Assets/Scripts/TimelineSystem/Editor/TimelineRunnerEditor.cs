using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace TimelineSystem
{
    [CustomEditor(typeof(TimelineRunner))]
    public class TimelineRunnerEditor : Editor
    {
        private enum EaseFamily
        {
            Linear,
            In,
            Out,
            InOut,
            Bounce,
            Elastic,
            Back,
            Shake
        }

        private enum EaseCurve
        {
            Quad,
            Cubic,
            Quart,
            Sine,
            Expo,
            Circ
        }

        private enum EaseDirection
        {
            In,
            Out,
            InOut
        }

        private ReorderableList stepsList;
        private TimelineRunner timelineRunner;
        private SerializedProperty stepsProperty;
        private Dictionary<int, GameObject> previousTargetObjects = new Dictionary<int, GameObject>();
        private SerializedProperty playOnStartProperty;
        private SerializedProperty loopProperty;
        private SerializedProperty enableGlobalKeysProperty;
        private SerializedProperty playKeyProperty;
        private SerializedProperty pauseKeyProperty;
        private SerializedProperty stopKeyProperty;
        private SerializedProperty skipKeyProperty;
        private SerializedProperty backKeyProperty;
        private SerializedProperty showPathProperty;
        private SerializedProperty useGradientColorsProperty;
        private SerializedProperty pathColorProperty;
        private SerializedProperty pathGradientProperty;
        private SerializedProperty pathLineRendererProperty;
        private SerializedProperty updatePathInRuntimeProperty;
        private SerializedProperty forceLocalSpaceProperty;
        private SerializedProperty localSpaceRootProperty;

        private void OnEnable()
        {
            timelineRunner = (TimelineRunner)target;

            stepsProperty = serializedObject.FindProperty("steps");
            playOnStartProperty = serializedObject.FindProperty("playOnStart");
            loopProperty = serializedObject.FindProperty("loop");
            enableGlobalKeysProperty = serializedObject.FindProperty("enableGlobalKeys");
            playKeyProperty = serializedObject.FindProperty("playKey");
            pauseKeyProperty = serializedObject.FindProperty("pauseKey");
            stopKeyProperty = serializedObject.FindProperty("stopKey");
            skipKeyProperty = serializedObject.FindProperty("skipKey");
            backKeyProperty = serializedObject.FindProperty("backKey");
            showPathProperty = serializedObject.FindProperty("showPath");
            useGradientColorsProperty = serializedObject.FindProperty("useGradientColors");
            pathColorProperty = serializedObject.FindProperty("pathColor");
            pathGradientProperty = serializedObject.FindProperty("pathGradient");
            pathLineRendererProperty = serializedObject.FindProperty("pathLineRenderer");
            updatePathInRuntimeProperty = serializedObject.FindProperty("updatePathInRuntime");
            forceLocalSpaceProperty = serializedObject.FindProperty("forceLocalSpace");
            localSpaceRootProperty = serializedObject.FindProperty("localSpaceRoot");

            SetupReorderableList();
        }

        private void SetupReorderableList()
        {
            stepsList = new ReorderableList(serializedObject, stepsProperty, true, true, true, true);

            stepsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Timeline Steps", EditorStyles.boldLabel);
            };

            stepsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= timelineRunner.steps.Count) return;

                var step = timelineRunner.steps[index];
                if (step == null) return;

                rect.y += 2;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = 2;
                float currentY = rect.y;

                // Step header with fold and enabled toggle
                Rect foldRect = new Rect(rect.x, currentY, rect.width - 60, lineHeight);
                Rect enabledRect = new Rect(rect.x + rect.width - 55, currentY, 50, lineHeight);

                step.enabled = EditorGUI.ToggleLeft(enabledRect, "On", step.enabled);

                EditorGUI.LabelField(foldRect, $"[{index}] {step.stepName} ({step.stepType})", EditorStyles.boldLabel);
                currentY += lineHeight + spacing;

                // Step Name
                Rect nameRect = new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight);
                step.stepName = EditorGUI.TextField(nameRect, "Step Name", step.stepName);
                currentY += lineHeight + spacing;

                // Step Type (readonly display, change requires recreation)
                Rect typeRect = new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight);
                EditorGUI.LabelField(typeRect, "Step Type", step.stepType.ToString());
                currentY += lineHeight + spacing;

                // Target Object
                Rect targetRect = new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight);
                GameObject newTarget = (GameObject)EditorGUI.ObjectField(targetRect, "Target Object", step.targetObject, typeof(GameObject), true);

                // Detect target object change and auto-fill values for MoveStep
                if (newTarget != step.targetObject && newTarget != null && step is MoveStep moveStep)
                {
                    bool useLocal = ShouldUseLocalSpaceForEditor(moveStep);
                    Transform localRoot = ResolveLocalSpaceRootForEditor(newTarget);
                    if (useLocal && localRoot != null)
                    {
                        moveStep.targetPosition = localRoot.InverseTransformPoint(newTarget.transform.position);
                    }
                    else
                    {
                        moveStep.targetPosition = newTarget.transform.position;
                    }
                    moveStep.targetRotation = newTarget.transform.rotation.eulerAngles;
                    moveStep.targetScale = newTarget.transform.localScale;
                    timelineRunner.UpdatePathVisualization();
                    EditorUtility.SetDirty(timelineRunner);
                }

                step.targetObject = newTarget;
                currentY += lineHeight + spacing;

                // Current Position (readonly)
                if (step.targetObject != null)
                {
                    Rect posRect = new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.Vector3Field(posRect, "Current Position", step.targetObject.transform.position);
                    EditorGUI.EndDisabledGroup();
                    currentY += lineHeight + spacing;
                }

                EditorGUI.LabelField(new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight), "Input Conditions", EditorStyles.boldLabel);
                currentY += lineHeight + spacing;

                // Input Condition
                Rect inputCondRect = new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight);
                step.inputCondition = (InputConditionType)EditorGUI.EnumPopup(inputCondRect, "Input Condition", step.inputCondition);
                currentY += lineHeight + spacing;

                if (step.inputCondition == InputConditionType.KeyPress)
                {
                    Rect keyRect = new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight);
                    step.inputKey = (KeyCode)EditorGUI.EnumPopup(keyRect, "Input Key", step.inputKey);
                    currentY += lineHeight + spacing;
                }
                else if (step.inputCondition == InputConditionType.CustomInput)
                {
                    Rect axisRect = new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight);
                    step.customInputAxis = EditorGUI.TextField(axisRect, "Input Axis", step.customInputAxis);
                    currentY += lineHeight + spacing;
                }

                // Type-specific fields
                currentY = DrawStepTypeFields(step, rect.x, currentY, rect.width, lineHeight, spacing);

                currentY += spacing * 2;
                // Events section
                EditorGUI.LabelField(new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight), "Events", EditorStyles.boldLabel);
                currentY += lineHeight + spacing;

                // Note: Unity Events need SerializedProperty to work properly in custom inspectors
                // For now, we'll add a button to edit them in the standard inspector
                if (GUI.Button(new Rect(rect.x + 15, currentY, rect.width - 20, lineHeight), "Edit Events (See Below)"))
                {
                    // Events are handled in the standard property drawer below
                }
                currentY += lineHeight + spacing;
            };

            stepsList.elementHeightCallback = (int index) =>
            {
                if (index >= timelineRunner.steps.Count) return EditorGUIUtility.singleLineHeight;

                var step = timelineRunner.steps[index];
                if (step == null) return EditorGUIUtility.singleLineHeight;

                float height = EditorGUIUtility.singleLineHeight * 2; // Header + spacing
                height += EditorGUIUtility.singleLineHeight * 2; // Name + Type
                height += EditorGUIUtility.singleLineHeight * 2; // Target + Current Position (if has target)

                if (step.targetObject != null)
                {
                    height += EditorGUIUtility.singleLineHeight;
                }

                height += EditorGUIUtility.singleLineHeight * 2; // Input section header + condition

                if (step.inputCondition == InputConditionType.KeyPress || step.inputCondition == InputConditionType.CustomInput)
                {
                    height += EditorGUIUtility.singleLineHeight;
                }

                // Add type-specific height
                height += GetStepTypeFieldsHeight(step);

                // Events section
                height += EditorGUIUtility.singleLineHeight * 2; // Header + button
                height += EditorGUIUtility.singleLineHeight; // Extra spacing

                height += 30; // Bottom padding

                return height;
            };

            stepsList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Move Step"), false, () => AddStep(StepType.Move));
                menu.AddItem(new GUIContent("Delay Step"), false, () => AddStep(StepType.Delay));
                menu.AddItem(new GUIContent("Custom Action Step"), false, () => AddStep(StepType.CustomAction));
                menu.ShowAsContext();
            };

            stepsList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("Remove Step",
                    "Are you sure you want to remove this step?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    timelineRunner.UpdatePathVisualization();
                }
            };

            stepsList.onReorderCallback = (ReorderableList list) =>
            {
                timelineRunner.UpdatePathVisualization();
            };
        }

        private float DrawStepTypeFields(TimelineStep step, float x, float y, float width, float lineHeight, float spacing)
        {
            float currentY = y;

            EditorGUI.LabelField(new Rect(x + 15, currentY, width - 20, lineHeight), "Step Configuration", EditorStyles.boldLabel);
            currentY += lineHeight + spacing;

            if (step is MoveStep moveStep)
            {
                // Move step fields
                Rect durationRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.duration = EditorGUI.FloatField(durationRect, "Duration", moveStep.duration);
                currentY += lineHeight + spacing;

                Rect animPosRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.animatePosition = EditorGUI.Toggle(animPosRect, "Animate Position", moveStep.animatePosition);
                currentY += lineHeight + spacing;

                if (moveStep.animatePosition)
                {
                    Rect posRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.targetPosition = EditorGUI.Vector3Field(posRect, "Target Position", moveStep.targetPosition);
                    currentY += lineHeight + spacing;

                    Rect spaceRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.space = (SpaceType)EditorGUI.EnumPopup(spaceRect, "Space", moveStep.space);
                    currentY += lineHeight + spacing;
                }

                if (moveStep.animatePosition)
                {
                    Rect easingRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.easing = DrawEasingSelector(easingRect, "Position Easing", moveStep.easing, false);
                    currentY += lineHeight + spacing;

                    Rect easePreviewRect = new Rect(x + 15, currentY, width - 20, lineHeight * 2f);
                    DrawEasingPreview(easePreviewRect, moveStep.easing);
                    currentY += easePreviewRect.height + spacing;

                    Rect shakeToggleRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.useShake = EditorGUI.Toggle(shakeToggleRect, "Use Shake", moveStep.useShake);
                    currentY += lineHeight + spacing;

                    if (moveStep.useShake)
                    {
                        Rect shakeIntensityRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                        moveStep.shakeIntensity = EditorGUI.Slider(shakeIntensityRect, "Shake Intensity", moveStep.shakeIntensity, 0f, 1f);
                        currentY += lineHeight + spacing;

                        Rect shakeAmplitudeRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                        moveStep.shakeAmplitude = EditorGUI.FloatField(shakeAmplitudeRect, "Max Amplitude", moveStep.shakeAmplitude);
                        currentY += lineHeight + spacing;

                        Rect shakeFrequencyRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                        moveStep.shakeFrequency = EditorGUI.FloatField(shakeFrequencyRect, "Shake Frequency", moveStep.shakeFrequency);
                        currentY += lineHeight + spacing;

                        Rect shakeAxisRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                        moveStep.shakeAxis = EditorGUI.Vector3Field(shakeAxisRect, "Shake Axis", moveStep.shakeAxis);
                        currentY += lineHeight + spacing;
                    }
                }

                Rect animRotRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.animateRotation = EditorGUI.Toggle(animRotRect, "Animate Rotation", moveStep.animateRotation);
                currentY += lineHeight + spacing;

                if (moveStep.animateRotation)
                {
                    Rect rotRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.targetRotation = EditorGUI.Vector3Field(rotRect, "Target Rotation", moveStep.targetRotation);
                    currentY += lineHeight + spacing;

                    Rect rotEasingRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.rotationEasing = DrawEasingSelector(rotEasingRect, "Rotation Easing", moveStep.rotationEasing, false);
                    currentY += lineHeight + spacing;

                    Rect rotPreviewRect = new Rect(x + 15, currentY, width - 20, lineHeight * 2f);
                    DrawEasingPreview(rotPreviewRect, moveStep.rotationEasing);
                    currentY += rotPreviewRect.height + spacing;

                    Rect rotPivotRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.rotationPivotOffset = EditorGUI.Vector3Field(rotPivotRect, "Rotation Pivot Offset", moveStep.rotationPivotOffset);
                    currentY += lineHeight + spacing;
                }

                Rect animScaleRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.animateScale = EditorGUI.Toggle(animScaleRect, "Animate Scale", moveStep.animateScale);
                currentY += lineHeight + spacing;

                if (moveStep.animateScale)
                {
                    Rect scaleRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.targetScale = EditorGUI.Vector3Field(scaleRect, "Target Scale", moveStep.targetScale);
                    currentY += lineHeight + spacing;
                }
            }
            else if (step is DelayStep delayStep)
            {
                // Delay step fields
                Rect durationRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                delayStep.duration = EditorGUI.FloatField(durationRect, "Duration", delayStep.duration);
                currentY += lineHeight + spacing;
            }
            else if (step is CustomActionStep actionStep)
            {
                // Custom action fields
                Rect waitRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                actionStep.waitForCompletion = EditorGUI.Toggle(waitRect, "Wait For Completion", actionStep.waitForCompletion);
                currentY += lineHeight + spacing;

                if (actionStep.waitForCompletion)
                {
                    Rect timeoutRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    actionStep.timeout = EditorGUI.FloatField(timeoutRect, "Timeout", actionStep.timeout);
                    currentY += lineHeight + spacing;
                }

                EditorGUI.LabelField(new Rect(x + 15, currentY, width - 20, lineHeight), "Custom Action (Edit Below)");
                currentY += lineHeight + spacing;
            }

            return currentY;
        }

        private float GetStepTypeFieldsHeight(TimelineStep step)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float height = lineHeight; // Header

            if (step is MoveStep moveStep)
            {
                float previewHeight = lineHeight * 2f;
                height += lineHeight; // duration
                height += lineHeight; // animatePosition toggle
                if (moveStep.animatePosition)
                {
                    height += lineHeight * 2; // position, space
                    height += lineHeight + previewHeight; // easing + preview
                    height += lineHeight; // shake toggle
                    if (moveStep.useShake) height += lineHeight * 4; // shake params
                }
                if (moveStep.animateRotation) height += lineHeight * 3 + previewHeight; // targetRotation + rotationEasing + preview + rotationPivotOffset
                if (moveStep.animateScale) height += lineHeight;
            }
            else if (step is DelayStep)
            {
                height += lineHeight; // duration
            }
            else if (step is CustomActionStep actionStep)
            {
                height += lineHeight * 2; // wait toggle + label
                if (actionStep.waitForCompletion) height += lineHeight; // timeout
            }

            return height;
        }

        private EasingType DrawEasingSelector(Rect rect, string label, EasingType current, bool allowShake)
        {
            DecodeEasing(current, out EaseFamily family, out EaseCurve curve, out EaseDirection direction);

            if (!allowShake && family == EaseFamily.Shake)
            {
                family = EaseFamily.Linear;
            }

            Rect contentRect = EditorGUI.PrefixLabel(rect, new GUIContent(label));
            float halfWidth = (contentRect.width - 4f) * 0.5f;
            Rect leftRect = new Rect(contentRect.x, contentRect.y, halfWidth, contentRect.height);
            Rect rightRect = new Rect(contentRect.x + halfWidth + 4f, contentRect.y, halfWidth, contentRect.height);

            family = (EaseFamily)EditorGUI.EnumPopup(leftRect, family);

            if (family == EaseFamily.Linear)
            {
                return EasingType.Linear;
            }

            if (family == EaseFamily.Shake)
            {
                return allowShake ? EasingType.Shake : EasingType.Linear;
            }

            if (family == EaseFamily.Bounce || family == EaseFamily.Elastic || family == EaseFamily.Back)
            {
                direction = (EaseDirection)EditorGUI.EnumPopup(rightRect, direction);
                return MapSpecialEasing(family, direction);
            }

            curve = (EaseCurve)EditorGUI.EnumPopup(rightRect, curve);
            EaseDirection easeDirection = FamilyToDirection(family);
            return MapStandardEasing(easeDirection, curve);
        }

        private void DrawEasingPreview(Rect rect, EasingType type)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), new Color(0.25f, 0.25f, 0.25f, 1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(0.25f, 0.25f, 0.25f, 1f));

            const int samples = 32;
            Vector3 prev = Vector3.zero;
            for (int i = 0; i <= samples; i++)
            {
                float t = i / (float)samples;
                float eased = Mathf.Clamp01(Easing.Ease(t, type));
                float x = Mathf.Lerp(rect.x + 2f, rect.xMax - 2f, t);
                float y = Mathf.Lerp(rect.yMax - 2f, rect.y + 2f, eased);
                Vector3 point = new Vector3(x, y, 0f);

                if (i > 0)
                {
                    Handles.color = new Color(0.6f, 0.9f, 1f, 1f);
                    Handles.DrawAAPolyLine(2f, prev, point);
                }

                prev = point;
            }

            if (type == EasingType.Shake)
            {
                EditorGUI.LabelField(rect, "Shake", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void DecodeEasing(EasingType type, out EaseFamily family, out EaseCurve curve, out EaseDirection direction)
        {
            family = EaseFamily.Linear;
            curve = EaseCurve.Quad;
            direction = EaseDirection.InOut;

            switch (type)
            {
                case EasingType.Linear:
                    family = EaseFamily.Linear;
                    return;
                case EasingType.Shake:
                    family = EaseFamily.Shake;
                    return;
                case EasingType.EaseInQuad:
                    family = EaseFamily.In; curve = EaseCurve.Quad; direction = EaseDirection.In; return;
                case EasingType.EaseOutQuad:
                    family = EaseFamily.Out; curve = EaseCurve.Quad; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutQuad:
                    family = EaseFamily.InOut; curve = EaseCurve.Quad; direction = EaseDirection.InOut; return;
                case EasingType.EaseInCubic:
                    family = EaseFamily.In; curve = EaseCurve.Cubic; direction = EaseDirection.In; return;
                case EasingType.EaseOutCubic:
                    family = EaseFamily.Out; curve = EaseCurve.Cubic; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutCubic:
                    family = EaseFamily.InOut; curve = EaseCurve.Cubic; direction = EaseDirection.InOut; return;
                case EasingType.EaseInQuart:
                    family = EaseFamily.In; curve = EaseCurve.Quart; direction = EaseDirection.In; return;
                case EasingType.EaseOutQuart:
                    family = EaseFamily.Out; curve = EaseCurve.Quart; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutQuart:
                    family = EaseFamily.InOut; curve = EaseCurve.Quart; direction = EaseDirection.InOut; return;
                case EasingType.EaseInSine:
                    family = EaseFamily.In; curve = EaseCurve.Sine; direction = EaseDirection.In; return;
                case EasingType.EaseOutSine:
                    family = EaseFamily.Out; curve = EaseCurve.Sine; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutSine:
                    family = EaseFamily.InOut; curve = EaseCurve.Sine; direction = EaseDirection.InOut; return;
                case EasingType.EaseInExpo:
                    family = EaseFamily.In; curve = EaseCurve.Expo; direction = EaseDirection.In; return;
                case EasingType.EaseOutExpo:
                    family = EaseFamily.Out; curve = EaseCurve.Expo; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutExpo:
                    family = EaseFamily.InOut; curve = EaseCurve.Expo; direction = EaseDirection.InOut; return;
                case EasingType.EaseInCirc:
                    family = EaseFamily.In; curve = EaseCurve.Circ; direction = EaseDirection.In; return;
                case EasingType.EaseOutCirc:
                    family = EaseFamily.Out; curve = EaseCurve.Circ; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutCirc:
                    family = EaseFamily.InOut; curve = EaseCurve.Circ; direction = EaseDirection.InOut; return;
                case EasingType.EaseInBack:
                    family = EaseFamily.Back; direction = EaseDirection.In; return;
                case EasingType.EaseOutBack:
                    family = EaseFamily.Back; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutBack:
                    family = EaseFamily.Back; direction = EaseDirection.InOut; return;
                case EasingType.EaseInElastic:
                    family = EaseFamily.Elastic; direction = EaseDirection.In; return;
                case EasingType.EaseOutElastic:
                    family = EaseFamily.Elastic; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutElastic:
                    family = EaseFamily.Elastic; direction = EaseDirection.InOut; return;
                case EasingType.EaseInBounce:
                    family = EaseFamily.Bounce; direction = EaseDirection.In; return;
                case EasingType.EaseOutBounce:
                    family = EaseFamily.Bounce; direction = EaseDirection.Out; return;
                case EasingType.EaseInOutBounce:
                    family = EaseFamily.Bounce; direction = EaseDirection.InOut; return;
            }
        }

        private EaseDirection FamilyToDirection(EaseFamily family)
        {
            switch (family)
            {
                case EaseFamily.In: return EaseDirection.In;
                case EaseFamily.Out: return EaseDirection.Out;
                case EaseFamily.InOut: return EaseDirection.InOut;
                default: return EaseDirection.InOut;
            }
        }

        private EasingType MapStandardEasing(EaseDirection direction, EaseCurve curve)
        {
            switch (curve)
            {
                case EaseCurve.Quad:
                    return direction == EaseDirection.In ? EasingType.EaseInQuad
                        : direction == EaseDirection.Out ? EasingType.EaseOutQuad
                        : EasingType.EaseInOutQuad;
                case EaseCurve.Cubic:
                    return direction == EaseDirection.In ? EasingType.EaseInCubic
                        : direction == EaseDirection.Out ? EasingType.EaseOutCubic
                        : EasingType.EaseInOutCubic;
                case EaseCurve.Quart:
                    return direction == EaseDirection.In ? EasingType.EaseInQuart
                        : direction == EaseDirection.Out ? EasingType.EaseOutQuart
                        : EasingType.EaseInOutQuart;
                case EaseCurve.Sine:
                    return direction == EaseDirection.In ? EasingType.EaseInSine
                        : direction == EaseDirection.Out ? EasingType.EaseOutSine
                        : EasingType.EaseInOutSine;
                case EaseCurve.Expo:
                    return direction == EaseDirection.In ? EasingType.EaseInExpo
                        : direction == EaseDirection.Out ? EasingType.EaseOutExpo
                        : EasingType.EaseInOutExpo;
                case EaseCurve.Circ:
                    return direction == EaseDirection.In ? EasingType.EaseInCirc
                        : direction == EaseDirection.Out ? EasingType.EaseOutCirc
                        : EasingType.EaseInOutCirc;
                default:
                    return EasingType.Linear;
            }
        }

        private EasingType MapSpecialEasing(EaseFamily family, EaseDirection direction)
        {
            switch (family)
            {
                case EaseFamily.Back:
                    return direction == EaseDirection.In ? EasingType.EaseInBack
                        : direction == EaseDirection.Out ? EasingType.EaseOutBack
                        : EasingType.EaseInOutBack;
                case EaseFamily.Elastic:
                    return direction == EaseDirection.In ? EasingType.EaseInElastic
                        : direction == EaseDirection.Out ? EasingType.EaseOutElastic
                        : EasingType.EaseInOutElastic;
                case EaseFamily.Bounce:
                    return direction == EaseDirection.In ? EasingType.EaseInBounce
                        : direction == EaseDirection.Out ? EasingType.EaseOutBounce
                        : EasingType.EaseInOutBounce;
                default:
                    return EasingType.Linear;
            }
        }

        private bool ShouldUseLocalSpaceForEditor(MoveStep moveStep)
        {
            return timelineRunner.forceLocalSpace || moveStep.space == SpaceType.Local;
        }

        private Transform ResolveLocalSpaceRootForEditor(GameObject target)
        {
            if (timelineRunner.localSpaceRoot != null)
            {
                return timelineRunner.localSpaceRoot;
            }

            if (timelineRunner.forceLocalSpace)
            {
                return timelineRunner.transform;
            }

            return target != null ? target.transform.parent : null;
        }

        private void AddStep(StepType type)
        {
            Undo.RecordObject(timelineRunner, "Add Timeline Step");

            TimelineStep newStep = null;

            switch (type)
            {
                case StepType.Move:
                    newStep = new MoveStep();
                    break;
                case StepType.Delay:
                    newStep = new DelayStep();
                    break;
                case StepType.CustomAction:
                    newStep = new CustomActionStep();
                    break;
            }

            if (newStep != null)
            {
                newStep.stepName = $"{type} Step {timelineRunner.steps.Count}";
                timelineRunner.steps.Add(newStep);
                timelineRunner.UpdatePathVisualization();
                EditorUtility.SetDirty(timelineRunner);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Timeline Runner", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Timeline Configuration
            EditorGUILayout.PropertyField(playOnStartProperty);
            EditorGUILayout.PropertyField(loopProperty);

            EditorGUILayout.Space(10);

            // Global Key Bindings
            EditorGUILayout.LabelField("Global Key Bindings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableGlobalKeysProperty);

            if (enableGlobalKeysProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(playKeyProperty);
                EditorGUILayout.PropertyField(pauseKeyProperty);
                EditorGUILayout.PropertyField(stopKeyProperty);
                EditorGUILayout.PropertyField(skipKeyProperty);
                EditorGUILayout.PropertyField(backKeyProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // Path Visualization
            EditorGUILayout.LabelField("Path Visualization", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showPathProperty);

            if (showPathProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(useGradientColorsProperty, new GUIContent("Use Gradient Colors"));

                if (useGradientColorsProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(pathGradientProperty, new GUIContent("Path Gradient"));
                }
                else
                {
                    EditorGUILayout.PropertyField(pathColorProperty, new GUIContent("Path Color"));
                }

                EditorGUILayout.PropertyField(pathLineRendererProperty);
                EditorGUILayout.PropertyField(updatePathInRuntimeProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // Space Overrides
            EditorGUILayout.LabelField("Space Overrides", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(forceLocalSpaceProperty, new GUIContent("Force Local Space"));
            EditorGUILayout.PropertyField(localSpaceRootProperty, new GUIContent("Local Space Root"));

            EditorGUILayout.Space(10);

            // Status
            EditorGUILayout.LabelField("Status (Read-Only)", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Playback State", timelineRunner.State.ToString());
            EditorGUILayout.LabelField("Current Step", timelineRunner.CurrentStepIndex.ToString());
            EditorGUILayout.Vector3Field("Current Position", timelineRunner.CurrentPosition);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            // Playback Controls (only in play mode)
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Playback Controls", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Play"))
                    timelineRunner.Play();

                if (GUILayout.Button("Pause"))
                    timelineRunner.Pause();

                if (GUILayout.Button("Resume"))
                    timelineRunner.Resume();

                if (GUILayout.Button("Stop"))
                    timelineRunner.Stop();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Back Step"))
                    timelineRunner.BackStep();

                if (GUILayout.Button("Skip Step"))
                    timelineRunner.SkipStep();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
            }

            // Steps List
            EditorGUILayout.LabelField("Timeline Steps", EditorStyles.boldLabel);
            stepsList.DoLayoutList();

            EditorGUILayout.Space(10);

            // Event property fields (for proper Unity Event editing)
            if (timelineRunner.steps != null && timelineRunner.steps.Count > 0)
            {
                EditorGUILayout.LabelField("Step Events", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Events for each step. These correspond to the steps listed above.", MessageType.Info);

                for (int i = 0; i < timelineRunner.steps.Count; i++)
                {
                    var step = timelineRunner.steps[i];
                    if (step != null)
                    {
                        EditorGUILayout.LabelField($"[{i}] {step.stepName} Events", EditorStyles.boldLabel);

                        // Manually draw UnityEvent fields
                        SerializedProperty stepsProp = serializedObject.FindProperty("steps");
                        if (stepsProp != null && i < stepsProp.arraySize)
                        {
                            SerializedProperty stepProp = stepsProp.GetArrayElementAtIndex(i);

                            if (stepProp != null)
                            {
                                SerializedProperty onStartProp = stepProp.FindPropertyRelative("onStepStart");
                                SerializedProperty onCompleteProp = stepProp.FindPropertyRelative("onStepComplete");

                                if (onStartProp != null)
                                    EditorGUILayout.PropertyField(onStartProp);

                                if (onCompleteProp != null)
                                    EditorGUILayout.PropertyField(onCompleteProp);

                                // For CustomActionStep, also show the customAction event
                                if (step is CustomActionStep)
                                {
                                    SerializedProperty customActionProp = stepProp.FindPropertyRelative("customAction");
                                    if (customActionProp != null)
                                        EditorGUILayout.PropertyField(customActionProp);
                                }
                            }
                        }

                        EditorGUILayout.Space(5);
                    }
                }
            }

            // Update path button
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Update Path Visualization"))
            {
                timelineRunner.UpdatePathVisualization();
            }

            serializedObject.ApplyModifiedProperties();

            // Repaint scene view to update gizmos
            SceneView.RepaintAll();
        }
    }
}
