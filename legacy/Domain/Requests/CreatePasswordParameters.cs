namespace TaskforceGenerator.Domain.Authentication.Requests;
/// <summary>
/// Query for creating new password parameters.
/// </summary>
public readonly record struct CreatePasswordParameters(CancellationToken CancellationToken) : IServiceRequest<PasswordParameters>;
