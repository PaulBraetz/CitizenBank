namespace CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class HashPasswordServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IHashPasswordService")]
    HashedPassword HashPassword(PrehashedPassword password, PasswordParameters parameters) =>
        throw Exceptions.DefinitionNotSupported<HashPasswordServiceDefinition>();
}
