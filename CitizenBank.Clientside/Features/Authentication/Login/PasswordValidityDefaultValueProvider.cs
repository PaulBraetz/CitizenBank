namespace CitizenBank.Features.Authentication.Login;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class PasswordMismatchDefaultValueProvider : IDefaultValueProvider<ValidatePassword.Mismatch>
{
    public ValidatePassword.Mismatch GetDefault() => new();
}