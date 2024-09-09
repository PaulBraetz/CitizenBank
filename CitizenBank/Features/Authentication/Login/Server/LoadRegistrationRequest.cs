namespace CitizenBank.Features.Authentication.Login.Server;

using RhoMicro.CodeAnalysis;

partial record struct LoadRegistrationRequest
{
    [UnionType<RegistrationRequest, DoesNotExist>]
    public readonly partial struct Result;
    public readonly struct DoesNotExist;
}