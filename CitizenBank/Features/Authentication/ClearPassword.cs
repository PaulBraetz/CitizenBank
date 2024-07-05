namespace CitizenBank.Features.Authentication;

using RhoMicro.CodeAnalysis;

/// <summary>
/// Represents a cleartext password.
/// </summary>
[UnionType<String>]
public readonly partial struct ClearPassword
{
    static ClearPassword Create([UnionTypeFactory] String value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ClearPassword(value);
    }

    /// <summary>
    /// Gets an empty clear password.
    /// </summary>
    public static readonly ClearPassword Empty = new(String.Empty);
}
