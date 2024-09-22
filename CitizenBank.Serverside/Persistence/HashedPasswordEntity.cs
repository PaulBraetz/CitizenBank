namespace CitizenBank.Persistence;

using CitizenBank.Features.Authentication;

class HashedPasswordEntity
{
    public required String Digest { get; set; }
    public required PasswordParametersEntity Parameters { get; set; }
    public required PrehashedPasswordParametersEntity PrehashedPasswordParameters { get; set; }
    public HashedPassword ToHashedPassword() =>
        new(Digest: Features.Authentication.ImmutableBytes.FromBase64String(Digest),
            Parameters: Parameters.ToPasswordParameters(),
            PrehashedPasswordParameters: PrehashedPasswordParameters.ToPrehashedPasswordParameters());
    public static HashedPasswordEntity FromHashedPassword(HashedPassword password) =>
        new()
        {
            Digest = password.Digest.ToBase64String(),
            Parameters = PasswordParametersEntity.FromPasswordParameters(password.Parameters),
            PrehashedPasswordParameters = PrehashedPasswordParametersEntity.FromPrehashedPasswordParameters(password.PrehashedPasswordParameters)
        };
}
