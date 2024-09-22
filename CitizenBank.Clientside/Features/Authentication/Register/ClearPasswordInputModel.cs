namespace CitizenBank.Features.Authentication.Register;
using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class ClearPasswordInputModel(IDefaultValueProvider<ClearPassword> valueDefaultProvider, IDefaultValueProvider<PasswordValidity> errorDefaultProvider)
    : InputModel<ClearPassword, PasswordValidity>(valueDefaultProvider, errorDefaultProvider);
