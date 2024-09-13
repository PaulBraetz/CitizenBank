namespace CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.CodeAnalysis;

partial record struct ValidatePrehashedPasswordParameters
{
    [UnionType<Success, Insecure>]
    public readonly partial struct Result;
    public readonly struct Insecure;
}