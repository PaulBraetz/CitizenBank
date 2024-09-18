namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public sealed record PrehashedPasswordParameters(
    ImmutableArray<Byte> Salt,
    Int32 HashSize,
    KeyDerivationPrf Prf,
    Int32 Iterations);
