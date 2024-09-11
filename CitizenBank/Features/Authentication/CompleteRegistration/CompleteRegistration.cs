namespace CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Authentication;

using RhoMicro.CodeAnalysis;

partial record struct CompleteRegistration
{
    [UnionType<Success, Failure>]
    public readonly partial struct Result;
    [UnionType<
        PersistRegistration.Success,
        PersistRegistration.OverwriteSuccess>]
    public readonly partial struct Success;
    [UnionType<LoadBio.UnknownCitizen>]
    [UnionType<RhoMicro.ApplicationFramework.Common.Failure>(Alias = "GenericFailure")]
    [UnionType<ValidatePassword.Mismatch>(Alias = "ValidatePasswordMismatch")]
    [UnionType<ValidateBioCode.Mismatch>(Alias = "ValidateBioCodeMismatch")]
    public readonly partial struct Failure;
}
