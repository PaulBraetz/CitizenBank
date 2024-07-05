namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Query for creating new passwords.
/// </summary>
/// <param name="ClearPassword">The clear password to hash.</param>
/// <param name="Parameters">The parameters to use when hashing.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
/// <param name="IsInitialPassword">Indicates whether the password created is the initial password and therefore exluded from password guidelines.</param>
public readonly record struct CreatePassword(
    String ClearPassword,
    PasswordParameters Parameters,
    Boolean IsInitialPassword,
    CancellationToken CancellationToken) : 
    IServiceRequest<OneOf<Password, PasswordCreationGuidelineViolatedResult>>;
