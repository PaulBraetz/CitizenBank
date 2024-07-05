namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Query for reconstituting a single citizen connection matching the query.
/// </summary>
/// <param name="CitizenName">The name of the citizen whose connection to retrieve.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct ReconstituteConnection(
    String CitizenName,
    CancellationToken CancellationToken) :
    IServiceRequest<OneOf<ICitizenConnection, CitizenNotRegisteredResult>>;
