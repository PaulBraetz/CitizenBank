namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Command for committing connections to the infrastructure.
/// </summary>
/// <param name="Connection">The connection to commit.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CommitConnection(
    ICitizenConnection Connection,
    CancellationToken CancellationToken) : 
    IServiceRequest<OneOf<ServiceResult, ConnectionAlreadyCommittedResult>>;
