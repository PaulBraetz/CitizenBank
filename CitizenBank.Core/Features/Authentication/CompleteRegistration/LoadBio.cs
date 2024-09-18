namespace CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Authentication;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class LoadBioServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "ILoadBioService")]
    static LoadBio.Result LoadBio(CitizenName name) =>
        throw Exceptions.DefinitionNotSupported<LoadBioServiceDefinition>();
}

public partial record struct LoadBio
{
    [UnionType<Bio, UnknownCitizen>]
    public readonly partial struct Result;
    public readonly struct UnknownCitizen;
}
