namespace CitizenBank.Features.Shared;

using System.Text.RegularExpressions;

using RhoMicro.CodeAnalysis;

public readonly partial record struct CitizenName
{
    public CitizenName(String value)
    : this(value, validate: true) { }
    private CitizenName(String value, Boolean validate)
    {
        if(validate)
            ArgumentException.ThrowIfNullOrEmpty(value);
        //TODO: pattern validation
        Value = value;
    }
    public String Value { get; }
    public static CitizenName Empty { get; } = new(String.Empty, validate: false);

    public static implicit operator String(CitizenName name) => name.Value;
    public static implicit operator CitizenName(String value) => new(value);
}
