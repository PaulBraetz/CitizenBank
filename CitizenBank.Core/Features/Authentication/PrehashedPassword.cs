namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

public readonly record struct PrehashedPassword(ImmutableBytes Digest, PrehashedPasswordParameters Parameters);