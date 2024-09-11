namespace CitizenBank.Features.Authentication.Login.Client;

using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class ClearPasswordInputModel(IDefaultValueProvider<ClearPassword> valueDefaultProvider, IDefaultValueProvider<ValidatePassword.Mismatch> errorDefaultProvider)
    : InputModel<ClearPassword, ValidatePassword.Mismatch>(valueDefaultProvider, errorDefaultProvider)
{

}
