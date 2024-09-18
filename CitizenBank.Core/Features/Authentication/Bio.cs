namespace CitizenBank.Features.Authentication;

/// <summary>
/// A citizens CIG profile bio.
/// </summary>
/// <param name="Content">
/// The textual content of the bio.
/// </param>
public sealed record Bio(String Content);