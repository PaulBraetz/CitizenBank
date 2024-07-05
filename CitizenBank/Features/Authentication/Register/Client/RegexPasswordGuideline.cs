namespace CitizenBank.Features.Authentication.Register.Client;

/// <summary>
/// A guideline for assessing password validity; a rule composite for configuration-based use.
/// </summary>
public sealed class RegexPasswordGuideline : IPasswordGuideline
{
    public required List<RegexPasswordRule> Rules { get; set; }
    /// <inheritdoc/>
    public PasswordValidity Assess(ClearPassword password)
    {
        var rulesViolated = new List<IPasswordRule>();
        foreach(var rule in Rules)
        {
            if(!rule.Matches(password))
            {
                rulesViolated.Add(rule);
            }
        }

        var result = new PasswordValidity([.. rulesViolated]);

        return result;
    }
}
