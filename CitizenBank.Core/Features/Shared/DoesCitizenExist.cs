namespace CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class DoesCitizenExistServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IDoesCitizenExistService")]
    ValueTask<DoesCitizenExist.Result> DoesCitizenExist(CitizenName name) =>
            throw Exceptions.DefinitionNotSupported<DoesCitizenExistServiceDefinition>();
}

partial record struct DoesCitizenExist :
    IApiRequest<DoesCitizenExist, DoesCitizenExist.Result, DoesCitizenExist.Dto, DoesCitizenExist.Result.Dto>
{
    sealed record Dto(String Name) :
        IApiRequestDto<DoesCitizenExist, Result>
    {
        DoesCitizenExist IApiRequestDto<DoesCitizenExist, Result>.ToRequest() => new(Name);
    }
    [UnionType<Success, DoesNotExist>]
    public readonly partial struct Result :
        IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(Boolean IsDoesNotExist) : IApiResultDto<Result>
        {
            Result IApiResultDto<Result>.ToResult() =>
                IsDoesNotExist
                ? new DoesNotExist()
                : new Success();
        }
        Dto IApiResult<Result, Dto>.ToDto() => new(IsDoesNotExist);
    }
    public readonly struct DoesNotExist;
    public readonly struct Success;

    Dto IApiRequest<DoesCitizenExist, Result, Dto, Result.Dto>.ToDto() => new(Name);
}