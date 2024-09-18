namespace CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.CodeAnalysis;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class DeleteRegistrationRequestServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IDeleteRegistrationRequestService")]
    static DeleteRegistrationRequest.Result DeleteRegistrationRequest(CitizenName name) =>
        throw Exceptions.DefinitionNotSupported<DeleteRegistrationRequestServiceDefinition>();
}

public partial record struct DeleteRegistrationRequest
{
    [UnionType<Failure, Success>]
    public readonly partial struct Result;
    public readonly struct Success;
}
