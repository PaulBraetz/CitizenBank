namespace TaskforceGenerator.Domain.Authentication;

/// <summary>
/// A citizen password.
/// </summary>
/// <param name="Hash">The hash</param>
/// <param name="Parameters"></param>
public readonly record struct Password(Byte[] Hash, PasswordParameters Parameters);
