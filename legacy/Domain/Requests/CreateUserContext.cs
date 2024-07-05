namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Query for creating a user context.
/// </summary>
/// <param name="CitizenName">The name of the citizen for which the user is authenticating.</param>
/// <param name="Password">The password using which the user is attempting to authenticate.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CreateUserContext(
    String CitizenName,
    String Password,
    CancellationToken CancellationToken) : IServiceRequest<IUserContext>;
