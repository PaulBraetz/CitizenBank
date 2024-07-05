namespace TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Result if the system is unable to commit a connection to the infrastructure 
/// because a connection with the same name has already been committed.
/// </summary>
public sealed class ConnectionAlreadyCommittedResult
{
    private ConnectionAlreadyCommittedResult() { }
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ConnectionAlreadyCommittedResult Instance { get; } = new();
}
