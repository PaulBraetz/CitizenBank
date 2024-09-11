namespace CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.CodeAnalysis;

public partial record struct ValidateBioCode
{
    [UnionType<Success, LoadBio.UnknownCitizen, Mismatch>]
    public readonly partial struct Result;
    public readonly struct Mismatch;
    public readonly struct Success;
}
