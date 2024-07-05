namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Sets the citizen connection the user is currently authenticated for.
/// </summary>
/// <param name="Connection">The citizen connection the user has been authenticated for.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct SetContextConnection(
    ICitizenConnection Connection,
    CancellationToken CancellationToken) : IServiceRequest;
