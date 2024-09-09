namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.CodeAnalysis;

partial record struct PersistRegistration
{
    [UnionType<PersistRegistrationOverwriteSuccess, PersistRegistrationSuccess, PersistRegistrationFailure>]
    public readonly partial struct Result;
    public readonly struct PersistRegistrationOverwriteSuccess;
    public readonly struct PersistRegistrationSuccess;
    public readonly struct PersistRegistrationFailure;
}
