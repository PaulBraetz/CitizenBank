namespace CitizenBank.Features.Authentication;

using RhoMicro.CodeAnalysis;

partial record struct ValidatePassword
{
    [UnionType<Success, Mismatch>]
    public readonly partial struct Result;
    public readonly struct Mismatch;
    public readonly struct Success;
}