namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

public readonly record struct HashedPassword(ImmutableBytes Digest, PasswordParameters Parameters, PrehashedPasswordParameters PrehashedPasswordParameters);
