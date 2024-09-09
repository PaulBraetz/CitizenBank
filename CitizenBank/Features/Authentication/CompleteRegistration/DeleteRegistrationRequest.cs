namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.CodeAnalysis;

partial record struct DeleteRegistrationRequest
{
    [UnionType<DeleteRegistrationRequestFailure, Success>]
    public readonly partial struct Result;
    public readonly struct DeleteRegistrationRequestFailure;
    public readonly struct Success;
}
