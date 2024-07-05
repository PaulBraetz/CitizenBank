namespace CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class ClearPasswordDefaultValueProvider : IDefaultValueProvider<ClearPassword>
{
    public ClearPassword GetDefault() => ClearPassword.Empty;
}
