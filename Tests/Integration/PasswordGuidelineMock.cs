namespace Tests.Integration;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Register;

using RhoMicro.ApplicationFramework.Composition;

[FakeService]
class PasswordGuidelineMock(Func<ClearPassword, PasswordValidity> impl) : IPasswordGuideline
{
    public PasswordValidity Assess(ClearPassword password) => impl.Invoke(password);
}