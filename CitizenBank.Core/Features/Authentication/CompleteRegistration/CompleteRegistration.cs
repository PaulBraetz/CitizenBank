namespace CitizenBank.Features.Authentication.CompleteRegistration;

using CitizenBank.Features.Shared;

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

/// <summary>
/// Represents a request to complete a registration request.
/// </summary>
public partial record struct CompleteRegistration
{
    /// <summary>
    /// Represents the result of requesting a registration request completion.
    /// </summary>
    [UnionType<Success, Failure>]
    public readonly partial struct Result;
    /// <summary>
    /// Represents a successful registration request completion.
    /// </summary>
    [UnionType<
        PersistRegistration.CreateSuccess,
        PersistRegistration.OverwriteSuccess>]
    public readonly partial struct Success;
    /// <summary>
    /// Represents a failed registration request completion.
    /// </summary>
    [UnionType<GetCitizenBio.NotFound>(Alias = "UnknownCitizen")]
    [UnionType<RhoMicro.ApplicationFramework.Common.Failure>(Alias = "GenericFailure")]
    [UnionType<ValidatePassword.Mismatch>(Alias = "ValidatePasswordMismatch")]
    [UnionType<ValidateBioCode.Mismatch>(Alias = "ValidateBioCodeMismatch")]
    public readonly partial struct Failure;
}
