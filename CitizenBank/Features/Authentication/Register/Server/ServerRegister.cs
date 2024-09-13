namespace CitizenBank.Features.Authentication.Register.Server;

using System.Collections.Immutable;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.CodeAnalysis;

partial record struct ServerRegister : IApiRequest<ServerRegister, ServerRegister.Result, ServerRegister.Dto, ServerRegister.Result.Dto>
{
    public sealed record Dto(String Name, String Password, PrehashedPasswordParameters Parameters) : IApiRequestDto<ServerRegister, Result>
    {
        ServerRegister IApiRequestDto<ServerRegister, Result>.ToRequest() => new
        (
            Name: Name,
            Password: new PrehashedPassword(
                Bytes: [.. Convert.FromBase64String(Password)],
                Parameters: Parameters)
        );
    }
    [UnionType<CreateSuccess, OverwriteSuccess, ValidatePrehashedPasswordParameters.Insecure, Failure>]
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
                _ => throw new InvalidOperationException("Invalid dto data received.")
            };
        }
        Dto IApiResult<Result, Dto>.ToDto() => new
        (
            Kind: Match(
                onCreateSuccess: _ => 0,
                onOverwriteSuccess: _ => 1,
                onInsecure: _ => 2,
                onFailure: _ => 3),
            BioCode: Match<String?>(
                onCreateSuccess: s => s.BioCode.Value,
                onOverwriteSuccess: s => s.BioCode.Value,
                onInsecure: _ => null,
                onFailure: _ => null),
            Reason: Match<String?>(
                onCreateSuccess: _ => null,
                onOverwriteSuccess: _ => null,
                onInsecure: _ => null,
                onFailure: f => f.Reason.TryAsSome(out var r) ? r : "")
        );
    }
    public readonly record struct CreateSuccess(BioCode BioCode);
    public readonly record struct OverwriteSuccess(BioCode BioCode);
    Dto IApiRequest<ServerRegister, Result, Dto, Result.Dto>.ToDto() => new
    (
        Name: Name,
        Password: Convert.ToBase64String(Password.Bytes.ToArray()),
        Parameters: Password.Parameters
    );
}
