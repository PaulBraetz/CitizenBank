namespace TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Query for creating new guideline validated passwords.
/// </summary>
/// <param name="Password">The validated clear password to hash.</param>
/// <param name="Parameters">The parameters to use when hashing.</param>
/// <param name="CancellationToken">The token used to signal the service execution to be cancelled.</param>
public readonly record struct CreateValidatedPassword(
    ValidatedClearPassword Password,
    PasswordParameters Parameters,
    CancellationToken CancellationToken) :
    IServiceRequest<OneOf<Password, PasswordCreationGuidelineViolatedResult>>;
