using System.Collections.Generic;
using UnityEngine;

public class LocalizationVariables : MonoBehaviour, ILocalizationVariableProvider
{
    [SerializeField] private List<LocalizedVariable> variables = new List<LocalizedVariable>();
    [SerializeField] private bool refreshOnChange = true;

    public void SetVariable(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;

        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].key == key)
            {
                variables[i] = new LocalizedVariable { key = key, value = value };
                if (refreshOnChange) LocalizationManager.RefreshAll();
                return;
            }
        }

        variables.Add(new LocalizedVariable { key = key, value = value });
        if (refreshOnChange) LocalizationManager.RefreshAll();
    }

    public bool TryGetVariable(string key, out string value)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].key == key)
            {
                value = variables[i].value;
                return true;
            }
        }

        value = null;
        return false;
    }

    public void RemoveVariable(string key, bool refresh = true)
    {
        for (int i = variables.Count - 1; i >= 0; i--)
        {
            if (variables[i].key == key)
            {
                variables.RemoveAt(i);
            }
        }

        if (refresh && refreshOnChange)
        {
            LocalizationManager.RefreshAll();
        }
    }

    public void PopulateVariables(Dictionary<string, string> target)
    {
        if (target == null) return;

        for (int i = 0; i < variables.Count; i++)
        {
            LocalizedVariable variable = variables[i];
            if (string.IsNullOrEmpty(variable.key)) continue;

            target[variable.key] = variable.value ?? string.Empty;
        }
    }
}
