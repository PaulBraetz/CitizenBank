namespace CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Register.Client;

using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class PasswordMismatchDefaultValueProvider : IDefaultValueProvider<ValidatePassword.PasswordMismatch>
{
    public ValidatePassword.PasswordMismatch GetDefault() => new();
}