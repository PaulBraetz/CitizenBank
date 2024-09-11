namespace CitizenBank.Features.Authentication.Login.Server;

using System.Collections.Immutable;

using CitizenBank.Features.Authentication.CompleteRegistration;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.CodeAnalysis;

partial record struct ServerLogin : IApiRequest<ServerLogin, ServerLogin.Result, ServerLogin.Dto, ServerLogin.Result.Dto>
{
    [UnionType<CompleteRegistration.Failure>(Alias = "CompleteRegistrationFailure")]
    [UnionType<RhoMicro.ApplicationFramework.Common.Failure>(Alias = "GenericFailure")]
    [UnionType<LoadRegistration.DoesNotExist>(Alias = "LoadRegistrationDoesNotExist")]
    [UnionType<ValidatePassword.Mismatch>(Alias = "PasswordMismatch")]
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
                    0 => (Failure)(CompleteRegistration.Failure)new LoadBio.UnknownCitizen(),
                    1 => (Failure)(CompleteRegistration.Failure)new RhoMicro.ApplicationFramework.Common.Failure(Reason),
                    2 => (Failure)(CompleteRegistration.Failure)new ValidatePassword.Mismatch(),
                    3 => (Failure)(CompleteRegistration.Failure)new ValidateBioCode.Mismatch(),

                    4 => (Failure)new LoadRegistration.DoesNotExist(),
                    5 => (Failure)new ValidatePassword.Mismatch(),

                    6 => new Success(),
                    _ => throw new InvalidOperationException("Invalid dto data received.")
                };
        }

        Dto IApiResult<Result, Dto>.ToDto() => new()
        {
            Kind = Match(
                (Failure f) => f.Match(
                    (CompleteRegistration.Failure f) => f.Match(
                        (LoadBio.UnknownCitizen _) => 0,
                        (RhoMicro.ApplicationFramework.Common.Failure _) => 1,
                        (ValidatePassword.Mismatch _) => 2,
                        (ValidateBioCode.Mismatch _) => 3),
                    (RhoMicro.ApplicationFramework.Common.Failure _) => 1,
                    (LoadRegistration.DoesNotExist _) => 4,
                    (ValidatePassword.Mismatch _) => 5),
                (Success _) => 6),
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

    public sealed class Dto : IApiRequestDto<ServerLogin, Result>
    {
        public required String Name { get; set; }
        public required String Password { get; set; }
        ServerLogin IApiRequestDto<ServerLogin, Result>.ToRequest() => new
        (
            Name: Name,
            Password: Convert.FromBase64String(Password).ToImmutableArray()
        );
    }

    Dto IApiRequest<ServerLogin, Result, Dto, Result.Dto>.ToDto() => new()
    {
        Name = Name,
        Password = Convert.ToBase64String(Password.AsImmutableArray_of_Byte.ToArray())
    };
}