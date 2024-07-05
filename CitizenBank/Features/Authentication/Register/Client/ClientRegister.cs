namespace CitizenBank.Features.Authentication.Register.Client;

using RhoMicro.CodeAnalysis;
using RhoMicro.ApplicationFramework.Common;
using CitizenBank.Features.Authentication.Register.Server;

partial record struct ClientRegister
{
    [UnionType<CreateSuccess, OverwriteSuccess, Failure>]
    [Relation<ServerRegister.Result>]
    [UnionType<PasswordValidity>(Alias = "ViolatedGuidelines")]
    public readonly partial struct Result;
    public readonly record struct CreateSuccess(BioCode BioCode);
    public readonly record struct OverwriteSuccess(BioCode BioCode);
}
