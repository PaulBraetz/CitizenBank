namespace CitizenBank.Features.Authentication.Register;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class CreatePasswordParametersServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "ICreatePasswordParametersService")]
    static PasswordParameters CreatePasswordParameters() =>
        throw Exceptions.DefinitionNotSupported<CreatePasswordParametersServiceDefinition>();
}
