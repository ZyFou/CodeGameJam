# Timeline System Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      TimelineRunner                         │
│  (Main MonoBehaviour - Manages Timeline Playback)          │
│                                                             │
│  - List<TimelineStep> steps                                │
│  - StepExecutor executor                                   │
│  - PlaybackState state                                     │
│  - Public API (Play, Pause, Stop, Skip, Back)             │
└────────────────┬────────────────────────────────────────────┘
                 │
                 │ manages
                 ↓
┌─────────────────────────────────────────────────────────────┐
│                      StepExecutor                           │
│  (Handles Step Execution via Coroutines)                   │
│                                                             │
│  - ExecuteStep(TimelineStep, GameObject)                   │
│  - Pause/Resume/Skip control                               │
│  - Input condition waiting                                 │
└────────────────┬────────────────────────────────────────────┘
                 │
                 │ executes
                 ↓
┌─────────────────────────────────────────────────────────────┐
│                  TimelineStep (Abstract)                    │
│  (Base class for all step types)                           │
│                                                             │
│  - Common Properties (name, type, enabled, input)          │
│  - UnityEvents (onStepStart, onStepComplete)              │
│  - Abstract Methods:                                       │
│    • Initialize(GameObject)                                │
│    • Update(float deltaTime) → bool                        │
│    • Complete()                                            │
│    • GetTargetPosition() → Vector3                         │
│    • DrawGizmos(Vector3)                                   │
└────────────────┬────────────────────────────────────────────┘
                 │
                 │ inherited by
                 ↓
    ┌────────────┴────────────┬────────────────┐
    ↓                         ↓                ↓
┌─────────┐            ┌─────────┐      ┌─────────────┐
│MoveStep │            │DelayStep│      │CustomAction │
│         │            │         │      │Step         │
│- target │            │-duration│      │-customAction│
│  position│            └─────────┘      │-timeout     │
│- space  │                              └─────────────┘
│- duration│
│- easing │
│- rotation│
│- scale  │
└─────────┘
```

## Execution Flow

```
User Calls Play()
       ↓
TimelineRunner.Play()
       ↓
Start PlaybackRoutine() Coroutine
       ↓
For each enabled step:
       ↓
       ├──→ Determine target GameObject
       ↓
       ├──→ Call StepExecutor.ExecuteStep(step, target)
       ↓         │
       │         ├──→ step.Initialize(target)
       │         ↓
       │         ├──→ Invoke step.onStepStart
       │         ↓
       │         ├──→ Wait for input condition (if specified)
       │         ↓
       │         ├──→ Loop: step.Update(deltaTime)
       │         │         └──→ Returns true when complete
       │         ↓
       │         ├──→ step.Complete()
       │         ↓
       │         └──→ Invoke step.onStepComplete
       ↓
       └──→ Move to next step
       ↓
Timeline complete → Stop or Loop
```

## Class Responsibilities

### TimelineRunner
**Role:** Orchestrator
- Stores and manages the step list
- Manages playback state (Playing/Paused/Stopped)
- Handles global key bindings
- Coordinates StepExecutor
- Manages path visualization
- Provides public API for external control

**Does NOT:**
- Execute step logic directly
- Handle frame-by-frame updates
- Manage individual step state

### StepExecutor
**Role:** Execution Engine
- Executes steps as coroutines
- Manages step lifecycle
- Handles pause/resume/skip
- Waits for input conditions
- Frame-rate independent timing

**Does NOT:**
- Store step data
- Manage timeline sequence
- Handle path visualization

### TimelineStep (Base Class)
**Role:** Data Container + Interface
- Stores step configuration
- Defines common properties
- Declares abstract methods for step lifecycle
- Handles input condition checking

**Does NOT:**
- Execute itself
- Manage coroutines
- Handle visualization directly

### Concrete Step Types (MoveStep, DelayStep, CustomActionStep)
**Role:** Specialized Implementations
- Implement abstract methods from TimelineStep
- Contain type-specific fields
- Execute type-specific logic

**Does NOT:**
- Manage their own execution timing (StepExecutor handles this)
- Track global timeline state

## Design Patterns Used

### 1. Template Method Pattern
**Where:** TimelineStep abstract class
**Why:** Defines step execution skeleton while letting subclasses implement specifics

```csharp
// TimelineStep defines the template
public abstract class TimelineStep
{
    public abstract void Initialize(GameObject target);
    public abstract bool Update(float deltaTime);
    public abstract void Complete();
}

// Subclasses fill in the specifics
public class MoveStep : TimelineStep
{
    public override void Initialize(GameObject target) { /* Move-specific init */ }
    public override bool Update(float deltaTime) { /* Move-specific update */ }
    public override void Complete() { /* Move-specific completion */ }
}
```

### 2. Strategy Pattern
**Where:** Step types
**Why:** Different step types can be swapped at runtime without changing executor logic

```csharp
// StepExecutor doesn't care what type of step it is
public IEnumerator ExecuteStep(TimelineStep step, GameObject target, ...)
{
    step.Initialize(target);  // Polymorphism handles the rest
    while (!step.Update(deltaTime)) { yield return null; }
    step.Complete();
}
```

### 3. Observer Pattern
**Where:** UnityEvents
**Why:** Steps notify interested parties without tight coupling

```csharp
step.onStepStart?.Invoke();   // Any listeners are notified
step.onStepComplete?.Invoke();
```

### 4. State Pattern
**Where:** PlaybackState enum
**Why:** Timeline behavior changes based on current state

```csharp
public enum PlaybackState { Stopped, Playing, Paused }

