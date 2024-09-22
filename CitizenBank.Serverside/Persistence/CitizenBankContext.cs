namespace CitizenBank.Persistence;

using Microsoft.EntityFrameworkCore;

sealed class CitizenBankContext(DbContextOptions<CitizenBankContext> options) : DbContext(options)
{
    public DbSet<RegistrationRequestEntity> RegistrationRequests { get; private set; }
    public DbSet<RegistrationEntity> Registrations { get; private set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.Owned<HashedPasswordEntity>();
        _ = modelBuilder.Owned<PasswordParameterDataEntity>();
        _ = modelBuilder.Owned<PasswordParameterNumericsEntity>();
        _ = modelBuilder.Owned<PasswordParametersEntity>();
        _ = modelBuilder.Owned<PrehashedPasswordParametersEntity>();
        _ = modelBuilder.Entity<RegistrationEntity>().HasKey(e => e.Name);
        _ = modelBuilder.Entity<RegistrationRequestEntity>().HasKey(e => e.Name);
    }
}
