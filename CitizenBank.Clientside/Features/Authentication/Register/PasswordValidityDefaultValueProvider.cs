namespace CitizenBank.Features.Authentication.Register;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class PasswordValidityDefaultValueProvider : IDefaultValueProvider<PasswordValidity>
{
    public PasswordValidity GetDefault() => PasswordValidity.Empty;
}