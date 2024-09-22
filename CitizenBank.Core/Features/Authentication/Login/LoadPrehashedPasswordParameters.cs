namespace CitizenBank.Features.Authentication.Login;

using System.Diagnostics.CodeAnalysis;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class LoadPrehashedPasswordParametersServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "ILoadPrehashedPasswordParametersService")]
    static LoadPrehashedPasswordParameters.Result LoadPrehashedPasswordParameters(CitizenName name, LoginType loginType) =>
        throw Exceptions.DefinitionNotSupported<LoadPrehashedPasswordParametersServiceDefinition>();
}

public partial record struct LoadPrehashedPasswordParameters
    : IApiRequest<
        LoadPrehashedPasswordParameters,
        LoadPrehashedPasswordParameters.Result,
        LoadPrehashedPasswordParameters.Dto,
        LoadPrehashedPasswordParameters.Result.Dto>
{
    public sealed record Dto(String Name, LoginType LoginType) : IApiRequestDto<LoadPrehashedPasswordParameters, Result>
    {
        LoadPrehashedPasswordParameters IApiRequestDto<LoadPrehashedPasswordParameters, Result>.ToRequest() => new(Name, LoginType);
    }

    [UnionType<PrehashedPasswordParameters, Failure>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(
#pragma warning disable IDE0280 // Use 'nameof'
            [property: MemberNotNullWhen(true, "Parameters", "Salt")] Boolean IsSuccess,
#pragma warning restore IDE0280 // Use 'nameof',
            LoginType LoginType,
            String? Salt,
            PrehashedPasswordParameters? Parameters) : IApiResultDto<Result>
        {
            Result IApiResultDto<Result>.ToResult() =>
                IsSuccess
                ? Parameters with { Salt = ImmutableBytes.FromBase64String(Salt) }
                : LoginType == LoginType.Regular
                    ? (Failure)new RegistrationNotFound()
                    : (Failure)new RegistrationRequestNotFound();
        }

        Dto IApiResult<Result, Dto>.ToDto() => Match<Dto>(
            onPrehashedPasswordParameters: p => new(true, 0, p.Salt.ToBase64String(), p),
            onFailure: f =>
                f.Match<Dto>(
                    onRegistrationNotFound: _ => new(false, LoginType.Regular, null, null),
                    onRegistrationRequestNotFound: _ => new(false, LoginType.CompleteRegistration, null, null)));
    }
    [UnionType<RegistrationNotFound, RegistrationRequestNotFound>]
    public readonly partial struct Failure;
    public readonly struct RegistrationNotFound;
    public readonly struct RegistrationRequestNotFound;

    Dto IApiRequest<LoadPrehashedPasswordParameters, Result, Dto, Result.Dto>.ToDto() => new(Name, LoginType);
}