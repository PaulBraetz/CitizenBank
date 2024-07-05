namespace CitizenBank.Features.Authentication.Register.Client;

using System.Collections.Immutable;

/// <summary>
/// The result of a password guideline assessment.
/// </summary>
/// <remarks>
/// Initializes a new instance.
/// </remarks>
/// <param name="RulesViolated">The violated rules to be represented by this instance.</param>
public readonly record struct PasswordValidity(ImmutableArray<IPasswordRule> RulesViolated) : IEquatable<PasswordValidity>
{
    /// <summary>
    /// An empty instance of <see cref="PasswordValidity"/>.
    /// </summary>
    public static readonly PasswordValidity Empty = new([]);

    /// <summary>
    /// Indicates whether the validity is equal to the default value of <see cref="PasswordValidity"/> or <see cref="Empty"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the validity is equal to the default value of <see cref="PasswordValidity"/> or <see cref="Empty"/>; otherwise, <see langword="false"/>.</returns>
    public Boolean IsValid => RulesViolated.IsDefaultOrEmpty;

    public Boolean Equals(PasswordValidity other) =>
        RulesViolated.IsDefaultOrEmpty == other.RulesViolated.IsDefaultOrEmpty
        && RulesViolated.SequenceEqual(other.RulesViolated);
    public override Int32 GetHashCode() =>
        RulesViolated.IsDefaultOrEmpty
        ? 0
        : RulesViolated.Aggregate(new HashCode(), (h, r) =>
        {
            h.Add(r);
            return h;
        }).ToHashCode();
}
