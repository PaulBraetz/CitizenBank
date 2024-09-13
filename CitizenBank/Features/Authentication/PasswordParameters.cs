namespace CitizenBank.Features.Authentication;

/// <summary>
/// The parameters used to create a password hash.
/// </summary>
/// <param name="Numerics">Gets numerics associated with the hash calculation.</param>
/// <param name="Data">Gets data associated with the hash calculation.</param>
public sealed record PasswordParameters(PasswordParameterNumerics Numerics, PasswordParameterData Data);
