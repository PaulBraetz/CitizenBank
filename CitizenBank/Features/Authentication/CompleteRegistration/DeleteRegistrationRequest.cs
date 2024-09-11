namespace CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

partial record struct DeleteRegistrationRequest
{
    [UnionType<Failure, Success>]
    public readonly partial struct Result;
    public readonly struct Success;
}
