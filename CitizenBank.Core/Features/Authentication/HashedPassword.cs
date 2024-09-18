namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

public readonly record struct HashedPassword(ImmutableArray<Byte> Digest, PasswordParameters Parameters, PrehashedPasswordParameters PrehashedPasswordParameters);
