namespace CitizenBank.Features.Authentication.Register.Server;

using System.Collections.Immutable;

using RhoMicro.ApplicationFramework.Common;
using RhoMicro.ApplicationFramework.Common.Abstractions;
using RhoMicro.CodeAnalysis;

partial record struct ServerRegister : IApiRequest<ServerRegister, ServerRegister.Result, ServerRegister.Dto, ServerRegister.Result.Dto>
{
    [UnionType<CreateSuccess, OverwriteSuccess, Failure>]
    public readonly partial struct Result : IApiResult<Result, Result.Dto>
    {
        public sealed class Dto : IApiResultDto<Result>
        {
            public Int32 Kind { get; set; }
            public String? BioCode { get; set; }

            Result IApiResultDto<Result>.ToResult() => Kind switch
            {
                0 => new CreateSuccess(new BioCode(BioCode ?? String.Empty)),
                1 => new OverwriteSuccess(new BioCode(BioCode ?? String.Empty)),
                2 => new Failure(),
                _ => throw new InvalidOperationException("Invalid dto data received.")
            };
        }
        Dto IApiResult<Result, Dto>.ToDto() => new()
        {
            Kind = Match(
                onCreateSuccess: _ => 0,
                onOverwriteSuccess: _ => 1,
                onFailure: _ => 2),
            BioCode = Match<String?>(
                onCreateSuccess: s => s.BioCode.Value,
                onOverwriteSuccess: s => s.BioCode.Value,
                onFailure: _ => null)
        };
    }
    public readonly record struct CreateSuccess(BioCode BioCode);
    public readonly record struct OverwriteSuccess(BioCode BioCode);
    public sealed class Dto : IApiRequestDto<ServerRegister, Result>
    {
        public required String Name { get; set; }
        public required String Password { get; set; }
        ServerRegister IApiRequestDto<ServerRegister, Result>.ToRequest() => new
        (
            Name: Name,
            Password: Convert.FromBase64String(Password).ToImmutableArray()
        );
    }
    Dto IApiRequest<ServerRegister, Result, Dto, Result.Dto>.ToDto() => new()
    {
        Name = Name,
        Password = Convert.ToBase64String(Password.AsImmutableArray_of_Byte.ToArray())
    };
}
