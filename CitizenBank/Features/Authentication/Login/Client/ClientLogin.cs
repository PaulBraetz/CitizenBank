namespace CitizenBank.Features.Authentication.Login.Client;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

partial record struct ClientLogin
{
    [UnionType<ValidatePassword.PasswordMismatch, Failure, Success>]
    public readonly partial struct Result;
}