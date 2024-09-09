namespace CitizenBank.Features.Authentication.Login.Server;
using RhoMicro.CodeAnalysis;

public partial record struct LoadRegistration
{
    [UnionType<Registration, DoesNotExist>]
    public readonly partial struct Result;
    public readonly struct DoesNotExist;
}
