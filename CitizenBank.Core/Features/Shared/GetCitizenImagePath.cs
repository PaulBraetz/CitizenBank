namespace CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.CodeAnalysis;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
partial class GetCitizenImagePathServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IGetCitizenImagePathService")]
    static GetCitizenImagePath.Result GetCitizenImagePath(CitizenName name) =>
        throw Exceptions.DefinitionNotSupported<GetCitizenImagePathServiceDefinition>();
}

partial record struct GetCitizenImagePath
    : IApiRequest<GetCitizenImagePath, GetCitizenImagePath.Result, GetCitizenImagePath.Dto, GetCitizenImagePath.Result.Dto>
{
    public sealed record Dto(String ImagePath)
        : IApiRequestDto<GetCitizenImagePath, Result>
    {
        GetCitizenImagePath IApiRequestDto<GetCitizenImagePath, Result>.ToRequest() => new(ImagePath);
    }
    [UnionType<CitizenImagePath, NotFound>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(String ImagePath, Boolean IsSuccess) : IApiResultDto<Result>
        {
            Result IApiResultDto<Result>.ToResult() => IsSuccess ? (CitizenImagePath)ImagePath : new NotFound();
        }

        Dto IApiResult<Result, Dto>.ToDto() => Match<Dto>(
            onCitizenImagePath: n => new(n, true),
            onNotFound: _ => new(String.Empty, false));
    }
    public readonly struct NotFound;
    Dto IApiRequest<GetCitizenImagePath, Result, Dto, Result.Dto>.ToDto() => new(Name);
}
