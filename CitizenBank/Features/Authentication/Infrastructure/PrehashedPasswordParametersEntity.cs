namespace CitizenBank.Features.Authentication.Entities;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

[Owned]
public class PrehashedPasswordParametersEntity
{
    public required Byte[] Salt { get; init; }
    public required Int32 HashSize { get; set; }
    public required KeyDerivationPrf Prf { get; set; }
    public required Int32 Iterations { get; set; }
    public PrehashedPasswordParameters ToPrehashedPasswordParameters() =>
        new(Salt: [.. Salt],
            HashSize: HashSize,
            Prf: Prf,
            Iterations: Iterations);
    public static PrehashedPasswordParametersEntity FromPrehashedPasswordParameters(PrehashedPasswordParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return new()
        {
            Salt = [.. parameters.Salt],
            HashSize = parameters.HashSize,
            Prf = parameters.Prf,
            Iterations = parameters.Iterations
        };
    }
}
