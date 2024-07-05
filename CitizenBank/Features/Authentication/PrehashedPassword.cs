namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

using RhoMicro.CodeAnalysis;

[UnionType<ImmutableArray<Byte>>]
public readonly partial struct PrehashedPassword
{
    public Boolean Equals(PrehashedPassword other) =>
        AsImmutableArray_of_Byte.IsDefaultOrEmpty == other.AsImmutableArray_of_Byte.IsDefaultOrEmpty
        && other.AsImmutableArray_of_Byte.SequenceEqual(AsImmutableArray_of_Byte);
}