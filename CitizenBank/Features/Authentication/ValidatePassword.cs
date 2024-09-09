namespace CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

partial record struct ValidatePassword
{
    [UnionType<Success, PasswordMismatch>]
    public readonly partial struct Result;
    public readonly partial struct PasswordMismatch;
}