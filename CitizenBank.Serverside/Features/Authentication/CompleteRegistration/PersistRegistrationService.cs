namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;
using CitizenBank.Persistence;
using CitizenBank.Features.Shared;

partial class PersistRegistrationService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(PersistRegistration), Service = typeof(IPersistRegistrationService))]
    async ValueTask<PersistRegistration.Result> PersistRegistration(CitizenName name, HashedPassword password, CancellationToken ct)
    {
        var existing = await context.FindAsync<RegistrationEntity>([name.Value], ct);
        PersistRegistration.Result result;
        if(existing == null)
        {
            var newRegistrationEntity = new RegistrationEntity()
            {
                Name = name,
                Password = HashedPasswordEntity.FromHashedPassword(password)
            };
            _ = await context.Registrations.AddAsync(newRegistrationEntity, ct);
            result = new PersistRegistration.CreateSuccess();
        } else
        {
            existing.Password = HashedPasswordEntity.FromHashedPassword(password);
            _ = context.Registrations.Update(existing);
            result = new PersistRegistration.OverwriteSuccess();
        }

        _ = await context.SaveChangesAsync(ct);

        return result;
    }
}
