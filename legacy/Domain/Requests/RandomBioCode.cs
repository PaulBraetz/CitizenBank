namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Query for creating a random bio code.
/// </summary>
public readonly record struct GenerateRandomBioCode(CancellationToken CancellationToken) : IServiceRequest<BioCode> { }
