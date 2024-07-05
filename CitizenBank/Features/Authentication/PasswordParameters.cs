namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

/// <summary>
/// The parameters used to create a password hash.
/// </summary>
/// <param name="Numerics">Gets numerics associated with the hash calculation.</param>
/// <param name="Data">Gets data associated with the hash calculation.</param>
public sealed record PasswordParameters(PasswordParameterNumerics Numerics, PasswordParameterData Data);

/// <summary>
/// The numerical values used in password parameters.
/// </summary>
/// <param name="Iterations">The number of iterations to apply to the hash. </param>
/// <param name="DegreeOfParallelism">The number of lanes to use while processing the hash. </param>
/// <param name="MemorySize">The number of 1kiB memory blocks to use while processing the hash. </param>
/// <param name="OutputLength">The amount of bytes in the hash value calculated. </param>
public sealed record PasswordParameterNumerics(Int32 Iterations, Int32 DegreeOfParallelism, Int32 MemorySize, Int32 OutputLength);

/// <summary>
/// Data blocks to be applied to the hash calculation.
/// </summary>
/// <param name="AssociatedData">Any extra associated data to use while hashing the password.</param>
/// <param name="KnownSecret">An optional secret to use while hashing the password.</param>
/// <param name="Salt">The password hashing salt.</param>
public sealed record PasswordParameterData(ImmutableArray<Byte> AssociatedData, ImmutableArray<Byte> KnownSecret, ImmutableArray<Byte> Salt);