namespace CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Authentication;

using RhoMicro.CodeAnalysis;

partial record struct CompleteRegistration
{
    [UnionType<Success, Failure>]
    public readonly partial struct Result;
    [UnionType<
        PersistRegistration.PersistRegistrationSuccess,
        PersistRegistration.PersistRegistrationOverwriteSuccess>]
    public readonly partial struct Success;
    [UnionType<
        ValidatePassword.PasswordMismatch,
        LoadBio.UnknownCitizen,
        ValidateBioCode.BioCodeMismatch,
        DeleteRegistrationRequest.DeleteRegistrationRequestFailure,
        PersistRegistration.PersistRegistrationFailure>]
    public readonly partial struct Failure;
}
