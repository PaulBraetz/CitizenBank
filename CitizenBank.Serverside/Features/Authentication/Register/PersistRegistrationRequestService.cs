namespace CitizenBank.Features.Authentication.Register;

using CitizenBank.Features.Shared;
using CitizenBank.Persistence;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class PersistRegistrationRequestService(CitizenBankContext context)
{
    [ServiceMethodImplementation(Request = typeof(PersistRegistrationRequest), Service = typeof(IPersistRegistrationRequestService))]
    async ValueTask<PersistRegistrationRequest.Result> PersistRegistrationRequest(
        CitizenName name,
        HashedPassword password,
        BioCode bioCode,
        CancellationToken ct)
    {
        var existing = await context.FindAsync<RegistrationRequestEntity>([name.Value], ct);
        PersistRegistrationRequest.Result result;
        if(existing == null)
        {
            var newRequestEntity = new RegistrationRequestEntity()
            {
                Name = name,
                Password = HashedPasswordEntity.FromHashedPassword(password),
                BioCode = bioCode
            };
            _ = await context.RegistrationRequests.AddAsync(newRequestEntity, ct);
            //_ = context.RegistrationRequests.Add(newRequestEntity);
            result = new PersistRegistrationRequest.CreateSuccess();
        } else
        {
            existing.Password = HashedPasswordEntity.FromHashedPassword(password);
            existing.BioCode = bioCode;
            _ = context.RegistrationRequests.Update(existing);
            result = new PersistRegistrationRequest.OverwriteSuccess();
        }

        _ = await context.SaveChangesAsync(ct);

        return result;
    }
}
