namespace CitizenBank.Features.Authentication.CompleteRegistration;
using RhoMicro.ApplicationFramework.Aspects;
using CitizenBank.Features.Authentication;

sealed partial class PersistRegistrationServiceFake(DbFake db)
{
    [ServiceMethod(ServiceInterfaceName = "IPersistRegistrationService")]
    ValueTask<PersistRegistration.Result> PersistRegistration(CitizenName name, HashedPassword password)
    {
        var newRegistration = new Registration(name, password);
        var createdRegistration = db.Registrations.AddOrUpdate(name, newRegistration, (n,old)=>new Registration(n,password));
        PersistRegistration.Result result = Object.ReferenceEquals(newRegistration, createdRegistration)
            ? new PersistRegistration.Success()
            : new PersistRegistration.OverwriteSuccess();

        return ValueTask.FromResult(result);
    }
}
