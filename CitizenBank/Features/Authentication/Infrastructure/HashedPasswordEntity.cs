namespace CitizenBank.Features.Authentication.Infrastructure;

using CitizenBank.Features.Authentication.Entities;

using Microsoft.EntityFrameworkCore;

[Owned]
public class HashedPasswordEntity
{
    public required Byte[] Digest { get; set; }
    public required PasswordParametersEntity Parameters { get; set; }
    public required PrehashedPasswordParametersEntity PrehashedPasswordParameters { get; set; }
    public HashedPassword ToHashedPassword() =>
        new(Digest: [.. Digest],
            Parameters: Parameters.ToPasswordParameters(),
            PrehashedPasswordParameters: PrehashedPasswordParameters.ToPrehashedPasswordParameters());
    public static HashedPasswordEntity FromHashedPassword(HashedPassword password) =>
        new()
        {
            Digest = [.. password.Digest],
            Parameters = PasswordParametersEntity.FromPasswordParameters(password.Parameters),
            PrehashedPasswordParameters = PrehashedPasswordParametersEntity.FromPrehashedPasswordParameters(password.PrehashedPasswordParameters)
        };
}
