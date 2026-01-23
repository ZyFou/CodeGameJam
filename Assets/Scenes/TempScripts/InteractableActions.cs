using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableActions : MonoBehaviour, IClickable
{
    [Serializable]
    public class ActionEntry
    {
        public string label;
        public UnityEvent action = new UnityEvent();
    }

    [SerializeField] private bool oneShot = false;
    [SerializeField] private List<ActionEntry> actions = new List<ActionEntry>();

    private bool hasFired;

    public void Click(ClickContext ctx)
    {
        if (oneShot && hasFired) return;
        hasFired = true;

        for (int i = 0; i < actions.Count; i++)
        {
            actions[i]?.action?.Invoke();
        }
    }
}
