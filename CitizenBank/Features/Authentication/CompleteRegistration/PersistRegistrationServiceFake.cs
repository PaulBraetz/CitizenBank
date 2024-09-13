namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;
using RhoMicro.ApplicationFramework.Composition;
using CitizenBank.Features.Authentication.Infrastructure;
using Microsoft.EntityFrameworkCore;

sealed partial class PersistRegistrationService(CitizenBankContext context)
{
    [ServiceMethod]
    async ValueTask<PersistRegistration.Result> PersistRegistration(CitizenName name, HashedPassword password, CancellationToken ct)
    {
        var newRegistration = new RegistrationEntity()
        {
            Name = name,
            Password = HashedPasswordEntity.FromHashedPassword(password)
        };

        var tracker = context.Registrations.Update(newRegistration);
        _ = await context.SaveChangesAsync(ct);

        PersistRegistration.Result result = tracker.State switch
        {
            EntityState.Added => new PersistRegistration.Success(),
            EntityState.Modified => new PersistRegistration.OverwriteSuccess(),
            _ => throw new InvalidOperationException("Unexpected registration entity state encountered after update.")
        };

        return result;
    }
}
