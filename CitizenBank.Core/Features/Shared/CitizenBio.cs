namespace CitizenBank.Features.Shared;

using RhoMicro.CodeAnalysis;

public readonly record struct CitizenBio
{
    public CitizenBio(String value)
    : this(value, validate: true) { }
    private CitizenBio(String value, Boolean validate)
    {
        if(validate)
            ArgumentException.ThrowIfNullOrEmpty(value);

        Value = value;
    }
    public String Value { get; }
    public static CitizenBio Empty { get; } = new(String.Empty, validate: false);
    public static implicit operator String(CitizenBio bio) => bio.Value;
    public static implicit operator CitizenBio(String value) => new(value);
}