// Behavior differs based on state
public void Play()
{
    if (playbackState == PlaybackState.Playing) return;  // Already playing
    // ... start playback
}
```

## Data Flow

### Initialization (Editor)
```
User creates steps in Inspector
       ↓
TimelineRunnerEditor serializes to TimelineRunner.steps
       ↓
Each step stores its configuration (position, duration, etc.)
       ↓
Path visualization calculates and displays
```

### Runtime Execution
```
PlaybackRoutine() coroutine starts
       ↓
Get step from steps[currentStepIndex]
       ↓
Pass step to StepExecutor.ExecuteStep()
       ↓
StepExecutor calls step.Initialize()
       ↓
Each frame: StepExecutor calls step.Update(deltaTime)
       ↓
Step calculates and applies transformations
       ↓
When step.Update() returns true, StepExecutor calls step.Complete()
       ↓
Return to PlaybackRoutine, increment currentStepIndex
       ↓
Repeat until all steps complete
```

## Extension Points

The system is designed for easy extension:

### 1. New Step Types
- Create class inheriting from `TimelineStep`
- Implement abstract methods
- Update `TimelineRunnerEditor` to display fields
- Add to dropdown menu

**Files to modify:**
- `Core/TimelineStep.cs` - Add to StepType enum
- `StepTypes/YourNewStep.cs` - New file
- `Editor/TimelineRunnerEditor.cs` - Add UI handling

### 2. Custom Input Conditions
- Add to `InputConditionType` enum
- Implement in `TimelineStep.CheckInputCondition()`

### 3. Custom Easing Functions
- Add to `EasingType` enum in `Utils/Easing.cs`
- Implement easing function in `Easing.Ease()`

### 4. External Control
Use the public API:
```csharp
TimelineRunner timeline = GetComponent<TimelineRunner>();
timeline.Play();
timeline.JumpToStep(5);
```

## Key Design Decisions

### Why Separate StepExecutor from TimelineRunner?
**Decision:** StepExecutor is a separate component
**Rationale:**
- Single Responsibility Principle
- Easier to test and debug
- Cleaner code organization
- Potential for future parallel execution

### Why Abstract TimelineStep Instead of Interface?
**Decision:** TimelineStep is an abstract class, not an interface
**Rationale:**
- Can store common data (name, type, enabled, events)
- Can provide base implementations (CheckInputCondition)
- Serializable by Unity
- More ergonomic than interface + base class combination

### Why Coroutines Instead of Update Loop?
**Decision:** Steps execute as coroutines via StepExecutor
**Rationale:**
- Natural representation of sequential execution
- Easy pause/resume implementation
- Cleaner than state machine in Update()
- Frame-rate independent by design

### Why Type-Specific Classes Instead of Generic Step?
**Decision:** MoveStep, DelayStep, CustomActionStep are separate classes
**Rationale:**
- Type safety
- Clear intent and documentation
- Editor can show type-specific fields
- Easier to extend with new types
- No wasted memory on unused fields

### Why ReorderableList in Editor?
**Decision:** Custom editor uses Unity's ReorderableList
**Rationale:**
- Professional drag-and-drop interface
- Familiar to Unity developers
- Built-in callbacks for add/remove/reorder
- Clean visual design

## Performance Characteristics

### Time Complexity
- **Add Step:** O(1)
- **Remove Step:** O(n) - list removal
- **Execute Step:** O(1) per frame
- **Reorder Steps:** O(n) - list reorder
- **Path Visualization:** O(n) - iterate all steps

### Space Complexity
- **Per Timeline:** O(n) where n = number of steps
- **Per Step:** O(1) - fixed size based on step type
- **Path LineRenderer:** O(n) - one vertex per step

### Frame-Rate Impact
- **Minimal:** Only currently executing step updates per frame
- **Path Visualization:** Can be disabled for production builds
- **Gizmos:** Editor-only, no runtime cost

## Thread Safety

**Current Implementation:** Single-threaded
- All execution on main Unity thread
- Coroutines are cooperative multitasking, not threads
- No race conditions possible

**Future:** Could add async/await for I/O-bound CustomActions

## Serialization

### What Unity Serializes
- `TimelineRunner.steps` (List<TimelineStep>)
- All step properties marked `[SerializeField]` or public
- UnityEvents (onStepStart, onStepComplete, customAction)

### What Unity Doesn't Serialize
- Runtime state (elapsedTime, currentStepIndex)
- Cached references (Transform, Renderer)
- Coroutine state

**Implication:** Timeline state doesn't persist between Play sessions

## Best Practices for Maintenance

1. **Adding Features:** Start with TimelineStep, then implement in concrete step types
2. **Debugging:** Check StepExecutor for execution issues, TimelineRunner for sequencing
3. **Performance:** Profile path visualization, consider disabling in builds
4. **Testing:** Test each step type independently before integration
5. **Version Control:** Steps serialize to scene files, consider prefabs for reusable timelines

## Summary

The Timeline System uses a clean separation of concerns:
- **TimelineRunner** orchestrates
- **StepExecutor** executes
- **TimelineStep** defines interface
- **Concrete Steps** implement behavior

This architecture enables:
- Easy extension with new step types
- Clean inspector with type-specific fields
- Frame-rate independent execution
- Testable components
- Maintainable codebase

The system prioritizes developer experience with a polished editor, comprehensive documentation, and clear extension points.
