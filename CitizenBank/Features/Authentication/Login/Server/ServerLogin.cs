namespace CitizenBank.Features.Authentication.Login.Server;

using CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

partial record struct ServerLogin
{
    [UnionType<Failure, Success>]
    public readonly partial struct Result;
    [UnionType<CompleteRegistration.Failure>(Alias = "CompleteRegistrationFailure")]
    [UnionType<LoadRegistration.DoesNotExist>(Alias = "RegistrationDoesNotExist")]
    [UnionType<ValidateBioCode.BioCodeMismatch, ValidatePassword.PasswordMismatch>]
    public readonly partial struct Failure;
}