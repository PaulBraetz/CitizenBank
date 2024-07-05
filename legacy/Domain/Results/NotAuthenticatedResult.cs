namespace TaskforceGenerator.Domain.Authentication.Results;
/// <summary>
/// Result if a citizen is required to be available from the user context but none have been authenticated.
/// </summary>
public sealed class NotAuthenticatedResult : AwaitableResultBase<NotAuthenticatedResult>
{
    private NotAuthenticatedResult() { }
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static NotAuthenticatedResult Instance { get; } = new();
}
