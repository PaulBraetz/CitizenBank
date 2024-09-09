namespace CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Authentication;

using RhoMicro.CodeAnalysis;

partial record struct LoadBio
{
    [UnionType<Bio, UnknownCitizen>]
    public readonly partial struct Result;
    public readonly struct UnknownCitizen;
}
