namespace CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class CompleteRegistrationServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "ICompleteRegistrationService")]
    static CompleteRegistration.Result CompleteRegistration(RegistrationRequest request, PrehashedPassword password) =>
        throw Exceptions.DefinitionNotSupported<CompleteRegistrationServiceDefinition>();
}

public partial record struct CompleteRegistration
{
    [UnionType<Success, Failure>]
    public readonly partial struct Result;
    [UnionType<
        PersistRegistration.CreateSuccess,
        PersistRegistration.OverwriteSuccess>]
    public readonly partial struct Success;
    [UnionType<LoadBio.UnknownCitizen>]
    [UnionType<RhoMicro.ApplicationFramework.Common.Failure>(Alias = "GenericFailure")]
    [UnionType<ValidatePassword.Mismatch>(Alias = "ValidatePasswordMismatch")]
    [UnionType<ValidateBioCode.Mismatch>(Alias = "ValidateBioCodeMismatch")]
    public readonly partial struct Failure;
}
