namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

public partial record struct ValidateBioCode
{
    [UnionType<Success, LoadBio.UnknownCitizen, BioCodeMismatch>]
    public readonly partial struct Result;
    public readonly struct BioCodeMismatch;
}
