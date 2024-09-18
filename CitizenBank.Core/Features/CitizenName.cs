namespace CitizenBank.Features;

using RhoMicro.CodeAnalysis;

[UnionType<String>]
public readonly partial struct CitizenName
{
    public static CitizenName Empty { get; } = String.Empty;
    static CitizenName Create([UnionTypeFactory] String value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new CitizenName(value);
    }
}