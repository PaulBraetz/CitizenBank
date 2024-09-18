namespace CitizenBank.Features;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class CitizenNameDefaultProvider : IDefaultValueProvider<CitizenName>
{
    public CitizenName GetDefault() => CitizenName.Empty;
}
