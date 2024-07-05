namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Query for generating initial citizen passwords.
/// </summary>
public readonly record struct GenerateInitialPassword(CancellationToken CancellationToken) : IServiceRequest<Password>;
