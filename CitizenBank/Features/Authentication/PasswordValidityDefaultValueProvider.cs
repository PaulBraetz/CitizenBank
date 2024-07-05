namespace CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Register.Client;

using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class PasswordValidityDefaultValueProvider : IDefaultValueProvider<PasswordValidity>
{
    public PasswordValidity GetDefault() => PasswordValidity.Empty;
}