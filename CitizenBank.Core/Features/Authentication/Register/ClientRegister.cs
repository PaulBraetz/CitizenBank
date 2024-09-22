namespace CitizenBank.Features.Authentication.Register;

using RhoMicro.CodeAnalysis;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Shared;

[FakeService]
partial class ClientRegisterServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IClientRegisterService")]
    static ClientRegister.Result ClientRegister(CitizenName name, ClearPassword password) =>
        throw Exceptions.DefinitionNotSupported<ClientRegisterServiceDefinition>();
}

public partial record struct ClientRegister
{
    [UnionType<CreateSuccess, OverwriteSuccess, Failure>]
    [Relation<ServerRegister.Result>]
    [UnionType<PasswordValidity>(Alias = "ViolatedGuidelines")]
    [UnionType<DoesCitizenExist.DoesNotExist>(Alias = "CitizenDoesNotExist")]
    public readonly partial struct Result;
    public readonly record struct CreateSuccess(BioCode BioCode);
    public readonly record struct OverwriteSuccess(BioCode BioCode);
}
