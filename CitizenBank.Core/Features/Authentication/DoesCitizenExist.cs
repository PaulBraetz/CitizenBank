namespace CitizenBank.Features.Authentication;

using System.Globalization;
using System.Xml.Linq;

using CitizenBank.Infrastructure;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.CodeAnalysis;

partial class DoesCitizenExistServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IDoesCitizenExistService")]
    async ValueTask<DoesCitizenExist.Result> DoesCitizenExist(CitizenName name) =>
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

    Dto IApiRequest<DoesCitizenExist, Result, Dto, Result.Dto>.ToDto() => new(Name);
}