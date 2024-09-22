namespace CitizenBank.Features.Authentication.Login;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class LoadRegistrationServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "ILoadRegistrationService")]
    static LoadRegistration.Result LoadRegistration(CitizenName name) =>
        throw Exceptions.DefinitionNotSupported<LoadRegistrationServiceDefinition>();
}

public partial record struct LoadRegistration
{
    [UnionType<Registration, DoesNotExist>]
    public readonly partial struct Result;
    public readonly struct DoesNotExist;
}
