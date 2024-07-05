namespace TaskforceGenerator.Domain.Authentication.Results;
/// <summary>
/// Result for when a new password is attempted to be created with an invalid clear password.
/// </summary>
/// <param name="Validity">The validity of the clear password.</param>
public readonly record struct PasswordCreationGuidelineViolatedResult(PasswordValidity Validity);
