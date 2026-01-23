using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueLine
{
    [Header("Content")]
    [Tooltip("Localization key (e.g., 'dialogue.intro.welcome')")]
    public string localizationKey;

    [Tooltip("Variables specific to this line (override global variables)")]
    public List<LocalizedVariable> variables = new List<LocalizedVariable>();

    [Header("Presentation")]
    [Tooltip("Speaker name (optional, can also use localization key)")]
    public string speakerName;

    [Tooltip("Speaker portrait sprite (optional)")]
    public Sprite speakerPortrait;

    [Header("Timing Overrides")]
    [Tooltip("Custom text speed for this line (-1 = use default from DialogueData)")]
    public float customTextSpeed = -1f;

    [Tooltip("Custom delay after this line (-1 = use default from DialogueData)")]
    public float customLineDelay = -1f;

    [Header("Events")]
    public UnityEvent onLineStart;
    public UnityEvent onLineEnd;

    public DialogueLine()
    {
        onLineStart = new UnityEvent();
        onLineEnd = new UnityEvent();
    }
}
