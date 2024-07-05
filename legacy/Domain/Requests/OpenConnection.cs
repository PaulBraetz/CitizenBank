namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Command for creating and committing a citizen connection entity.
/// </summary>
/// <param name="CitizenName">The name of the citizen whose connection to create and commit.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct OpenConnection(
    String CitizenName,
    CancellationToken CancellationToken) : IServiceRequest<OneOf<ServiceResult, ConnectionAlreadyCommittedResult>>;
