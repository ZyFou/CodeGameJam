using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple component to trigger dialogues from Unity Editor or other scripts
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    [Tooltip("The dialogue data to display")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Variables (Optional)")]
    [Tooltip("Global variables for text substitution")]
    [SerializeField] private List<LocalizedVariable> variables = new List<LocalizedVariable>();

    [Header("Trigger Options")]
    [Tooltip("Automatically trigger dialogue on Start")]
    [SerializeField] private bool triggerOnStart = false;

    [Tooltip("Trigger dialogue when player enters trigger collider")]
    [SerializeField] private bool triggerOnEnter = false;

    [Tooltip("Tag required for OnTriggerEnter (empty = any tag)")]
    [SerializeField] private string requiredTag = "Player";

    [Header("Events")]
    [Tooltip("Called when dialogue completes")]
    [SerializeField] private UnityEngine.Events.UnityEvent onDialogueComplete;

    private bool hasTriggered = false;

    private void Start()
    {
        if (triggerOnStart)
        {
            TriggerDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter || hasTriggered) return;

        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            TriggerDialogue();
            hasTriggered = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnEnter || hasTriggered) return;

        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            TriggerDialogue();
            hasTriggered = true;
        }
    }

    /// <summary>
    /// Trigger the dialogue (call from buttons, other scripts, etc.)
    /// </summary>
    public void TriggerDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("DialogueTrigger: No DialogueData assigned!", this);
            return;
        }

        // Convert LocalizedVariable list to Dictionary
        Dictionary<string, string> vars = null;
        if (variables != null && variables.Count > 0)
        {
            vars = new Dictionary<string, string>();
            foreach (var variable in variables)
            {
                if (!string.IsNullOrEmpty(variable.key))
                {
                    vars[variable.key] = variable.value ?? string.Empty;
                }
            }
        }

        // Show dialogue
        DialogueManager.ShowDialogue(dialogueData, vars, OnDialogueFinished);
    }

    /// <summary>
    /// Trigger dialogue with custom variables
    /// </summary>
    public void TriggerDialogue(Dictionary<string, string> customVariables)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("DialogueTrigger: No DialogueData assigned!", this);
            return;
        }

        DialogueManager.ShowDialogue(dialogueData, customVariables, OnDialogueFinished);
    }

    /// <summary>
    /// Reset the trigger so it can be triggered again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void OnDialogueFinished()
    {
        onDialogueComplete?.Invoke();
    }

    // Helper method for testing in editor
    [ContextMenu("Test Trigger Dialogue")]
    private void TestTrigger()
    {
        TriggerDialogue();
    }
}
