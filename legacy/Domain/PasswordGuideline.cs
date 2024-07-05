namespace TaskforceGenerator.Domain.Authentication;
/// <summary>
/// A guideline for assessing password validity; a rule composite.
/// </summary>
public sealed class PasswordGuideline : IPasswordGuideline
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="rules">The rules represented by this guideline.</param>
    public PasswordGuideline(ISet<IPasswordRule> rules)
    {
        _rules = rules;
    }

    private readonly ISet<IPasswordRule> _rules;

    /// <summary>
    /// Assesses the validity of a password.
    /// </summary>
    /// <param name="clearPassword">The password to assess.</param>
    /// <returns>The validity of the password, that is, which rules represented by the guideline are being violated by the password.</returns>
    public PasswordValidity Assess(String clearPassword)
    {
        var rulesViolated = new List<IPasswordRule>();
        foreach(var rule in _rules)
        {
            if(!rule.Matches(clearPassword))
            {
                rulesViolated.Add(rule);
            }
        }

        var result = new PasswordValidity(rulesViolated.ToArray());

        return result;
    }
}
