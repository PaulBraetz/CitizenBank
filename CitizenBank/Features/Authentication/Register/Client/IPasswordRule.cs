namespace CitizenBank.Features.Authentication.Register.Client;

/// <summary>
/// Defines a rule that a password may either match or not.
/// </summary>
public interface IPasswordRule
{
    /// <summary>
    /// Gets the name of the rule.
    /// </summary>
    String Name { get; }
    /// <summary>
    /// Gets the description of the rule.
    /// </summary>
    String Description { get; }
    /// <summary>
    /// Evaluates whether a clear password matches the rule.
    /// </summary>
    /// <param name="password">The password to assess.</param>
    /// <returns><see langword="true"/> if the password matches this rule; otherwise <see langword="false"/>.</returns>
    Boolean Matches(ClearPassword password);
}
