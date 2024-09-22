namespace CitizenBank.Features.Shared;
using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class CitizenNameInputModel(IDefaultValueProvider<CitizenName> valueDefaultProvider, IDefaultValueProvider<DoesCitizenExist.DoesNotExist> errorDefaultProvider)
    : InputModel<CitizenName, DoesCitizenExist.DoesNotExist>(valueDefaultProvider, errorDefaultProvider);
