namespace CitizenBank.Features;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class CitizenNameDefaultProvider : IDefaultValueProvider<CitizenName>
{
    public CitizenName GetDefault() => CitizenName.Empty;
}
