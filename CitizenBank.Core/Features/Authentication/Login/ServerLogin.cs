namespace CitizenBank.Features.Authentication.Login;

using CitizenBank.Features.Authentication.CompleteRegistration;
using CitizenBank.Features.Shared;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.ApplicationFramework.Composition;
using RhoMicro.CodeAnalysis;

[FakeService]
partial class ServerLoginServiceDefinition
{
    [ServiceMethod(ServiceInterfaceName = "IServerLoginService")]
    static ServerLogin.Result ServerLogin(CitizenName name, PrehashedPassword password) =>
        throw Exceptions.DefinitionNotSupported<ServerLoginServiceDefinition>();
}

public partial record struct ServerLogin
    : IApiRequest<ServerLogin, ServerLogin.Result, ServerLogin.Dto, ServerLogin.Result.Dto>
{
    [UnionType<CompleteRegistration.Failure>(Alias = "CompleteRegistrationFailure")]
    [UnionType<RhoMicro.ApplicationFramework.Common.Failure>(Alias = "GenericFailure")]
    [UnionType<LoadRegistration.DoesNotExist>(Alias = "LoadRegistrationDoesNotExist")]
    [UnionType<ValidatePassword.Mismatch>(Alias = "PasswordMismatch")]
    [UnionType<ValidatePrehashedPasswordParameters.Insecure>(Alias = "InsecurePrehashParameters")]
    [UnionType<DoesCitizenExist.DoesNotExist>(Alias = "CitizenDoesNotExist")]
    public readonly partial struct Failure;
    public readonly struct Success;
    [UnionType<Failure, Success>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed class Dto : IApiResultDto<Result>
        {
            public Int32 Kind { get; set; }
            public String Reason { get; set; } = String.Empty;

            Result IApiResultDto<Result>.ToResult() =>
                Kind switch
                {
                    0 => (Failure)(CompleteRegistration.Failure)new GetCitizenBio.NotFound(),
                    1 => (Failure)(CompleteRegistration.Failure)new RhoMicro.ApplicationFramework.Common.Failure(Reason),
                    2 => (Failure)(CompleteRegistration.Failure)new ValidatePassword.Mismatch(),
                    3 => (Failure)(CompleteRegistration.Failure)new ValidateBioCode.Mismatch(),

                    4 => (Failure)new LoadRegistration.DoesNotExist(),
                    5 => (Failure)new ValidatePassword.Mismatch(),
                    6 => (Failure)new ValidatePrehashedPasswordParameters.Insecure(),
                    8 => (Failure)new DoesCitizenExist.DoesNotExist(),

                    7 => new Success(),
                    _ => throw new InvalidOperationException("Invalid dto data received.")
                };
        }

        Dto IApiResult<Result, Dto>.ToDto() => new()
        {
            Kind = Match(
                (Failure f) => f.Match(
                    (CompleteRegistration.Failure f) => f.Match(
                        (GetCitizenBio.NotFound _) => 0,
                        (RhoMicro.ApplicationFramework.Common.Failure _) => 1,
                        (ValidatePassword.Mismatch _) => 2,
                        (ValidateBioCode.Mismatch _) => 3),
                    (RhoMicro.ApplicationFramework.Common.Failure _) => 1,
                    (LoadRegistration.DoesNotExist _) => 4,
                    (ValidatePassword.Mismatch _) => 5,
                    (ValidatePrehashedPasswordParameters.Insecure _) => 6,
                    (DoesCitizenExist.DoesNotExist _) => 8),
                (Success _) => 7),
            Reason =
                TryAsFailure(out var f0)
                ? f0.TryAsGenericFailure(out var f1) && f1.Reason.TryAsSome(out var r0)
                    ? r0
                    : f0.TryAsCompleteRegistrationFailure(out var f2) && f2.TryAsGenericFailure(out var f3) && f3.Reason.TryAsSome(out var r1)
                        ? r1
                        : String.Empty
                : String.Empty
        };
    }

    public sealed record Dto(String Name, String Password, String Salt, PrehashedPasswordParameters Parameters) : IApiRequestDto<ServerLogin, Result>
    {
        ServerLogin IApiRequestDto<ServerLogin, Result>.ToRequest() => new
        (
            Name: Name,
            Password: new(
                Digest: ImmutableBytes.FromBase64String(Password),
                Parameters: Parameters with { Salt = ImmutableBytes.FromBase64String(Salt) })
        );
    }

    Dto IApiRequest<ServerLogin, Result, Dto, Result.Dto>.ToDto() => new(
        Name: Name,
        Password: Password.Digest.ToBase64String(),
        Salt: Password.Parameters.Salt.ToBase64String(),
        Parameters: Password.Parameters);
}