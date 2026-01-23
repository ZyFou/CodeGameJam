using System.Collections.Generic;

public interface ILocalizationVariableProvider
{
    void PopulateVariables(Dictionary<string, string> variables);
}
