namespace CitizenBank.Features.Authentication.Login.Server;

using System.Diagnostics.CodeAnalysis;

using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.CodeAnalysis;

partial record struct LoadPrehashedPasswordParameters
    : IApiRequest<
        LoadPrehashedPasswordParameters,
        LoadPrehashedPasswordParameters.Result,
        LoadPrehashedPasswordParameters.Dto,
        LoadPrehashedPasswordParameters.Result.Dto>
{
    sealed record Dto(String Name, PrehashedPasswordParametersSource Source) : IApiRequestDto<LoadPrehashedPasswordParameters, Result>
    {
        LoadPrehashedPasswordParameters IApiRequestDto<LoadPrehashedPasswordParameters, Result>.ToRequest() => new(Name, Source);
    }

    [UnionType<PrehashedPasswordParameters, NotFound>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(
            [property: MemberNotNullWhen(true, "Parameters")] Boolean IsSuccess,
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