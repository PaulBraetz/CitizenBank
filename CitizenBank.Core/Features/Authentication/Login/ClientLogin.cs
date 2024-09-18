namespace CitizenBank.Features.Authentication.Login;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class ClientLoginServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IClientLoginService")]
    static ClientLogin.Result ClientLogin(CitizenName name, ClearPassword password, PrehashedPasswordParametersSource parametersSource) =>
        throw Exceptions.DefinitionNotSupported<ClientLoginServiceDefinition>();
}

public partial record struct ClientLogin
{
    [UnionType<ValidatePassword.Mismatch, Failure, Success>]
    public readonly partial struct Result;
    public readonly struct Success;
}