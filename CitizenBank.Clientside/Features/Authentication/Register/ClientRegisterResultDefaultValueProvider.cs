namespace CitizenBank.Features.Authentication.Register;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

class OptionalClientRegisterResultDefaultValueProvider : IDefaultValueProvider<Optional<ClientRegister.Result>>
{
    public Optional<ClientRegister.Result> GetDefault() => Optional.None<ClientRegister.Result>();
}
