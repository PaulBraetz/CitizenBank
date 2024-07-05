namespace TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Result if a password passed for authentication does not match the password set for a citizen.
/// </summary>
public sealed class PasswordMismatchResult
{
    private PasswordMismatchResult() { }
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static PasswordMismatchResult Instance { get; } = new();
}
