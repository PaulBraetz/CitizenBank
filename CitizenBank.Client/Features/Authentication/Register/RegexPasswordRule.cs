namespace CitizenBank.Features.Authentication.Register;
using System.Text.RegularExpressions;

sealed class RegexPasswordRule : IPasswordRule, IEquatable<RegexPasswordRule>
{
    /// <inheritdoc/>
    public required String Name { get; set; }
    /// <inheritdoc/>
    public required String Description { get; set; }
    private String? _patternString;
    private Regex? _pattern;
    public required String PatternString 
    {
        get => _patternString!; 
        set
        {
            _patternString = value;
            _pattern = new(value, RegexOptions.Compiled);
        }
    }

    /// <inheritdoc/>
    public Boolean Matches(ClearPassword password) => _pattern!.IsMatch(password);

    /// <inheritdoc/>
    public override Boolean Equals(Object? obj) => obj is RegexPasswordRule rule && Equals(rule);

    /// <inheritdoc/>
    public Boolean Equals(RegexPasswordRule? other) => other?.PatternString == PatternString;

    /// <inheritdoc/>
    public override Int32 GetHashCode() => HashCode.Combine(_pattern);

    /// <inheritdoc/>
    public static Boolean operator ==(RegexPasswordRule left, RegexPasswordRule right)
    {
        ArgumentNullException.ThrowIfNull(left);

        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static Boolean operator !=(RegexPasswordRule left, RegexPasswordRule right) => !( left == right );
}
