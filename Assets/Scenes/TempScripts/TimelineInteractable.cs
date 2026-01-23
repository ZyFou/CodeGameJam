using UnityEngine;
using TimelineSystem;

public class TimelineInteractable : MonoBehaviour, IClickable
{
    public enum TimelineAction
    {
        Play,
        Pause,
        Resume,
        Stop,
        SkipStep,
        BackStep,
        JumpToStep,
        TogglePlayPause
    }

    [SerializeField] private TimelineRunner timeline;
    [SerializeField] private TimelineAction action = TimelineAction.Play;
    [SerializeField] private int jumpToStepIndex = 0;

    private void Reset()
    {
        if (timeline == null)
        {
            timeline = GetComponentInParent<TimelineRunner>();
        }
    }

    public void Click(ClickContext ctx)
    {
        if (timeline == null)
        {
            timeline = GetComponentInParent<TimelineRunner>();
        }

        if (timeline == null) return;

        switch (action)
        {
            case TimelineAction.Play:
                timeline.Play();
                break;
            case TimelineAction.Pause:
                timeline.Pause();
                break;
            case TimelineAction.Resume:
                timeline.Resume();
                break;
            case TimelineAction.Stop:
                timeline.Stop();
                break;
            case TimelineAction.SkipStep:
                timeline.SkipStep();
                break;
            case TimelineAction.BackStep:
                timeline.BackStep();
                break;
            case TimelineAction.JumpToStep:
                timeline.JumpToStep(jumpToStepIndex);
                break;
            case TimelineAction.TogglePlayPause:
                if (timeline.State == PlaybackState.Playing)
                {
                    timeline.Pause();
                }
                else if (timeline.State == PlaybackState.Paused)
                {
                    timeline.Resume();
                }
                else
                {
                    timeline.Play();
                }
                break;
        }
    }
}
