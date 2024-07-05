namespace TaskforceGenerator.Domain.Authentication;

/// <summary>
/// A token used for authenticating a user as a citizen.
/// </summary>
/// <param name="Value">The token value.</param>
public readonly record struct AuthenticationToken(String Value)
{
    /// <summary>
    /// An empty instance. This is not equal to the default value of <see cref="AuthenticationToken"/>.
    /// </summary>
    public static readonly AuthenticationToken Empty = new(String.Empty);
    /// <summary>
    /// Indicates whether the token is equal to the default value of <see cref="AuthenticationToken"/> or <see cref="Empty"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the token is equal to the default value of <see cref="AuthenticationToken"/> or <see cref="Empty"/>; otherwise, <see langword="false"/>.</returns>
    public Boolean IsDefaultOrEmpty()
    {
        var result = String.IsNullOrEmpty(Value);
        return result;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator AuthenticationToken(String value) => new(value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    public static implicit operator String(AuthenticationToken code) => code.Value;
}
