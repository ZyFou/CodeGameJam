using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizationAutoTextUpdater : MonoBehaviour
{
    [SerializeField] private string keyPrefix = "@";
    [SerializeField] private bool includeInactive = true;
    [SerializeField] private bool rescanOnEnable = true;
    [SerializeField] private bool rescanOnLanguageChange = true;

    private readonly Dictionary<int, TMP_Text> trackedTexts = new Dictionary<int, TMP_Text>();
    private readonly Dictionary<int, string> trackedKeys = new Dictionary<int, string>();
    private readonly List<int> idsToRemove = new List<int>();

    private void OnEnable()
    {
        if (!Application.isPlaying) return;

        LocalizationManager.LanguageChanged += HandleLanguageChanged;
        if (rescanOnEnable)
        {
            Rescan();
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;

        LocalizationManager.LanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged()
    {
        if (rescanOnLanguageChange)
        {
            Rescan();
        }
        Refresh();
    }

    public void ClearCache()
    {
        trackedTexts.Clear();
        trackedKeys.Clear();
    }

    public void Rescan()
    {
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(includeInactive);
        HashSet<int> seen = new HashSet<int>();

        for (int i = 0; i < allTexts.Length; i++)
        {
            TMP_Text text = allTexts[i];
            if (text == null) continue;

            int id = text.GetInstanceID();
            seen.Add(id);
            trackedTexts[id] = text;

            if (!trackedKeys.ContainsKey(id))
            {
                string key = ExtractKey(text.text);
                if (!string.IsNullOrEmpty(key))
                {
                    trackedKeys[id] = key;
                }
            }
        }

        idsToRemove.Clear();
        foreach (KeyValuePair<int, string> pair in trackedKeys)
        {
            if (!seen.Contains(pair.Key))
            {
                idsToRemove.Add(pair.Key);
            }
        }

        for (int i = 0; i < idsToRemove.Count; i++)
        {
            int id = idsToRemove[i];
            trackedKeys.Remove(id);
            trackedTexts.Remove(id);
        }
    }

    public void Refresh()
    {
        if (trackedKeys.Count == 0) return;

        idsToRemove.Clear();
        foreach (KeyValuePair<int, string> pair in trackedKeys)
        {
            if (!trackedTexts.TryGetValue(pair.Key, out TMP_Text text) || text == null)
            {
                idsToRemove.Add(pair.Key);
                continue;
            }

            text.text = LocalizationManager.GetText(pair.Value);
        }

        for (int i = 0; i < idsToRemove.Count; i++)
        {
            int id = idsToRemove[i];
            trackedKeys.Remove(id);
            trackedTexts.Remove(id);
        }
    }

    private string ExtractKey(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return null;
        if (string.IsNullOrEmpty(keyPrefix)) return null;
        if (!rawText.StartsWith(keyPrefix)) return null;

        string key = rawText.Substring(keyPrefix.Length).Trim();
        return string.IsNullOrEmpty(key) ? null : key;
    }
}
