using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace TimelineSystem
{
    [CustomEditor(typeof(TimelineRunner))]
    public class TimelineRunnerEditor : Editor
    {
        private ReorderableList stepsList;
        private TimelineRunner timelineRunner;
        private SerializedProperty stepsProperty;
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
                step.targetObject = (GameObject)EditorGUI.ObjectField(targetRect, "Target Object", step.targetObject, typeof(GameObject), true);
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

                height += 10; // Bottom padding

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
                Rect posRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.targetPosition = EditorGUI.Vector3Field(posRect, "Target Position", moveStep.targetPosition);
                currentY += lineHeight + spacing;

                Rect spaceRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.space = (SpaceType)EditorGUI.EnumPopup(spaceRect, "Space", moveStep.space);
                currentY += lineHeight + spacing;

                Rect durationRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.duration = EditorGUI.FloatField(durationRect, "Duration", moveStep.duration);
                currentY += lineHeight + spacing;

                Rect easingRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.easing = (EasingType)EditorGUI.EnumPopup(easingRect, "Easing", moveStep.easing);
                currentY += lineHeight + spacing;

                Rect animRotRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                moveStep.animateRotation = EditorGUI.Toggle(animRotRect, "Animate Rotation", moveStep.animateRotation);
                currentY += lineHeight + spacing;

                if (moveStep.animateRotation)
                {
                    Rect rotRect = new Rect(x + 15, currentY, width - 20, lineHeight);
                    moveStep.targetRotation = EditorGUI.Vector3Field(rotRect, "Target Rotation", moveStep.targetRotation);
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
                height += lineHeight * 6; // position, space, duration, easing, animRot toggle, animScale toggle
                if (moveStep.animateRotation) height += lineHeight;
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
