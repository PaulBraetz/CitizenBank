namespace CitizenBank.Persistence;

using CitizenBank.Features.Authentication;

using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(Name))]
class RegistrationRequestEntity
{
    public required String Name { get; set; }
    public required HashedPasswordEntity Password { get; set; }
    public required String BioCode { get; set; }
    public RegistrationRequest ToRegistrationRequest() =>
        new(Name: Name,
            Password: Password.ToHashedPassword(),
            BioCode: BioCode);
    public static RegistrationRequestEntity FromRegistrationRequest(RegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new()
        {
            BioCode = request.BioCode,
            Password = HashedPasswordEntity.FromHashedPassword(request.Password),
            Name = request.Name
        };
    }
}
