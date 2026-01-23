using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class AnimatedDialogueText : MonoBehaviour
{
    private TMP_Text targetText;
    private Coroutine animationCoroutine;
    private bool isSkipped;

    public bool IsAnimating => animationCoroutine != null;

    private void Awake()
    {
        targetText = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Start typewriter animation on the text
    /// </summary>
    /// <param name="fullText">Complete text to animate</param>
    /// <param name="charsPerSecond">Animation speed (characters per second)</param>
    /// <param name="onComplete">Callback when animation completes</param>
    public void StartAnimation(string fullText, float charsPerSecond, Action onComplete)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        isSkipped = false;
        animationCoroutine = StartCoroutine(AnimateText(fullText, charsPerSecond, onComplete));
    }

    /// <summary>
    /// Skip to the end of the current animation
    /// </summary>
    public void SkipToEnd()
    {
        isSkipped = true;
    }

    /// <summary>
    /// Stop animation and clear text
    /// </summary>
    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        isSkipped = false;
        targetText.maxVisibleCharacters = 0;
        targetText.text = string.Empty;
    }

    private IEnumerator AnimateText(string fullText, float charsPerSecond, Action onComplete)
    {
        if (targetText == null)
        {
            Debug.LogError("AnimatedDialogueText: TMP_Text component not found!");
            onComplete?.Invoke();
            yield break;
        }

        // Set full text for proper layout calculation
        targetText.text = fullText;
        targetText.maxVisibleCharacters = 0;

        // Force text mesh update to get accurate character count
        targetText.ForceMeshUpdate();
        int totalChars = targetText.textInfo.characterCount;

        if (totalChars == 0)
        {
            // Empty text, complete immediately
            onComplete?.Invoke();
            animationCoroutine = null;
            yield break;
        }

        float charDelay = 1f / charsPerSecond;

        for (int i = 0; i <= totalChars; i++)
        {
            if (isSkipped)
            {
                // Skip to end
                targetText.maxVisibleCharacters = totalChars;
                break;
            }

            targetText.maxVisibleCharacters = i;

            // Frame-rate independent wait
            float elapsed = 0f;
            while (elapsed < charDelay && !isSkipped)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // Ensure all characters are visible at the end
        targetText.maxVisibleCharacters = totalChars;

        onComplete?.Invoke();
        animationCoroutine = null;
    }
}
