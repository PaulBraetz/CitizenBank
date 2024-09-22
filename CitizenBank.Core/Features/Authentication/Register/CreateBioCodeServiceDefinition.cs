namespace CitizenBank.Features.Authentication.Register;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class CreateBioCodeServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "ICreateBioCodeService")]
    static BioCode CreateBioCode(CitizenName name, CancellationToken ct) =>
        throw Exceptions.DefinitionNotSupported<CreateBioCodeServiceDefinition>();
}
