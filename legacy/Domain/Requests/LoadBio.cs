namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Query for loading a citizen's bio.
/// </summary>
/// <param name="Name">The name of the citizen whose bio to load.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct LoadBio(
    String Name,
    CancellationToken CancellationToken)
    : IServiceRequest<OneOf<IBio, CitizenNotExistingResult>>;
