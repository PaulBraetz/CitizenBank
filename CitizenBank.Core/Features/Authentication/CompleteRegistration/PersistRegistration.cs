namespace CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class PersistRegistrationServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IPersistRegistrationService")]
    static PersistRegistration.Result PersistRegistration(CitizenName name, HashedPassword password) =>
        throw Exceptions.DefinitionNotSupported<PersistRegistrationServiceDefinition>();
}

public partial record struct PersistRegistration
{
    [UnionType<OverwriteSuccess, CreateSuccess, Failure>]
    public readonly partial struct Result;
    public readonly struct OverwriteSuccess;
    public readonly struct CreateSuccess;
}
