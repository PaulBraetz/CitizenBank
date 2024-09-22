namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public sealed record PrehashedPasswordParameters(
    ImmutableBytes Salt,
    Int32 HashSize,
    KeyDerivationPrf Prf,
    Int32 Iterations);
