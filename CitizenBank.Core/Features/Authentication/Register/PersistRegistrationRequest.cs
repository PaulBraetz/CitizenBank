namespace CitizenBank.Features.Authentication.Register;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class PersistRegistrationRequestServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IPersistRegistrationRequestService")]
    static PersistRegistrationRequest.Result PersistRegistrationRequest(CitizenName name, HashedPassword password, BioCode bioCode) =>
        throw Exceptions.DefinitionNotSupported<PersistRegistrationRequestServiceDefinition>();
}

public partial record struct PersistRegistrationRequest
{
    [UnionType<CreateSuccess, OverwriteSuccess, Failure>]
    public readonly partial struct Result;
    public readonly struct OverwriteSuccess;
    public readonly struct CreateSuccess;
}
