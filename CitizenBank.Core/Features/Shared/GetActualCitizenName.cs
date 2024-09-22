namespace CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class GetActualCitizenNameServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IGetActualCitizenNameService")]
    static GetActualCitizenName.Result GetActualCitizenName(CitizenName name) =>
        throw Exceptions.DefinitionNotSupported<GetActualCitizenNameServiceDefinition>();
}

partial record struct GetActualCitizenName
    : IApiRequest<GetActualCitizenName, GetActualCitizenName.Result, GetActualCitizenName.Dto, GetActualCitizenName.Result.Dto>
{
    public sealed record Dto(String Name)
        : IApiRequestDto<GetActualCitizenName, Result>
    {
        GetActualCitizenName IApiRequestDto<GetActualCitizenName, Result>.ToRequest() => new(Name);
    }
    [UnionType<CitizenName, NotFound>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(String Name, Boolean IsSuccess) : IApiResultDto<Result>
        {
            Result IApiResultDto<Result>.ToResult() => IsSuccess ? (CitizenName)Name : new NotFound();
        }

        Dto IApiResult<Result, Dto>.ToDto() => Match<Dto>(
            onCitizenName: n => new(n, true),
            onNotFound: _ => new(String.Empty, false));
    }
    public readonly struct NotFound;
    Dto IApiRequest<GetActualCitizenName, Result, Dto, Result.Dto>.ToDto() => new(Name);
}
