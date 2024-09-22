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
    static LoadPrehashedPasswordParameters.Result LoadPrehashedPasswordParameters(CitizenName name, PrehashedPasswordParametersSource source) =>
        throw Exceptions.DefinitionNotSupported<LoadPrehashedPasswordParametersServiceDefinition>();
}

public partial record struct LoadPrehashedPasswordParameters
    : IApiRequest<
        LoadPrehashedPasswordParameters,
        LoadPrehashedPasswordParameters.Result,
        LoadPrehashedPasswordParameters.Dto,
        LoadPrehashedPasswordParameters.Result.Dto>
{
    public sealed record Dto(String Name, PrehashedPasswordParametersSource Source) : IApiRequestDto<LoadPrehashedPasswordParameters, Result>
    {
        LoadPrehashedPasswordParameters IApiRequestDto<LoadPrehashedPasswordParameters, Result>.ToRequest() => new(Name, Source);
    }

    [UnionType<PrehashedPasswordParameters, NotFound>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(
#pragma warning disable IDE0280 // Use 'nameof'
            [property: MemberNotNullWhen(true, "Parameters")] Boolean IsSuccess,
#pragma warning restore IDE0280 // Use 'nameof'
            PrehashedPasswordParameters? Parameters) : IApiResultDto<Result>
        {
            Result IApiResultDto<Result>.ToResult() =>
                IsSuccess
                ? Parameters
                : new NotFound();
        }

        Dto IApiResult<Result, Dto>.ToDto() => Match<Dto>(
            onPrehashedPasswordParameters: p => new(true, p),
            onNotFound: _ => new(false, null));
    }
    public readonly struct NotFound;

    Dto IApiRequest<LoadPrehashedPasswordParameters, Result, Dto, Result.Dto>.ToDto() => new(Name, Source);
}