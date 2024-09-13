namespace CitizenBank.Features.Authentication.Register.Server;

using CitizenBank.Features.Authentication.Infrastructure;

using Microsoft.EntityFrameworkCore;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class PersistRegistrationRequestService(CitizenBankContext context)
{
    [ServiceMethod]
    async ValueTask<PersistRegistrationRequest.Result> PersistRegistrationRequest(
        CitizenName name,
        HashedPassword password,
        BioCode bioCode,
        CancellationToken ct)
    {
        var newRequestEntity = new RegistrationRequestEntity()
        {
            Name = name,
            Password = HashedPasswordEntity.FromHashedPassword(password),
            BioCode = bioCode
        };

        var tracker = context.RegistrationRequests.Update(newRequestEntity);
        _ = await context.SaveChangesAsync(ct);

        PersistRegistrationRequest.Result result = tracker.State switch
        {
            EntityState.Added => new PersistRegistrationRequest.CreateSuccess(),
            EntityState.Modified => new PersistRegistrationRequest.OverwriteSuccess(),
            _ => throw new InvalidOperationException("Unexpected registration request entity state encountered after update.")
        };

        return result;
    }
}
