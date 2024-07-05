namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Query for creating citizen connection entities (factory query).
/// </summary>
/// <param name="CitizenName">The name of the citizen whose connection to create.</param>
/// <param name="Code">The connections initial bio code.</param>
/// <param name="Password">The connections initial (random) password.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CreateConnection(
    String CitizenName,
    BioCode Code,
    Password Password,
    CancellationToken CancellationToken) : IServiceRequest<ICitizenConnection>;
