using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Localization/Localization Database")]
public class LocalizationDatabase : ScriptableObject
{
    public string defaultLanguageCode = "en";
    public List<LanguageDefinition> languages = new List<LanguageDefinition>();
    public List<LocalizationEntry> entries = new List<LocalizationEntry>();
}

[Serializable]
public class LanguageDefinition
{
    public string code = "en";
    public string displayName = "English";
}

[Serializable]
public class LocalizationEntry
{
    public string key;
    public List<LocalizationValue> values = new List<LocalizationValue>();
}

[Serializable]
public class LocalizationValue
{
    public string languageCode = "en";
    [TextArea(2, 4)]
    public string text;
}
