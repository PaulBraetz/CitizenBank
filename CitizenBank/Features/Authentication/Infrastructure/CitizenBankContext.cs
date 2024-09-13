namespace CitizenBank.Features.Authentication.Infrastructure;

using Microsoft.EntityFrameworkCore;

public sealed class CitizenBankContext : DbContext
{
    public DbSet<RegistrationRequestEntity> RegistrationRequests { get; private set; }
    public DbSet<RegistrationEntity> Registrations { get; private set; }
}
