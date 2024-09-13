namespace CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class PasswordMismatchDefaultValueProvider : IDefaultValueProvider<ValidatePassword.Mismatch>
{
    public ValidatePassword.Mismatch GetDefault() => new();
}