# Quick Start Guide

Get started with the Timeline System in 5 minutes.

## Step 1: Add Timeline to Scene

1. Create empty GameObject (Right-click in Hierarchy → Create Empty)
2. Name it "Timeline"
3. Add Component → Search "TimelineRunner"

## Step 2: Add Your First Steps

In the Inspector:

1. Scroll to "Timeline Steps"
2. Click `+` button → Choose "Move Step"
3. Set fields:
   - Step Name: "Move Forward"
   - Target Position: (10, 0, 0)
   - Duration: 2
   - Easing: EaseOutCubic

4. Click `+` again → Choose "Delay Step"
5. Set Duration: 1

6. Click `+` again → Choose "Move Step"
7. Set fields:
   - Step Name: "Move Up"
   - Target Position: (10, 5, 0)
   - Duration: 1.5

## Step 3: (Optional) Add Path Visualization

1. Drag `Assets/TimelineSystem/Prefabs/PathLineRenderer.prefab` onto your Timeline GameObject
2. In TimelineRunner, assign the LineRenderer to "Path Line Renderer" field
3. Check "Show Path" checkbox
4. You should see a cyan line showing your path

## Step 4: Test It

1. Press Play
2. Check "Play On Start" in TimelineRunner
3. Press Play again
4. Watch your Timeline GameObject follow the path!

## Step 5: Add Controls (Optional)

In TimelineRunner:
1. Enable "Enable Global Keys"
2. Use keyboard shortcuts:
   - P: Play
   - O: Pause
   - I: Stop
   - Right Arrow: Skip Step
   - Left Arrow: Back Step

## Common Use Cases

### Camera Cutscene
1. Add TimelineRunner to Main Camera
2. Add Move steps for camera positions
3. Set "Play On Start"

### Character Animation
1. Add TimelineRunner to character
2. Add Move steps for walk path
3. Add CustomAction step to trigger animations
4. Use UnityEvents to call Animator methods

### UI Tutorial
1. Add TimelineRunner to highlight sprite
2. Add Move steps to button positions
3. Set Input Condition to "Mouse Click" on each step
4. Player must click to advance

## Next Steps

- Read [README.md](README.md) for full documentation
- Learn how to add custom step types
- Explore easing functions for better animation
- Try multiple timelines for parallel actions

## Need Help?

Check the Troubleshooting section in [README.md](README.md)
