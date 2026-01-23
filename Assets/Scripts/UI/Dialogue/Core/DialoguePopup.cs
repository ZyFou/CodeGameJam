using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialoguePopup : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component for dialogue content")]
    public TMP_Text dialogueText;

    [Tooltip("Text component for speaker name (optional)")]
    public TMP_Text speakerNameText;

    [Tooltip("Image component for speaker portrait (optional)")]
    public Image speakerPortraitImage;

    [Tooltip("GameObject shown when waiting for player input (optional)")]
    public GameObject continueIndicator;

    [Tooltip("CanvasGroup for fade in/out transitions")]
    public CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [Tooltip("Duration of fade in animation")]
    public float fadeInDuration = 0.3f;

    [Tooltip("Duration of fade out animation")]
    public float fadeOutDuration = 0.2f;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    // Private state
    private DialogueData currentDialogueData;
    private Dictionary<string, string> globalVariables;
    private Action onCompleteCallback;
    private AnimatedDialogueText animatedText;
    private int currentLineIndex;
    private bool isWaitingForInput;
    private bool isActiveDialogue;
    private Coroutine dialogueCoroutine;

    private void Awake()
    {
        // Get or add AnimatedDialogueText component
        if (dialogueText != null)
        {
            animatedText = dialogueText.GetComponent<AnimatedDialogueText>();
            if (animatedText == null)
            {
                animatedText = dialogueText.gameObject.AddComponent<AnimatedDialogueText>();
            }
        }

        // Hide continue indicator initially
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        // Initialize canvas group
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Initialize and start the dialogue
    /// </summary>
    public void Initialize(DialogueData data, Dictionary<string, string> variables, Action onComplete)
    {
        if (data == null)
        {
            Debug.LogError("DialoguePopup: Cannot initialize with null DialogueData");
            onComplete?.Invoke();
            Destroy(gameObject);
            return;
        }

        if (data.lines == null || data.lines.Count == 0)
        {
            Debug.LogWarning("DialoguePopup: DialogueData has no lines");
            onComplete?.Invoke();
            Destroy(gameObject);
            return;
        }

        currentDialogueData = data;
        globalVariables = variables;
        onCompleteCallback = onComplete;
        currentLineIndex = 0;
        isActiveDialogue = true;

        dialogueCoroutine = StartCoroutine(PlayDialogueSequence());
    }

    /// <summary>
    /// Skip the current typewriter animation
    /// </summary>
    public void SkipCurrentAnimation()
    {
        if (animatedText != null && animatedText.IsAnimating)
        {
            animatedText.SkipToEnd();
        }
    }

    /// <summary>
    /// Advance to the next dialogue line
    /// </summary>
    public void AdvanceDialogue()
    {
        if (isWaitingForInput)
        {
            isWaitingForInput = false;
        }
    }

    /// <summary>
    /// Close the dialogue popup
    /// </summary>
    public void Close()
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }

        isActiveDialogue = false;
        StartCoroutine(CloseDialogue());
    }

    private void Update()
    {
        if (!isActiveDialogue) return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (currentDialogueData == null) return;

        bool spacePressed = Input.GetKeyDown(currentDialogueData.continueKey);
        bool clickPressed = Input.GetMouseButtonDown(0) && currentDialogueData.allowClickToContinue;

        if (spacePressed || clickPressed)
        {
            if (animatedText != null && animatedText.IsAnimating && currentDialogueData.allowSkipAnimation)
            {
                // Skip animation
                SkipCurrentAnimation();
            }
            else if (isWaitingForInput && currentDialogueData.allowSkipLine)
            {
                // Advance to next line
                AdvanceDialogue();
            }
        }
    }

    private IEnumerator PlayDialogueSequence()
    {
        // Fade in
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeInDuration));
        }

        // Invoke dialogue start event
        onDialogueStart?.Invoke();

        // Play each line
        for (currentLineIndex = 0; currentLineIndex < currentDialogueData.lines.Count; currentLineIndex++)
        {
            DialogueLine line = currentDialogueData.lines[currentLineIndex];
            yield return StartCoroutine(ShowLine(line));
        }

        // Invoke dialogue end event
        onDialogueEnd?.Invoke();

        // Complete
        yield return StartCoroutine(CloseDialogue());
    }

    private IEnumerator ShowLine(DialogueLine line)
    {
        if (line == null) yield break;

        // Resolve localization key and variables
        string resolvedText = ResolveLineText(line, globalVariables);

        // Update speaker info
        UpdateSpeakerInfo(line);

        // Hide continue indicator while animating
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        // Invoke line start event
        line.onLineStart?.Invoke();

        // Determine text speed for this line
        float textSpeed = line.customTextSpeed > 0 ? line.customTextSpeed : currentDialogueData.textSpeed;
        float charsPerSecond = 1f / textSpeed;

        // Start typewriter animation
        bool animationComplete = false;
        if (animatedText != null)
        {
            animatedText.StartAnimation(resolvedText, charsPerSecond, () =>
            {
                animationComplete = true;
            });

            // Wait for animation to complete
            while (!animationComplete)
            {
                yield return null;
            }
        }
        else
        {
            // Fallback if AnimatedDialogueText is missing
            if (dialogueText != null)
            {
                dialogueText.text = resolvedText;
            }
        }

        // Show continue indicator
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }

        // Wait for player input or auto-advance
        isWaitingForInput = true;
        float lineDelay = line.customLineDelay >= 0 ? line.customLineDelay : currentDialogueData.lineDelay;

        float elapsed = 0f;
        while (isWaitingForInput && elapsed < 60f) // 60s timeout
        {
            elapsed += Time.unscaledDeltaTime;

            // Auto-advance after delay (if lineDelay > 0 and not last line)
            if (lineDelay > 0 && elapsed >= lineDelay && currentLineIndex < currentDialogueData.lines.Count - 1)
            {
                break;
            }

            yield return null;
        }

        isWaitingForInput = false;

        // Hide continue indicator
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        // Invoke line end event
        line.onLineEnd?.Invoke();
    }

    private string ResolveLineText(DialogueLine line, Dictionary<string, string> globalVars)
    {
        if (string.IsNullOrEmpty(line.localizationKey))
        {
            return string.Empty;
        }

        // Merge global and line-specific variables
        Dictionary<string, string> mergedVars = MergeVariables(globalVars, line.variables);

        // Get localized text with variable substitution
        return LocalizationManager.GetText(line.localizationKey, mergedVars);
    }

    private Dictionary<string, string> MergeVariables(Dictionary<string, string> global, List<LocalizedVariable> local)
    {
        Dictionary<string, string> merged = new Dictionary<string, string>();

        // Start with global variables
        if (global != null)
        {
            foreach (var kvp in global)
            {
                merged[kvp.Key] = kvp.Value;
            }
        }

        // Override with line-specific variables
        if (local != null)
        {
            foreach (var variable in local)
            {
                if (!string.IsNullOrEmpty(variable.key))
                {
                    merged[variable.key] = variable.value ?? string.Empty;
                }
            }
        }

        return merged;
    }

    private void UpdateSpeakerInfo(DialogueLine line)
    {
        // Update speaker name
        if (speakerNameText != null)
        {
            if (!string.IsNullOrEmpty(line.speakerName))
            {
                speakerNameText.text = line.speakerName;
                speakerNameText.gameObject.SetActive(true);
            }
            else
            {
                speakerNameText.gameObject.SetActive(false);
            }
        }

        // Update speaker portrait
        if (speakerPortraitImage != null)
        {
            if (line.speakerPortrait != null)
            {
                speakerPortraitImage.sprite = line.speakerPortrait;
                speakerPortraitImage.gameObject.SetActive(true);
            }
            else
            {
                speakerPortraitImage.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        if (canvasGroup == null || duration <= 0f)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = to;
            }
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator CloseDialogue()
    {
        // Fade out
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 0f, fadeOutDuration));
        }

        // Invoke completion callback
        onCompleteCallback?.Invoke();

        // Destroy popup
        Destroy(gameObject);
    }
}
