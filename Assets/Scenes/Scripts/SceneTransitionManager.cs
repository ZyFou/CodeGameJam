using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFadeLoader : MonoBehaviour
{
    [Header("Fade UI")]
    [Tooltip("Image plein écran (noire) dans un Canvas overlay, alpha=0 au départ.")]
    [SerializeField]
    private Image fadeImage;

    [Tooltip("Canvas à désactiver juste avant de charger la scène (ex: ton menu UI).")]
    [SerializeField]
    private Canvas canvasToDisable;

    [Header("Load")]
    [Tooltip("Nom exact de la scène à charger (elle doit être dans Build Settings).")]
    [SerializeField]
    private string sceneToLoad;

    [Header("Timings")]
    [SerializeField]
    private float fadeDuration = 0.5f;

    bool isLoading;

    void Awake()
    {
        if (fadeImage != null)
        {
            // s'assure qu'on démarre visible (alpha 0)
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;

            // Important: qu'elle bloque pas les clics tant que alpha=0
            fadeImage.raycastTarget = false;
        }
    }

    // À connecter dans le bouton (OnClick)
    public void LoadSceneWithFade()
    {
        if (isLoading)
            return;
        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        isLoading = true;

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true; // bloque les clics pendant la transition
            yield return Fade(0f, 1f, fadeDuration);
        }

        if (canvasToDisable != null)
            canvasToDisable.enabled = false; // ou canvasToDisable.gameObject.SetActive(false);

        // Charge la scène
        if (!string.IsNullOrWhiteSpace(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogError("[SceneFadeLoader] sceneToLoad est vide.");

        // (Optionnel) si tu utilises un écran de fade persistant entre scènes, c’est un autre setup.
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null)
            yield break;

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
