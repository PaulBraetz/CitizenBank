namespace TaskforceGenerator.Domain.Authentication.Queries;
/// <summary>
/// Query for checking whether a citizens connection already exists.
/// </summary>
/// <param name="CitizenName">The name of the citizen whose connection to check for.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CheckConnectionExists(
    String CitizenName,
    CancellationToken CancellationToken) : IServiceRequest<Boolean>;
