# Unity Timeline/Cutscene Manager

A modular, extensible timeline-based cutscene and object movement system for Unity.

## Features

- **Type-Based Step System**: Choose from Move, Delay, or CustomAction steps
- **Frame-Rate Independent**: All animations use delta time, no Animator required
- **Path Visualization**: See your timeline path in Editor and runtime using LineRenderer and Gizmos
- **Input Conditions**: Wait for mouse clicks, key presses, or custom input per step
- **UnityEvents**: Execute events at the start and end of each step
- **Modular Architecture**: Easy to extend with new step types
- **Clean Inspector**: Reorderable list with type-specific field visibility
- **Multiple Objects**: Each step can target different GameObjects
- **External Control**: Play, Pause, Stop, SkipStep, BackStep API
- **Global Key Bindings**: Optional keyboard shortcuts for playback control

## Quick Start

### 1. Setup Timeline

1. Create an empty GameObject in your scene
2. Add the `TimelineRunner` component
3. (Optional) Assign a LineRenderer for path visualization:
   - Drag the `PathLineRenderer.prefab` from `Assets/TimelineSystem/Prefabs/` into your scene as a child
   - Assign it to the `Path Line Renderer` field in TimelineRunner

### 2. Add Steps

1. In the Inspector, click the `+` button in the Timeline Steps list
2. Choose a step type:
   - **Move Step**: Animate position, rotation, and scale
   - **Delay Step**: Wait for a duration
   - **Custom Action Step**: Execute UnityEvents or custom code

### 3. Configure Steps

Each step type shows only relevant fields:

**Move Step:**
- Target Position (Vector3)
- Space (World/Local)
- Duration (seconds)
- Easing (Linear, EaseIn/Out, etc.)
- Optional Rotation animation
- Optional Scale animation

**Delay Step:**
- Duration only

**Custom Action Step:**
- UnityEvent to execute
- Optional wait for completion with timeout

### 4. Playback

**In Play Mode:**
- Check `Play On Start` to auto-play
- Use the Inspector buttons: Play, Pause, Resume, Stop, Skip, Back
- Enable Global Keys for keyboard shortcuts

**Via Code:**
```csharp
TimelineRunner timeline = GetComponent<TimelineRunner>();
timeline.Play();
timeline.Pause();
timeline.Resume();
timeline.Stop();
timeline.SkipStep();
timeline.BackStep();
timeline.JumpToStep(3);
```

## Architecture Overview

### Core Components

#### 1. TimelineStep (Abstract Base Class)
Location: `Assets/TimelineSystem/Core/TimelineStep.cs`

Base class for all step types. Defines:
- Common properties (name, type, enabled, input conditions)
- UnityEvents (onStepStart, onStepComplete)
- Abstract methods that derived classes implement:
  - `Initialize()`: Setup before execution
  - `Update()`: Called each frame, returns true when complete
  - `Complete()`: Finalize step execution
  - `GetTargetPosition()`: For path visualization
  - `DrawGizmos()`: Custom Gizmo drawing

**Key Design Principle:**
Each step type derives from TimelineStep and implements only what it needs. The abstract methods ensure all steps can be executed uniformly by StepExecutor.

#### 2. StepExecutor (Execution Engine)
Location: `Assets/TimelineSystem/Core/StepExecutor.cs`

Handles step execution as coroutines:
- Manages step lifecycle (Initialize → Update loop → Complete)
- Handles input conditions (waits for user input before starting)
- Supports Pause/Resume/Skip during execution
- Frame-rate independent timing

**Why a Separate Executor?**
Separating execution logic from the TimelineRunner allows for:
- Cleaner code organization
- Easier testing and debugging
- Potential future enhancement (multiple concurrent executors)

#### 3. TimelineRunner (Main Component)
Location: `Assets/TimelineSystem/Core/TimelineRunner.cs`

Main MonoBehaviour that:
- Holds the list of TimelineSteps
- Manages playback state (Playing/Paused/Stopped)
- Coordinates StepExecutor
- Handles path visualization
- Processes global key bindings
- Provides public API for external control

**Execution Flow:**
```
Play() → PlaybackRoutine() → For each step → StepExecutor.ExecuteStep() → OnStepExecuted() → Next step
```

