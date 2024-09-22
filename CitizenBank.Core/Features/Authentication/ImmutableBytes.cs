namespace CitizenBank.Features.Authentication;

using System.Collections;

public readonly struct ImmutableBytes : IEquatable<ImmutableBytes>
{
    private ImmutableBytes(Byte[] bytes) => _bytes = bytes;

    readonly Byte[]? _bytes;
    public Int32 Length => _bytes is null ? 0 : _bytes.Length;

    public override Boolean Equals(Object? obj) => obj is ImmutableBytes digest && Equals(digest);
    public Boolean Equals(ImmutableBytes other)
    {
        if(_bytes == other._bytes)
            return true;

        if(_bytes == null || other._bytes == null || _bytes.Length != other._bytes.Length)
            return false;

        for(var i = 0; i < _bytes.Length; i++)
        {
            if(_bytes[i] != other._bytes[i])
                return false;
        }

        return true;
    }

    public override Int32 GetHashCode()
    {
        if(_bytes is null)
            return 0;

        var hc = new HashCode();
        hc.AddBytes(_bytes);
        var result = hc.ToHashCode();

        return result;
    }

    public override String ToString() => "***";

    private Byte[] ToArray()
    {
        if(_bytes is null)
            return [];

        var result = new Byte[_bytes.Length];
        Array.Copy(_bytes, result, _bytes.Length);
        return result;
    }
    private static ImmutableBytes FromArray(Byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var result = new Byte[bytes.Length];
        Array.Copy(bytes, result, bytes.Length);
        return new(result);
    }
    public String ToBase64String() => _bytes is null ? String.Empty : Convert.ToBase64String(_bytes);
    public static ImmutableBytes FromBase64String(String digest) => Convert.FromBase64String(digest);

    public ImmutableBytes Concat(Byte[] other)
    {
        if(_bytes is null)
            return FromArray(other);

        ArgumentNullException.ThrowIfNull(other);

        var result = new Byte[_bytes.Length + other.Length];
        Array.Copy(_bytes, result, _bytes.Length);
        Array.Copy(other, 0, result, _bytes.Length, other.Length);
        return result;
    }

    public static Boolean operator ==(ImmutableBytes left, ImmutableBytes right) => left.Equals(right);
    public static Boolean operator !=(ImmutableBytes left, ImmutableBytes right) => !( left == right );

    public static implicit operator ImmutableBytes(Byte[] bytes) => FromArray(bytes);
    public static implicit operator Byte[](ImmutableBytes bytes) => bytes.ToArray();
}