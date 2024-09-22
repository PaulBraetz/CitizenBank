namespace CitizenBank.Features.Authentication.Login;
using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class LoginTypeInputModel(
    IDefaultValueProvider<LoginType> valueDefaultProvider, 
    IDefaultValueProvider<LoadPrehashedPasswordParameters.Failure> errorDefaultProvider) : 
    InputModel<LoginType, LoadPrehashedPasswordParameters.Failure>(valueDefaultProvider, errorDefaultProvider)
{}