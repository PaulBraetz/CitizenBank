namespace CitizenBank.Features.Authentication.Login.Client;

using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class ClearPasswordInputModel(IDefaultValueProvider<ClearPassword> valueDefaultProvider, IDefaultValueProvider<ValidatePassword.PasswordMismatch> errorDefaultProvider)
    : InputModel<ClearPassword, ValidatePassword.PasswordMismatch>(valueDefaultProvider, errorDefaultProvider)
{

}
