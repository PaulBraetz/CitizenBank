namespace CitizenBank.Features.Authentication.Register;

using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class ServerRegisterDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IServerRegisterService")]
    static ServerRegister.Result ServerRegister(CitizenName name, PrehashedPassword password) =>
        throw Exceptions.DefinitionNotSupported<ServerRegisterDefinition>();
}

public partial record struct ServerRegister : IApiRequest<ServerRegister, ServerRegister.Result, ServerRegister.Dto, ServerRegister.Result.Dto>
{
    public sealed record Dto(String Name, String Password, String Salt, PrehashedPasswordParameters Parameters) : IApiRequestDto<ServerRegister, Result>
    {
        ServerRegister IApiRequestDto<ServerRegister, Result>.ToRequest() => new
        (
            Name: Name,
            Password: new PrehashedPassword(
                Digest: ImmutableBytes.FromBase64String(Password),
                Parameters: Parameters with { Salt = ImmutableBytes.FromBase64String(Salt) })
        );
    }
    [UnionType<CreateSuccess, OverwriteSuccess, ValidatePrehashedPasswordParameters.Insecure, DoesCitizenExist.DoesNotExist, Failure>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed record Dto(Int32 Kind, String? BioCode, String? Reason) : IApiResultDto<Result>
        {
            Result IApiResultDto<Result>.ToResult() => Kind switch
            {
                0 => new CreateSuccess(new BioCode(BioCode ?? String.Empty)),
                1 => new OverwriteSuccess(new BioCode(BioCode ?? String.Empty)),
                2 => new ValidatePrehashedPasswordParameters.Insecure(),
                3 => new Failure(Reason ?? ""),
                4 => new DoesCitizenExist.DoesNotExist(),
                _ => throw new InvalidOperationException("Invalid dto data received.")
            };
        }
        Dto IApiResult<Result, Dto>.ToDto() => new
        (
            Kind: Match(
                onCreateSuccess: _ => 0,
                onOverwriteSuccess: _ => 1,
                onInsecure: _ => 2,
                onFailure: _ => 3,
                onDoesNotExist: _ => 4),
            BioCode: Match<String?>(
                onCreateSuccess: s => s.BioCode.Value,
                onOverwriteSuccess: s => s.BioCode.Value,
                onInsecure: _ => null,
                onFailure: _ => null,
                onDoesNotExist: _ => null),
            Reason: Match<String?>(
                onCreateSuccess: _ => null,
                onOverwriteSuccess: _ => null,
                onInsecure: _ => null,
                onDoesNotExist: _ => null,
                onFailure: f => f.Reason.TryAsSome(out var r) ? r : "")
        );
    }
    public readonly record struct CreateSuccess(BioCode BioCode);
    public readonly record struct OverwriteSuccess(BioCode BioCode);
    Dto IApiRequest<ServerRegister, Result, Dto, Result.Dto>.ToDto() => new
    (
        Name: Name,
        Password: Password.Digest.ToBase64String(),
        Parameters: Password.Parameters,
        Salt: Password.Parameters.Salt.ToBase64String()
    );
}
