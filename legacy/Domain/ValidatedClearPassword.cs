namespace TaskforceGenerator.Domain.Authentication;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a cleartext password that has been assessed using a <see cref="IPasswordGuideline"/>.
/// </summary>
[MacroRecord(Options = RecordOptions.None)]
[Field(typeof(ClearPassword), "Password",
    Summary = "The clear password that was validated.",
    Visibility = Visibility.Public,
    Options = FieldOptions.Validated)]
[Field(typeof(IPasswordGuideline), "Guideline",
    Summary = "The guideline used to validate the password.",
    Visibility = Visibility.Public,
    Options = FieldOptions.Validated)]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ValidatedClearPassword
{
    private ValidatedClearPassword(ClearPassword password, IPasswordGuideline guideline)
    {
        Password = password;
        Guideline = guideline;

        Validity = guideline.Assess(password);
    }
    static partial void Validate(ValidateParameters parameters, ref ValidateResult result)
    {
        var (password, guideline) = parameters;

        if(guideline == null)
        {
            result.GuidelineError = "Guideline cannot be null.";
            result.GuidelineIsInvalid = true;
        }

        if(password == ClearPassword.Empty)
        {
            result.PasswordError = "Password cannot be empty.";
            result.PasswordIsInvalid = true;
        }
    }
    /// <summary>
    /// Gets the validity of this password, when assessed by <see cref="Guideline"/>.
    /// </summary>
    public readonly PasswordValidity Validity;
    /// <inheritdoc/>
    public override String ToString() => $"*** - {Validity.RulesViolated.Length} rules violated";
    /// <summary>
    /// Gets an empty instance of <see cref="ValidatedClearPassword"/>.
    /// </summary>
    public static readonly ValidatedClearPassword Empty = new(ClearPassword.Empty, PasswordGuideline.Empty);
}
