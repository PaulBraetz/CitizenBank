namespace CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Register.Client;

using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class ClearPasswordInputModel(IDefaultValueProvider<ClearPassword> valueDefaultProvider, IDefaultValueProvider<PasswordValidity> errorDefaultProvider)
    : InputModel<ClearPassword, PasswordValidity>(valueDefaultProvider, errorDefaultProvider)
{ 

}
