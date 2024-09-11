namespace CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.CodeAnalysis;

partial record struct PersistRegistration
{
    [UnionType<OverwriteSuccess, Success, Failure>]
    public readonly partial struct Result;
    public readonly struct OverwriteSuccess;
    public readonly struct Success;
}
