using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data", fileName = "NewDialogue")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Configuration")]
    [Tooltip("List of dialogue lines to display sequentially")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Animation Settings")]
    [Tooltip("Seconds per character (0.05 = 20 chars/sec)")]
    [Range(0.01f, 0.2f)]
    public float textSpeed = 0.05f;

    [Tooltip("Delay in seconds before auto-advancing to next line")]
    [Range(0f, 5f)]
    public float lineDelay = 0.5f;

    [Tooltip("Allow player to skip typewriter animation (Space/Click)")]
    public bool allowSkipAnimation = true;

    [Tooltip("Allow player to advance to next line (Space/Click)")]
    public bool allowSkipLine = true;

    [Header("Input Settings")]
    [Tooltip("Key to continue/skip dialogue")]
    public KeyCode continueKey = KeyCode.Space;

    [Tooltip("Allow mouse click to continue/skip")]
    public bool allowClickToContinue = true;
}
