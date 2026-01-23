using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static event Action LanguageChanged;

    private static LocalizationManager instance;

    [Header("Database")]
    [SerializeField] private LocalizationDatabase database;
    [SerializeField] private string resourcesDatabasePath = "Localization/LocalizationDatabase";
    [SerializeField] private string defaultLanguageCode = "en";

    [Header("Persistence")]
    [SerializeField] private bool saveSelection = true;
    [SerializeField] private string playerPrefsKey = "Localization.Language";

    private readonly Dictionary<string, Dictionary<string, string>> table =
        new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
    private readonly Dictionary<string, string> globalVariables =
        new Dictionary<string, string>(StringComparer.Ordinal);

    private string currentLanguageCode;
    private bool isInitialized;

    public static string CurrentLanguageCode
    {
        get
        {
            LocalizationManager manager = EnsureInstance();
            return manager.currentLanguageCode;
        }
    }

    public static IReadOnlyList<LanguageDefinition> GetLanguages()
    {
        LocalizationManager manager = EnsureInstance();
        if (manager.database == null || manager.database.languages == null)
        {
            return new LanguageDefinition[0];
        }

        return manager.database.languages;
    }

    public static string GetText(string key, IDictionary<string, string> variables = null)
    {
        LocalizationManager manager = EnsureInstance();
        return manager.GetTextInternal(key, variables);
    }

    public static void SetLanguage(string languageCode)
    {
        LocalizationManager manager = EnsureInstance();
        manager.SetLanguageInternal(languageCode, true);
    }

    public static void SetGlobalVariable(string key, string value, bool refresh = true)
    {
        if (string.IsNullOrEmpty(key)) return;

        LocalizationManager manager = EnsureInstance();
        manager.globalVariables[key] = value ?? string.Empty;

        if (refresh)
        {
            RefreshAll();
        }
    }

    public static void ClearGlobalVariable(string key, bool refresh = true)
    {
        if (string.IsNullOrEmpty(key)) return;

        LocalizationManager manager = EnsureInstance();
        if (manager.globalVariables.Remove(key) && refresh)
        {
            RefreshAll();
        }
    }

    public static void ClearGlobalVariables(bool refresh = true)
    {
        LocalizationManager manager = EnsureInstance();
        manager.globalVariables.Clear();

        if (refresh)
        {
            RefreshAll();
        }
    }

    public static void RefreshAll()
    {
        LanguageChanged?.Invoke();
    }

    private static LocalizationManager EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindObjectOfType<LocalizationManager>();
        if (instance == null)
        {
            GameObject go = new GameObject("LocalizationManager");
            instance = go.AddComponent<LocalizationManager>();
        }

        instance.Initialize();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        if (database == null && !string.IsNullOrEmpty(resourcesDatabasePath))
        {
            database = Resources.Load<LocalizationDatabase>(resourcesDatabasePath);
        }

        BuildLookupTable();

        string initialLanguage = GetDefaultLanguageCode();
        if (saveSelection && PlayerPrefs.HasKey(playerPrefsKey))
        {
            string saved = PlayerPrefs.GetString(playerPrefsKey, initialLanguage);
            initialLanguage = string.IsNullOrEmpty(saved) ? initialLanguage : saved;
        }

        SetLanguageInternal(initialLanguage, false);
        isInitialized = true;
    }

    private void BuildLookupTable()
    {
        table.Clear();
        if (database == null || database.entries == null) return;

        for (int i = 0; i < database.entries.Count; i++)
        {
            LocalizationEntry entry = database.entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.key)) continue;

            Dictionary<string, string> perLanguage =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (entry.values != null)
            {
                for (int j = 0; j < entry.values.Count; j++)
                {
                    LocalizationValue value = entry.values[j];
                    if (value == null || string.IsNullOrEmpty(value.languageCode)) continue;

                    string code = NormalizeLanguageCode(value.languageCode);
                    if (string.IsNullOrEmpty(code)) continue;

                    perLanguage[code] = value.text ?? string.Empty;
                }
            }

            table[entry.key] = perLanguage;
        }
    }

    private void SetLanguageInternal(string languageCode, bool notify)
    {
        string normalized = NormalizeLanguageCode(languageCode);
        if (string.IsNullOrEmpty(normalized))
        {
            normalized = GetDefaultLanguageCode();
        }

        if (!IsKnownLanguage(normalized))
        {
            normalized = GetDefaultLanguageCode();
        }

        if (string.Equals(currentLanguageCode, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        currentLanguageCode = normalized;

        if (saveSelection)
        {
            PlayerPrefs.SetString(playerPrefsKey, normalized);
        }

        if (notify)
        {
            LanguageChanged?.Invoke();
        }
    }

    private bool IsKnownLanguage(string languageCode)
    {
        if (database == null || database.languages == null || database.languages.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < database.languages.Count; i++)
        {
            LanguageDefinition def = database.languages[i];
            if (def == null || string.IsNullOrEmpty(def.code)) continue;

            if (string.Equals(def.code, languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private string GetDefaultLanguageCode()
    {
        if (database != null && !string.IsNullOrEmpty(database.defaultLanguageCode))
        {
            return NormalizeLanguageCode(database.defaultLanguageCode);
        }

        return NormalizeLanguageCode(defaultLanguageCode);
    }

    private static string NormalizeLanguageCode(string code)
    {
        return string.IsNullOrEmpty(code) ? string.Empty : code.Trim();
    }

    private string GetTextInternal(string key, IDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        string result = null;
        if (table.TryGetValue(key, out Dictionary<string, string> perLanguage))
        {
            if (!string.IsNullOrEmpty(currentLanguageCode) &&
                perLanguage.TryGetValue(currentLanguageCode, out string text))
            {
                result = text;
            }
            else
            {
                string fallbackCode = GetDefaultLanguageCode();
                if (!string.IsNullOrEmpty(fallbackCode) &&
                    perLanguage.TryGetValue(fallbackCode, out string fallbackText))
                {
                    result = fallbackText;
                }
            }
        }

        if (string.IsNullOrEmpty(result))
        {
            result = key;
        }

        if (globalVariables.Count == 0 && (variables == null || variables.Count == 0))
        {
            return result;
        }

        Dictionary<string, string> merged = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (KeyValuePair<string, string> pair in globalVariables)
        {
            merged[pair.Key] = pair.Value;
        }

        if (variables != null)
        {
            foreach (KeyValuePair<string, string> pair in variables)
            {
                if (string.IsNullOrEmpty(pair.Key)) continue;
                merged[pair.Key] = pair.Value ?? string.Empty;
            }
        }

        return ReplaceVariables(result, merged);
    }

    private static string ReplaceVariables(string text, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(text) || variables == null || variables.Count == 0)
        {
            return text;
        }

        StringBuilder builder = new StringBuilder(text.Length);
        int length = text.Length;

        for (int i = 0; i < length; i++)
        {
            char c = text[i];
            if (c == '{')
            {
                int end = text.IndexOf('}', i + 1);
                if (end > i + 1)
                {
                    string token = text.Substring(i + 1, end - i - 1);
                    if (variables.TryGetValue(token, out string value))
                    {
                        builder.Append(value);
                    }
                    else
                    {
                        builder.Append('{').Append(token).Append('}');
                    }
                    i = end;
                    continue;
                }
            }

            builder.Append(c);
        }

        return builder.ToString();
    }
}
