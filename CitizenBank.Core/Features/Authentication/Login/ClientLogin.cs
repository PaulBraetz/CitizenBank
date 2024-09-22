namespace CitizenBank.Features.Authentication.Login;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class ClientLoginServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IClientLoginService")]
    static ClientLogin.Result ClientLogin(CitizenName name, ClearPassword password, LoginType loginType) =>
        throw Exceptions.DefinitionNotSupported<ClientLoginServiceDefinition>();
}

public partial record struct ClientLogin
{
    [UnionType<Failure, Success>]
    [UnionType<DoesCitizenExist.DoesNotExist>(Alias = "CitizenDoesNotExist")]
    [UnionType<LoadPrehashedPasswordParameters.Failure>(Alias = "ParametersError")]
    [UnionType<ValidatePassword.Mismatch>(Alias = "PasswordMismatch")]
    public readonly partial struct Result;
    public readonly struct Success;
}