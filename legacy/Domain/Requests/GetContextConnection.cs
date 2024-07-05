namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Query for retrieving the citizen connection associated with the users context.
/// </summary>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct GetContextConnection(CancellationToken CancellationToken) :
    IServiceRequest<OneOf<ICitizenConnection, NotAuthenticatedResult>>;
