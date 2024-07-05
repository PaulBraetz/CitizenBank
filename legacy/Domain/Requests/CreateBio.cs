namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Query for creating instances of <see cref="IBio"/>.
/// </summary>
/// <param name="BioText">The text representing the ctizens bio.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CreateBio(String BioText, CancellationToken CancellationToken) : IServiceRequest<IBio>;
