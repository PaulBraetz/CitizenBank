namespace CitizenBank.Features.Shared;

using RhoMicro.CodeAnalysis;

public readonly record struct CitizenImagePath
{
    public CitizenImagePath(String value)
    : this(value, validate: true) { }
    private CitizenImagePath(String value, Boolean validate)
    {
        if(validate)
            ArgumentException.ThrowIfNullOrEmpty(value);

        Value = value;
    }
    public String Value { get; }
    public static CitizenImagePath Empty { get; } = new(String.Empty, validate: false);
    public static implicit operator String(CitizenImagePath path) => path.Value;
    public static implicit operator CitizenImagePath(String value) => new(value);
}