namespace CitizenBank.Features.Authentication.Login.Client;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

partial record struct ClientLogin
{
    [UnionType<ValidatePassword.Mismatch, Failure, Success>]
    public readonly partial struct Result;
    public readonly struct Success;
}