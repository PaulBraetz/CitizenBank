namespace CitizenBank.Features.Shared;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class DoesDoesNotExistDefaultValueProvider : IDefaultValueProvider<DoesCitizenExist.DoesNotExist>
{
    public DoesCitizenExist.DoesNotExist GetDefault() => new();
}