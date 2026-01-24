using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeInOnStart : MonoBehaviour
{
    [Tooltip("Image plein écran (noire) dans un Canvas overlay, alpha=1 au départ.")]
    [SerializeField]
    private Image fadeImage;

    [SerializeField]
    private float fadeDuration = 0.6f;

    void Awake()
    {
        if (fadeImage != null)
        {
            // Démarre noir
            var c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;

            // bloque les clics au tout début, puis on libère
            fadeImage.raycastTarget = true;
        }
    }

    void Start()
    {
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        if (fadeImage == null)
            yield break;

        yield return Fade(1f, 0f, fadeDuration);
        fadeImage.raycastTarget = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        var c = fadeImage.color;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}
