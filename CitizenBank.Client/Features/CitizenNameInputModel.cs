namespace CitizenBank.Features;
using System;

using RhoMicro.ApplicationFramework.Presentation.Models;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class CitizenNameInputModel(IDefaultValueProvider<CitizenName> valueDefaultProvider, IDefaultValueProvider<String> errorDefaultProvider)
    : InputModel<CitizenName, String>(valueDefaultProvider, errorDefaultProvider);
