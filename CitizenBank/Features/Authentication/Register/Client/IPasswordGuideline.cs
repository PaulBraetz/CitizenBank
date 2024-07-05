namespace CitizenBank.Features.Authentication.Register.Client;

/// <summary>
/// represents a guideline for assessing password validity; a rule composite.
/// </summary>
public interface IPasswordGuideline
{
    /// <summary>
    /// Assesses the validity of a password.
    /// </summary>
    /// <param name="password">The password to assess.</param>
    /// <returns>The validity of the password, that is, which rules represented by the guideline are being violated by the password.</returns>
    PasswordValidity Assess(ClearPassword password);
}