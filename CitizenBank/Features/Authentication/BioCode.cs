namespace CitizenBank.Features.Authentication;

/// <summary>
/// A bio code used for verifying an action related to a specific citizen.
/// In order to verify an action, the bio code value must be posted to the citizens bio.
/// </summary>
/// <param name="Value">The bio code value.</param>
public readonly record struct BioCode(String Value)
{
    /// <summary>
    /// An empty instance. This is not equal to the default value of <see cref="BioCode"/>.
    /// </summary>
    public static readonly BioCode Empty = new(String.Empty);
    /// <summary>
    /// Indicates whether the code is equal to the default value of <see cref="BioCode"/> or <see cref="Empty"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the code is equal to the default value of <see cref="BioCode"/> or <see cref="Empty"/>; otherwise, <see langword="false"/>.</returns>
    public Boolean IsDefaultOrEmpty() => String.IsNullOrEmpty(Value);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator BioCode(String value) => new(value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    public static implicit operator String(BioCode code) => code.Value;
}
