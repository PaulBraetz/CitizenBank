namespace CitizenBank.Features.Authentication.Login;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class LoadRegistrationRequestServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "ILoadRegistrationRequestService")]
    static LoadRegistrationRequest.Result LoadRegistrationRequest(CitizenName name) =>
        throw Exceptions.DefinitionNotSupported<LoadRegistrationRequestServiceDefinition>();
}

public partial record struct LoadRegistrationRequest
{
    [UnionType<RegistrationRequest, NotFound>]
    public readonly partial struct Result;
    public readonly struct NotFound;
}