### Step Types

#### MoveStep
Location: `Assets/TimelineSystem/StepTypes/MoveStep.cs`

Smoothly animates GameObject position, rotation, and scale.

**Features:**
- Easing functions for natural motion
- Local/World space support
- Optional rotation and scale animation
- Frame-rate independent using elapsed time

**Implementation Notes:**
- Stores start values in `Initialize()`
- Uses `Vector3.Lerp()` with eased time value
- Snaps to final position in `Complete()` to avoid floating-point errors

#### DelayStep
Location: `Assets/TimelineSystem/StepTypes/DelayStep.cs`

Simple duration-based delay with no movement.

**Use Cases:**
- Pause between actions
- Wait for events to process
- Timing coordination between multiple objects

#### CustomActionStep
Location: `Assets/TimelineSystem/StepTypes/CustomActionStep.cs`

Executes UnityEvents with optional timeout.

**Use Cases:**
- Trigger audio/visual effects
- Enable/disable GameObjects
- Call custom game logic
- Integrate with other systems

**Configuration:**
- `customAction`: UnityEvent to invoke
- `waitForCompletion`: If true, waits for timeout before continuing
- `timeout`: Duration to wait (0 = instant)

### Utilities

#### Easing Functions
Location: `Assets/TimelineSystem/Utils/Easing.cs`

Provides 28 easing functions for natural motion:
- Linear
- Quad, Cubic, Quart (ease in/out/in-out)
- Sine, Expo, Circ
- Back, Elastic, Bounce

**Usage:**
```csharp
float easedValue = Easing.Ease(t, EasingType.EaseOutCubic);
```

### Custom Editor

#### TimelineRunnerEditor
Location: `Assets/TimelineSystem/Editor/TimelineRunnerEditor.cs`

Custom Inspector that:
- Uses `ReorderableList` for drag-and-drop step reordering
- Shows type-specific fields based on StepType
- Displays current GameObject position as readonly
- Provides add dropdown menu for creating new steps
- Shows playback controls in Play mode
- Allows editing UnityEvents for each step

**Key Features:**
- Dynamic element height calculation based on step type
- Type-specific field rendering (only shows relevant properties)
- Real-time path visualization updates
- Clean, spaced layout using EditorGUILayout

## Extension Guide

### Adding a New Step Type

Want to add a new step type (e.g., ColorStep, AudioStep)? Follow these steps:

#### 1. Update StepType Enum

Edit `Assets/TimelineSystem/Core/TimelineStep.cs`:

```csharp
public enum StepType
{
    Move,
    Delay,
    CustomAction,
    Color  // Add your new type
}
```

#### 2. Create Step Class

Create `Assets/TimelineSystem/StepTypes/ColorStep.cs`:

```csharp
using UnityEngine;
using System;

namespace TimelineSystem
{
    [Serializable]
    public class ColorStep : TimelineStep
    {
        [Header("Color Configuration")]
        public Color targetColor = Color.white;
        public float duration = 1f;
        public EasingType easing = EasingType.Linear;

        private Renderer targetRenderer;
        private Color startColor;
        private float elapsedTime;

        public ColorStep() : base(StepType.Color)
        {
        }

        public override void Initialize(GameObject target)
        {
            targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer != null && targetRenderer.material != null)
            {
                startColor = targetRenderer.material.color;
            }
            elapsedTime = 0f;
        }

        public override bool Update(float deltaTime)
        {
            if (targetRenderer == null) return true;

            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = Easing.Ease(t, easing);

            targetRenderer.material.color = Color.Lerp(startColor, targetColor, easedT);

            return t >= 1f;
        }

        public override void Complete()
        {
            if (targetRenderer != null && targetRenderer.material != null)
            {
                targetRenderer.material.color = targetColor;
            }
        }

        public override Vector3 GetTargetPosition()
        {
            // Color doesn't change position
            return targetRenderer != null ? targetRenderer.transform.position : Vector3.zero;
        }

        public override void DrawGizmos(Vector3 currentPosition)
        {
            // Draw a colored sphere
            Gizmos.color = targetColor;
            Gizmos.DrawWireSphere(currentPosition, 0.3f);
        }
    }
}
```

