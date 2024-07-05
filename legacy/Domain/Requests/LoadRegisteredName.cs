namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Query for loading a citizens actual name. Citizens may be specified using case-insensitivity, however they should always be represented using the RSI-registered typing.
/// </summary>
/// <param name="CitizenName">The name of the citizen whose actual name to load.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct LoadActualName(
    String CitizenName,
    CancellationToken CancellationToken) : 
    IServiceRequest<OneOf<String, CitizenNotExistingResult>>;
