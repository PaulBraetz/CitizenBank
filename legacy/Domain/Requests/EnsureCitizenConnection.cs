namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Command for ensuring a citizen connection is available to the system.
/// </summary>
/// <param name="CitizenName">The name of the citizen whose connection to ensure exists.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct EnsureCitizenConnection(
    String CitizenName,
    CancellationToken CancellationToken) : IServiceRequest<ServiceResult>;
