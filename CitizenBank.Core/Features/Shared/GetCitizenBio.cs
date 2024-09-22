namespace CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.CodeAnalysis;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class GetCitizenBioServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IGetCitizenBioService")]
    static GetCitizenBio.Result GetCitizenBio(CitizenName name) =>
        throw Exceptions.DefinitionNotSupported<GetCitizenBioServiceDefinition>();
}

partial record struct GetCitizenBio
    : IApiRequest<GetCitizenBio, GetCitizenBio.Result, GetCitizenBio.Dto, GetCitizenBio.Result.Dto>
{
    public sealed record Dto(String Name)
        : IApiRequestDto<GetCitizenBio, Result>
    {
        GetCitizenBio IApiRequestDto<GetCitizenBio, Result>.ToRequest() => new(Name);
    }
    [UnionType<CitizenBio, NotFound>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(String Bio, Boolean IsSuccess) : IApiResultDto<Result>
        {
            Result IApiResultDto<Result>.ToResult() => IsSuccess ? (CitizenBio)Bio : new NotFound();
        }

        Dto IApiResult<Result, Dto>.ToDto() => Match<Dto>(
            onCitizenBio: n => new(n, true),
            onNotFound: _ => new(String.Empty, false));
    }
    public readonly struct NotFound;
    Dto IApiRequest<GetCitizenBio, Result, Dto, Result.Dto>.ToDto() => new(Name);
}