#### 3. Update Custom Editor

Edit `Assets/TimelineSystem/Editor/TimelineRunnerEditor.cs`:

**Add to dropdown menu:**
```csharp
stepsList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
{
    var menu = new GenericMenu();
    menu.AddItem(new GUIContent("Move Step"), false, () => AddStep(StepType.Move));
    menu.AddItem(new GUIContent("Delay Step"), false, () => AddStep(StepType.Delay));
    menu.AddItem(new GUIContent("Custom Action Step"), false, () => AddStep(StepType.CustomAction));
    menu.AddItem(new GUIContent("Color Step"), false, () => AddStep(StepType.Color)); // Add this
    menu.ShowAsContext();
};
```

**Add case in AddStep method:**
```csharp
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
        case StepType.Color:  // Add this
            newStep = new ColorStep();
            break;
    }

    // ... rest of method
}
```

**Add fields in DrawStepTypeFields:**
```csharp
private float DrawStepTypeFields(TimelineStep step, float x, float y, float width, float lineHeight, float spacing)
{
    float currentY = y;
    EditorGUI.LabelField(new Rect(x + 15, currentY, width - 20, lineHeight), "Step Configuration", EditorStyles.boldLabel);
    currentY += lineHeight + spacing;

    if (step is MoveStep moveStep)
    {
        // ... existing Move fields
    }
    else if (step is DelayStep delayStep)
    {
        // ... existing Delay fields
    }
    else if (step is CustomActionStep actionStep)
    {
        // ... existing CustomAction fields
    }
    else if (step is ColorStep colorStep)  // Add this
    {
        Rect colorRect = new Rect(x + 15, currentY, width - 20, lineHeight);
        colorStep.targetColor = EditorGUI.ColorField(colorRect, "Target Color", colorStep.targetColor);
        currentY += lineHeight + spacing;

        Rect durationRect = new Rect(x + 15, currentY, width - 20, lineHeight);
        colorStep.duration = EditorGUI.FloatField(durationRect, "Duration", colorStep.duration);
        currentY += lineHeight + spacing;

        Rect easingRect = new Rect(x + 15, currentY, width - 20, lineHeight);
        colorStep.easing = (EasingType)EditorGUI.EnumPopup(easingRect, "Easing", colorStep.easing);
        currentY += lineHeight + spacing;
    }

    return currentY;
}
```

**Add case in GetStepTypeFieldsHeight:**
```csharp
private float GetStepTypeFieldsHeight(TimelineStep step)
{
    float lineHeight = EditorGUIUtility.singleLineHeight;
    float height = lineHeight; // Header

    if (step is MoveStep moveStep)
    {
        // ... existing
    }
    else if (step is DelayStep)
    {
        // ... existing
    }
    else if (step is CustomActionStep actionStep)
    {
        // ... existing
    }
    else if (step is ColorStep)  // Add this
    {
        height += lineHeight * 3; // color, duration, easing
    }

    return height;
}
```

That's it! Your new step type is fully integrated.

## Best Practices

### 1. Design Principles

**Single Responsibility:**
- Each step type has one clear purpose
- TimelineRunner manages playback, StepExecutor handles execution
- Separation of concerns makes debugging easier

**Composition over Inheritance:**
- Steps contain data, executors contain logic
- Easy to add new behaviors without modifying existing code

**Frame-Rate Independence:**
- Always use `deltaTime` for timing
- Store elapsed time and normalize to 0-1 range
- Ensures consistent playback regardless of FPS

### 2. Performance Considerations

**Path Visualization:**
- Disable `updatePathInRuntime` if you don't need live path updates
- LineRenderer is more performant than Gizmos in builds
- Consider disabling `showPath` in production builds

**UnityEvents:**
- Be cautious with expensive operations in UnityEvents
- Consider using CustomActionStep with timeout for long operations

**Step Count:**
- System is optimized for 10-100 steps
- For thousands of steps, consider breaking into multiple timelines

### 3. Common Patterns

**Sequential Movement:**
```
Move → Delay → Move → Delay → CustomAction
```

**Parallel Timelines:**
Create multiple TimelineRunners on different objects and synchronize:
```csharp
timeline1.Play();
timeline2.Play();
timeline3.Play();
```

