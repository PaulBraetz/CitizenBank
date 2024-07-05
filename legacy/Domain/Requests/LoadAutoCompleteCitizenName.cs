namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Query for retrieving a citizen name that is known to the system as well as being the closest larger neighbor to the name provided.
/// This functionality is intended to be used for the purpose of autocompletion primarily.
/// </summary>
/// <param name="CitizenName">The name whose registered neighbor to load.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct LoadAutoCompleteCitizenName(
    String CitizenName,
    CancellationToken CancellationToken) : IServiceRequest<String?>;
