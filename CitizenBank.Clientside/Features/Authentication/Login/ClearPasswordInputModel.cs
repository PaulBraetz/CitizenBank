namespace CitizenBank.Features.Authentication.Login;

using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class ClearPasswordInputModel(IDefaultValueProvider<ClearPassword> valueDefaultProvider, IDefaultValueProvider<ValidatePassword.Mismatch> errorDefaultProvider)
    : InputModel<ClearPassword, ValidatePassword.Mismatch>(valueDefaultProvider, errorDefaultProvider);
