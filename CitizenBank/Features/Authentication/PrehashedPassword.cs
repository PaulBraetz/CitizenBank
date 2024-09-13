namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

public readonly record struct PrehashedPassword(ImmutableArray<Byte> Digest, PrehashedPasswordParameters Parameters);