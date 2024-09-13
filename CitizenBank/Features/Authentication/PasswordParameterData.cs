namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

/// <summary>
/// Data blocks to be applied to the hash calculation.
/// </summary>
/// <param name="AssociatedData">Any extra associated data to use while hashing the password.</param>
/// <param name="KnownSecret">An optional secret to use while hashing the password.</param>
/// <param name="Salt">The password hashing salt.</param>
public sealed record PasswordParameterData(ImmutableArray<Byte> AssociatedData, ImmutableArray<Byte> KnownSecret, ImmutableArray<Byte> Salt);
