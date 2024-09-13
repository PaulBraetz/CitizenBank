namespace CitizenBank.Features.Authentication;

using System.Collections.Immutable;

using RhoMicro.CodeAnalysis;

public readonly record struct PrehashedPassword(ImmutableArray<Byte> Bytes, PrehashedPasswordParameters Parameters);