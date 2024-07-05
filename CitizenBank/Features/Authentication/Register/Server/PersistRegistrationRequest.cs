namespace CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

partial record struct PersistRegistrationRequest
{
    [UnionType<CreateSuccess, OverwriteSuccess, Failure>]
    public readonly partial struct Result;
}
