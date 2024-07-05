namespace CitizenBank.Features.Authentication.Register.Client;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Presentation.Models.Abstractions;

sealed class OptionalClientRegisterResultDefaultValueProvider : IDefaultValueProvider<Optional<ClientRegister.Result>>
{
    public Optional<ClientRegister.Result> GetDefault() => Optional<ClientRegister.Result>.None();
}
