using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private bool useTextAsKeyWhenEmpty = true;
    [SerializeField] private List<LocalizedVariable> variables = new List<LocalizedVariable>();
    [SerializeField] private List<MonoBehaviour> variableProviders = new List<MonoBehaviour>();

    private TMP_Text tmpText;
    private Text uiText;

    private void Reset()
    {
        CacheComponents();
    }

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) return;

        LocalizationManager.LanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;

        LocalizationManager.LanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        string resolvedKey = key;
        if (string.IsNullOrEmpty(resolvedKey) && useTextAsKeyWhenEmpty)
        {
            resolvedKey = GetCurrentText();
        }

        Dictionary<string, string> merged = BuildVariables();
        string localized = LocalizationManager.GetText(resolvedKey, merged);
        SetText(localized);
    }

    private Dictionary<string, string> BuildVariables()
    {
        Dictionary<string, string> merged = null;

        if (variableProviders != null)
        {
            for (int i = 0; i < variableProviders.Count; i++)
            {
                MonoBehaviour provider = variableProviders[i];
                if (provider is ILocalizationVariableProvider source)
                {
                    if (merged == null)
                    {
                        merged = new Dictionary<string, string>();
                    }
                    source.PopulateVariables(merged);
                }
            }
        }

        if (variables != null)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                LocalizedVariable variable = variables[i];
                if (string.IsNullOrEmpty(variable.key)) continue;

                if (merged == null)
                {
                    merged = new Dictionary<string, string>();
                }
                merged[variable.key] = variable.value ?? string.Empty;
            }
        }

        return merged;
    }

    private void CacheComponents()
    {
        if (tmpText == null)
        {
            tmpText = GetComponent<TMP_Text>();
        }

        if (uiText == null)
        {
            uiText = GetComponent<Text>();
        }
    }

    private string GetCurrentText()
    {
        if (tmpText != null)
        {
            return tmpText.text;
        }

        if (uiText != null)
        {
            return uiText.text;
        }

        return string.Empty;
    }

    private void SetText(string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;
        }
        else if (uiText != null)
        {
            uiText.text = value;
        }
    }
}
