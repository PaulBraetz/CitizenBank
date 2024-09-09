namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

using RhoMicro.CodeAnalysis;

[UnionType<ImmutableArray<Byte>>]
public readonly partial struct PrehashedPassword
{
    public Boolean Equals(PrehashedPassword other) =>
        AsImmutableArray_of_Byte.IsDefaultOrEmpty == other.AsImmutableArray_of_Byte.IsDefaultOrEmpty
        && other.AsImmutableArray_of_Byte.SequenceEqual(AsImmutableArray_of_Byte);
    /// <inheritdoc/>
    public override Int32 GetHashCode()
    {
        var thisArray = AsImmutableArray_of_Byte;

        if(thisArray.IsDefault)
            return 0;

        var hc = new HashCode();
        for(var i = 0; i < thisArray.Length; i++)
        {
            hc.Add(thisArray[i]);
        }

        var result = hc.ToHashCode();

        return result;
    }
    /// <inheritdoc/>
    public override String ToString() => "***";
}