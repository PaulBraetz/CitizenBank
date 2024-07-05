namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Command for authenticating the user as for a connection.
/// </summary>
/// <param name="Connection">The citizen connection for which to authenticate the user.</param>
/// <param name="ClearPassword">The password provided by the user.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct Authenticate(
    ICitizenConnection Connection,
    String ClearPassword,
    CancellationToken CancellationToken) : IServiceRequest<OneOf<ServiceResult, PasswordMismatchResult>>;
