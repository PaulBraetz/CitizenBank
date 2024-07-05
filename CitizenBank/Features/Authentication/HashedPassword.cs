namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

public readonly record struct HashedPassword(ImmutableArray<Byte> Hash, PasswordParameters Parameters);