**Looping Patrols:**
Enable `loop` on TimelineRunner for continuous patrol paths.

**Waiting for Input:**
Set `inputCondition` on any step to pause until user input.

**Camera Cutscenes:**
Create a timeline on your camera with Move steps for cinematic shots.

### 4. Debugging Tips

**Path Not Showing:**
- Ensure `showPath` is enabled
- Assign a LineRenderer or check Gizmos are enabled in Scene view
- Verify steps have valid target positions

**Step Not Executing:**
- Check `enabled` toggle on the step
- Verify `targetObject` is assigned (or leave null to use GameObject with TimelineRunner)
- Check input condition isn't blocking execution

**Jerky Movement:**
- Verify `duration` is > 0
- Try different easing functions
- Check for expensive operations in Update()

**Events Not Firing:**
- Expand the "Step Events" section in Inspector
- Ensure target objects/methods are assigned in UnityEvents
- Check UnityEvent methods are public

## API Reference

### TimelineRunner

**Public Methods:**
```csharp
void Play()                  // Start/restart timeline from beginning or current position
void Pause()                 // Pause playback (can resume)
void Resume()                // Resume after pause
void Stop()                  // Stop and reset to beginning
void SkipStep()              // Skip current step immediately
void BackStep()              // Go back one step and replay
void JumpToStep(int index)   // Jump to specific step index
void UpdatePathVisualization() // Manually refresh path visualization
```

**Public Properties:**
```csharp
PlaybackState State { get; }        // Current playback state
int CurrentStepIndex { get; }       // Current step being executed (-1 if stopped)
Vector3 CurrentPosition { get; }    // Current position of timeline object
List<TimelineStep> steps            // List of timeline steps
bool playOnStart                    // Auto-play on Start()
bool loop                           // Loop timeline when complete
bool showPath                       // Show path visualization
```

### TimelineStep

**Public Methods:**
```csharp
bool CheckInputCondition()   // Check if input condition is met
```

**Public Properties:**
```csharp
string stepName              // Display name
StepType stepType           // Type of step (readonly after creation)
bool enabled                // Enable/disable step
GameObject targetObject     // Target to animate (null = Timeline's GameObject)
InputConditionType inputCondition  // Input required to start
KeyCode inputKey            // Key to press (if KeyPress condition)
UnityEvent onStepStart      // Fired when step starts
UnityEvent onStepComplete   // Fired when step completes
```

## Examples

### Example 1: Simple Camera Path

```
Step 0 (Move): Camera position (0,0,0) → (10,5,0), 2s, EaseOutCubic
Step 1 (Delay): 1s pause
Step 2 (Move): Camera position (10,5,0) → (10,5,10), 2s, Linear
Step 3 (CustomAction): Trigger "ShowDialog" event
```

### Example 2: Character Cutscene

```
Step 0 (CustomAction): Disable player control
Step 1 (Move): Character walks to door, 3s
Step 2 (Delay): Wait 0.5s
Step 3 (CustomAction): Play door opening sound
Step 4 (Move): Door opens (rotate), 1s
Step 5 (Move): Character enters door, 2s
Step 6 (CustomAction): Enable player control
```

### Example 3: UI Tutorial

```
Step 0 (Move): Highlight move to button 1, 0.5s
Step 1 (Delay + Input): Wait for player click
Step 2 (Move): Highlight move to button 2, 0.5s
Step 3 (Delay + Input): Wait for player click
Step 4 (CustomAction): Show completion dialog
```

## Troubleshooting

**Q: My steps aren't reordering properly**
A: Make sure you're dragging by the left edge of the step (the drag handle in ReorderableList)

**Q: Path visualization isn't updating in Editor**
A: Click "Update Path Visualization" button or toggle a step's enabled state

**Q: Global keys aren't working**
A: Enable "Enable Global Keys" checkbox in Timeline Runner

**Q: Local space movement isn't working correctly**
A: Ensure your target object has a parent Transform for local space to work

**Q: UnityEvents show "Missing" in Inspector**
A: The target object was deleted or the method signature changed. Reassign the event.

## Credits

Created for Unity Timeline/Cutscene management. Designed to be modular and extensible.

## License

Free to use in personal and commercial projects.
