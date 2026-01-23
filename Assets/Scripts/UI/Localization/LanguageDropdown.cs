using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown tmpDropdown;
    [SerializeField] private Dropdown uiDropdown;

    private readonly List<string> languageCodes = new List<string>();
    private bool suppressCallback;

    private void Reset()
    {
        if (tmpDropdown == null)
        {
            tmpDropdown = GetComponent<TMP_Dropdown>();
        }

        if (uiDropdown == null)
        {
            uiDropdown = GetComponent<Dropdown>();
        }
    }

    private void Awake()
    {
        if (tmpDropdown == null)
        {
            tmpDropdown = GetComponent<TMP_Dropdown>();
        }

        if (uiDropdown == null)
        {
            uiDropdown = GetComponent<Dropdown>();
        }
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) return;

        PopulateOptions();
        HookEvents();
        SyncSelection();
        LocalizationManager.LanguageChanged += SyncSelection;
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;

        LocalizationManager.LanguageChanged -= SyncSelection;
        UnhookEvents();
    }

    private void PopulateOptions()
    {
        IReadOnlyList<LanguageDefinition> languages = LocalizationManager.GetLanguages();
        languageCodes.Clear();

        if (languages == null || languages.Count == 0)
        {
            return;
        }

        if (tmpDropdown != null)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < languages.Count; i++)
            {
                LanguageDefinition def = languages[i];
                if (def == null || string.IsNullOrEmpty(def.code)) continue;

                string code = def.code.Trim();
                languageCodes.Add(code);
                string label = string.IsNullOrEmpty(def.displayName) ? code : def.displayName;
                options.Add(new TMP_Dropdown.OptionData(label));
            }

            tmpDropdown.options = options;
        }

        if (uiDropdown != null)
        {
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for (int i = 0; i < languages.Count; i++)
            {
                LanguageDefinition def = languages[i];
                if (def == null || string.IsNullOrEmpty(def.code)) continue;

                string code = def.code.Trim();
                if (!languageCodes.Contains(code))
                {
                    languageCodes.Add(code);
                }

                string label = string.IsNullOrEmpty(def.displayName) ? code : def.displayName;
                options.Add(new Dropdown.OptionData(label));
            }

            uiDropdown.options = options;
        }
    }

    private void HookEvents()
    {
        if (tmpDropdown != null)
        {
            tmpDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        if (uiDropdown != null)
        {
            uiDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    private void UnhookEvents()
    {
        if (tmpDropdown != null)
        {
            tmpDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        if (uiDropdown != null)
        {
            uiDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
    }

    private void OnDropdownChanged(int index)
    {
        if (suppressCallback) return;
        if (index < 0 || index >= languageCodes.Count) return;

        LocalizationManager.SetLanguage(languageCodes[index]);
    }

    private void SyncSelection()
    {
        if (languageCodes.Count == 0) return;

        string current = LocalizationManager.CurrentLanguageCode;
        int index = -1;
        for (int i = 0; i < languageCodes.Count; i++)
        {
            if (string.Equals(languageCodes[i], current, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }
        if (index < 0) index = 0;

        suppressCallback = true;
        if (tmpDropdown != null)
        {
            tmpDropdown.value = index;
        }

        if (uiDropdown != null)
        {
            uiDropdown.value = index;
        }
        suppressCallback = false;
    }
}
