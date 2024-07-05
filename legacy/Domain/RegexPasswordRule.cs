namespace TaskforceGenerator.Domain.Authentication;
using System.Text.RegularExpressions;

/// <summary>
/// A pattern-based implementation of <see cref="IPasswordRule"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance.
/// </remarks>
/// <param name="name"></param>
/// <param name="description"></param>
/// <param name="pattern"></param>
sealed class RegexPasswordRule(String name, String description, String pattern) : IPasswordRule, IEquatable<RegexPasswordRule>
{
    /// <inheritdoc/>
    public String Name { get; } = name;
    /// <inheritdoc/>
    public String Description { get; } = description;

    private String PatternString => pattern;

    private readonly Regex _pattern = new(pattern, RegexOptions.Compiled);

    /// <inheritdoc/>
    public Boolean Matches(ClearPassword password) => _pattern.IsMatch(password);

    /// <inheritdoc/>
    public override Boolean Equals(Object? obj) => obj is RegexPasswordRule rule && Equals(rule);

    /// <inheritdoc/>
    public Boolean Equals(RegexPasswordRule? other) => other?.PatternString == PatternString;

    /// <inheritdoc/>
    public override Int32 GetHashCode() => HashCode.Combine(_pattern);

    /// <inheritdoc/>
    public static Boolean operator ==(RegexPasswordRule left, RegexPasswordRule right) => left.Equals(right);

    /// <inheritdoc/>
    public static Boolean operator !=(RegexPasswordRule left, RegexPasswordRule right) => !( left == right );
}
